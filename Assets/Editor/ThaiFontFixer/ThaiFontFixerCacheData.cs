/*
 * ThaiFontFixerCacheData.cs
 *
 * This file defines the `ThaiFontFixerCacheData` and `ThaiGlyphRecord` classes, which are used to manage cached data and records for Thai font glyph adjustments.
 * It includes functionality for storing font asset GUIDs, language preferences, and glyph group pairings.
 *
 * Classes:
 * - `ThaiFontFixerCacheData`: Represents cached data for a font asset, including its GUID and language settings.
 * - `ThaiGlyphRecord`: Represents a collection of `ThaiGlyphGroupPair` objects for managing glyph group pairings.
 *
 * Author: Pada Cherdchoothai
 * Created: 2023-10-06
 *
 * Usage:
 * - Use `ThaiFontFixerCacheData` to store and retrieve font asset metadata and language preferences.
 * - Use `ThaiGlyphRecord` to manage and serialize glyph group pairings for Thai font adjustments.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Editor.ThaiFontFixer
{
    [Serializable]
    public class ThaiFontFixerCacheData
    {
        public string fontAssetGuid;
        public ThaiGlyphLanguage language;
    }

    [Serializable]
    public class ThaiGlyphRecord
    {
        [SerializeField]
        public List<ThaiGlyphGroupPair> glyphGroupPairs = new List<ThaiGlyphGroupPair>();
        
        public ThaiGlyphRecord(List<ThaiGlyphGroupPair> list)
        {
            glyphGroupPairs = list;
        }
        
    }
}