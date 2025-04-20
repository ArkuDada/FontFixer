/*
 * ThaiFontFixerWindow.cs
 *
 * This file defines the `ThaiFontFixerWindow` class, which provides a Unity Editor window for managing and fixing Thai font glyphs.
 * It includes functionality for loading font assets, managing glyph combinations, and saving/loading configurations.
 *
 * Classes:
 * - `ThaiFontFixerWindow`: Represents the main editor window for managing Thai font glyph adjustments.
 *
 * Author: Pada Cherdchoothai
 * Created: 2023-10-06
 *
 * Usage:
 * - Open the window via the Unity menu: `Window > Thai Font Fixer`.
 * - Use the interface to load a font asset, adjust glyph combinations, and save/load presets.
 * - Use the "Fix Glyphs" button to apply adjustments to the selected font asset.
 */

using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Editor.ThaiFontFixer
{
    public class ThaiFontFixerWindow : EditorWindow
    {
        static string m_CachePath => Application.persistentDataPath + "/ThaiFontFixerCache.json";

        Button m_FixGlyphsButton;
        ObjectField m_FontAssetField;

        TMP_FontAsset m_FontAsset;

        List<ThaiGlyphGroupPair> m_Combinations = new List<ThaiGlyphGroupPair>();
        private ListView m_CombinationListView;
        private Button m_AddCombinationButton;
        private ObjectField m_JsonField;
        private Button m_ExtractJsonButton;
        private Button m_LoadJsonButton;
        private ThaiGlyphRecord m_SelectedGlyphRecord;
        private EnumField m_LanguageEnumField;
        
        private ThaiGlyphLanguage m_Language = ThaiGlyphLanguage.English;
        UnityEvent<ThaiGlyphLanguage> m_LanguageCallback = new UnityEvent<ThaiGlyphLanguage>();
        
        [MenuItem("Window/Thai Font Fixer")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ThaiFontFixerWindow));
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            VisualTreeAsset visualTree = null;
            VisualElement tree = null;
            if(!CheckTMP())
            {
                visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Resources/ThaiFontFixer/ThaiFontFixerPrimer.uxml");
                tree = visualTree.Instantiate();
                root.Add(tree);
                
                var importButton = root.Q<Button>("import-button");
                importButton.clicked += TryImportTMP;
                
                return;
            }
            
            visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Resources/ThaiFontFixer/ThaiFontFixer.uxml");
            tree = visualTree.Instantiate();
            root.Add(tree);
            
            m_FontAssetField = root.Q<ObjectField>("font-asset-field");
            m_FixGlyphsButton = root.Q<Button>("fix-glyph-button");
            m_AddCombinationButton = root.Q<Button>("add-combination-button");
            m_CombinationListView = root.Q<ListView>("combination-list-view");
            
            m_JsonField = root.Q<ObjectField>("json-field");
            m_ExtractJsonButton = root.Q<Button>("extract-preset-button");
            m_LoadJsonButton = root.Q<Button>("load-preset-button");

            m_LanguageEnumField = root.Q<EnumField>("lang-setting");
            
            m_FontAssetField.RegisterValueChangedCallback(FontAssetSelected);

            m_FixGlyphsButton.clicked += FixGlyphsButtonClicked;
            m_AddCombinationButton.clicked += AddCombinationButtonClicked;

            m_JsonField.RegisterValueChangedCallback(JsonFileSelected);
            m_ExtractJsonButton.clicked += ExtractJsonButtonClicked;
            m_LoadJsonButton.clicked += LoadJsonButtonClicked;

            m_LanguageEnumField.RegisterValueChangedCallback(LangFieldSelected);
            
            root.Q<Button>("reset-button").clicked += () =>
            {
                ClearPair();
                UpdateChanges();
            };

            if(LoadCache())
            {
                LoadCombination();
            }
        }

        private void LangFieldSelected(ChangeEvent<Enum> evt)
        {
            m_Language = (ThaiGlyphLanguage)evt.newValue;
            m_LanguageCallback.Invoke(m_Language);
            SaveCache();
        }

        private void LoadJsonButtonClicked()
        {
            if(m_SelectedGlyphRecord == null)
                return;
            
            ClearPair();
            
            foreach(var pair in m_SelectedGlyphRecord.glyphGroupPairs)
            {
                AddCombinationPair(pair);
            }
            
            FixGlyphsButtonClicked();
        }

        private async void ExtractJsonButtonClicked()
        {
            string name = m_FontAsset.name;
            var path = $"{Application.dataPath}/{name}_ThaiGlyphRecord.json";
            await System.IO.File.WriteAllTextAsync(path, JsonUtility.ToJson(new ThaiGlyphRecord(m_Combinations)));
            AssetDatabase.Refresh();

            var assetPath = $"Assets/{name}_ThaiGlyphRecord.json";
            m_JsonField.value = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            
            Debug.Log($"Asset saved at {assetPath}");
        }

        private void JsonFileSelected(ChangeEvent<Object> evt)
        {
            var textFile = evt.newValue as TextAsset;
            if(textFile == null)
                return;
            var fileType = Path.GetExtension(AssetDatabase.GetAssetPath(textFile));

            if(fileType != ".json")
            {
                m_JsonField.value = null;
                return;
            }
            
            string textString = textFile.text;
            try
            {
                m_SelectedGlyphRecord = JsonUtility.FromJson<ThaiGlyphRecord>(textString);
            }
            catch(Exception e)
            {
                Debug.LogWarning(e.Message);
                throw;
            }
        }

        private void AddCombinationButtonClicked()
        {
            AddCombinationPair(null);
        }

        private void AddCombinationPair(ThaiGlyphGroupPair pair = null)
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/Resources/ThaiFontFixer/ThaiGlyphCombinator.uxml");
            VisualElement combination = visualTree.Instantiate();
            m_CombinationListView.hierarchy.Add(combination);
            
            var foldout = combination.Q<Foldout>("combination-label");
            foldout.text = pair?.Name ?? "New Combination";
            
            var toggle = combination.Q<Toggle>("combination-toggle");
            toggle.value = pair?.IsEnabled ?? true;
            
            var offsetLeftField = combination.Q<Vector2Field>("left-offset");
            var offsetRightField = combination.Q<Vector2Field>("right-offset");
            var enumLeftField = combination.Q<EnumField>("left-glyph-selector");
            var enumRightField = combination.Q<EnumField>("right-glyph-selector");
            var previewLeft = combination.Q<Label>("left-preview");
            var previewRight = combination.Q<Label>("right-preview");
            var removeButton = combination.Q<Button>("remove-button");
            
            
            if(pair != null)
            {
                offsetLeftField.value = pair.Left.offset;
                offsetRightField.value = pair.Right.offset;

                enumLeftField.value = pair.Left.typeEnum;
                enumRightField.value = pair.Right.typeEnum;
                
                pair.Left.SetGroup(pair.Left.typeEnum);
                pair.Right.SetGroup(pair.Right.typeEnum);
                
                previewLeft.text = pair.Left.Name;
                previewRight.text = pair.Right.Name;
                
                foldout.text = pair.GetFormalName(m_Language == ThaiGlyphLanguage.Thai);
            }
            else
            {
                pair = new ThaiGlyphGroupPair();
                
                pair.Left.SetGroup(ThaiGlyphTypeEnum.AllConsonants);
                pair.Right.SetGroup(ThaiGlyphTypeEnum.AllConsonants);
            }
            
            m_Combinations.Add(pair);
            removeButton.clicked += () =>
            {
                m_Combinations.Remove(pair);
                m_CombinationListView.hierarchy.Remove(combination);
                
                UpdateChanges();
            };

            toggle.RegisterValueChangedCallback(evt =>
            {
                pair.IsEnabled = evt.newValue;
                
                UpdateChanges();
            });
            
            offsetLeftField.RegisterValueChangedCallback(evt =>
            {
                pair.Left.offset = evt.newValue;
            });
            offsetRightField.RegisterValueChangedCallback(evt =>
            {
                pair.Right.offset = evt.newValue;
            });

            enumLeftField.RegisterValueChangedCallback(evt =>
            {
                var enumValue = (ThaiGlyphTypeEnum)evt.newValue;
                pair.Left.SetGroup(enumValue);
                previewLeft.text = pair.Left.Name;
                foldout.text = pair.GetFormalName(m_Language == ThaiGlyphLanguage.Thai);

            });
            
            enumRightField.RegisterValueChangedCallback(evt =>
            {
                var enumValue = (ThaiGlyphTypeEnum)evt.newValue;
                pair.Right.SetGroup(enumValue);
                previewRight.text = pair.Right.Name;
                foldout.text = pair.GetFormalName(m_Language == ThaiGlyphLanguage.Thai);
            });
            
            m_LanguageCallback.AddListener(lang =>
            {
                foldout.text = pair.GetFormalName(lang == ThaiGlyphLanguage.Thai);
            });
        }

        private void UpdateChanges()
        {
            SaveCombination();
            FixGlyphsButtonClicked();
        }

        private void OnGUI()
        {
            if(!CheckTMP()) return;
            
            m_FixGlyphsButton.SetEnabled(m_FontAsset != null);
        }

        private void FixGlyphsButtonClicked()
        {
            var adjustmentRecords = m_FontAsset.fontFeatureTable.glyphPairAdjustmentRecords;
            adjustmentRecords.Clear();

            foreach(ThaiGlyphGroupPair pair in m_Combinations)
            {
                if(pair.IsEnabled == false) continue;
                
                var leftCharacters = pair.Left.Glyphs.Trim();

                var rightCharacters = pair.Right.Glyphs.Trim();

                foreach(var leftChar in leftCharacters)
                {
                    if(!TMPHelper.GetGlyphIndex(m_FontAsset, leftChar, out var leftGlyphIndex, out var _))
                        continue;

                    foreach(var rightChar in rightCharacters)
                    {
                        if(!TMPHelper.GetGlyphIndex(m_FontAsset, rightChar, out var rightGlyphIndex, out var _))
                            continue;

                        var record = TMPHelper.GetGlyphAdjustmentRecord(m_FontAsset, leftGlyphIndex, rightGlyphIndex);

                        var recordLeft = record.firstAdjustmentRecord;
                        var recordRight = record.secondAdjustmentRecord;

                        recordLeft.glyphValueRecord = ApplyGroupToRecord(pair.Left, recordLeft);
                        recordRight.glyphValueRecord = ApplyGroupToRecord(pair.Right, recordRight);

                        var newRecord = new TMP_GlyphPairAdjustmentRecord(recordLeft, recordRight);

                        m_FontAsset.fontFeatureTable.glyphPairAdjustmentRecords.Remove(record);
                        m_FontAsset.fontFeatureTable.glyphPairAdjustmentRecords.Add(newRecord);
                    }
                }
            }

            m_FontAsset.ReadFontAssetDefinition();
            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, m_FontAsset);
            EditorUtility.SetDirty(m_FontAsset);

            SaveCombination();
        }

        static TMP_GlyphValueRecord ApplyGroupToRecord(ThaiGlyphGroup pair, TMP_GlyphAdjustmentRecord record)
        {
            var rec = record.glyphValueRecord;
            rec.xPlacement = pair.XPlacement;
            rec.yPlacement = pair.YPlacement;
            return rec;
        }

        private void FontAssetSelected(ChangeEvent<Object> evt)
        {
            m_FontAsset = evt.newValue as TMP_FontAsset;

            if(m_FontAsset != null)
            {
                SaveCache();

                LoadCombination();
            }
        }
        

        private bool LoadCache()
        {
            if(!System.IO.File.Exists(m_CachePath))
            {
                return false;
            }

            string jsonString = System.IO.File.ReadAllText(m_CachePath);
            ThaiFontFixerCacheData cache = JsonUtility.FromJson<ThaiFontFixerCacheData>(jsonString);

            m_FontAsset =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(cache.fontAssetGuid));
            if(m_FontAsset != null)
            {
                m_FontAssetField.value = m_FontAsset;
            }
            
            m_Language = cache.language;
            m_LanguageEnumField.value = m_Language;

            return true;
        }

        private void SaveCache()
        {
            string guid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(m_FontAsset)).ToString();

            ThaiFontFixerCacheData data = new ThaiFontFixerCacheData()
            {
                fontAssetGuid = guid,
                language = m_Language
            };

            System.IO.File.WriteAllTextAsync(m_CachePath, JsonUtility.ToJson(data));
        }

        private bool LoadCombination()
        {
            ClearPair();
            
            string guid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(m_FontAsset)).ToString();
            var path = $"{Application.persistentDataPath}/{guid}_ThaiGlyphRecord.json";
            if(!System.IO.File.Exists(path))
            {
                return false;
            }

            string jsonString = System.IO.File.ReadAllText(path);
            ThaiGlyphRecord cache = JsonUtility.FromJson<ThaiGlyphRecord>(jsonString);
            
            foreach(var pair in cache.glyphGroupPairs)
            {
                AddCombinationPair(pair);
            }

            return true;
        }

        private void SaveCombination()
        {
            string guid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(m_FontAsset)).ToString();
            var path = $"{Application.persistentDataPath}/{guid}_ThaiGlyphRecord.json";

            System.IO.File.WriteAllTextAsync(path, JsonUtility.ToJson(new ThaiGlyphRecord(m_Combinations)));
            
        }

        private void ClearPair()
        {
            m_Combinations.Clear();
            m_CombinationListView.hierarchy.Clear();
        }
        
        private bool CheckTMP()
        {
            string[] settings = AssetDatabase.FindAssets("t:TMP_Settings");
            return settings.Length > 0;
        }
        
        private void TryImportTMP()
        {
            if (!CheckTMP())
            {
                string packagePath = Path.GetFullPath("Packages/com.unity.textmeshpro");
            
                AssetDatabase.ImportPackage(packagePath + "/Package Resources/TMP Essential Resources.unitypackage", true);
                
                GetWindow(typeof(ThaiFontFixerWindow)).Close();
            }
        }
    }
}