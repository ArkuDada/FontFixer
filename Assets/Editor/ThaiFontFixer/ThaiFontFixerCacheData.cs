using System;
using System.Collections.Generic;
using UnityEngine;

namespace Editor.ThaiFontFixer
{
    [Serializable]
    public class ThaiFontFixerCacheData
    {
        public string fontAssetGUID;
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