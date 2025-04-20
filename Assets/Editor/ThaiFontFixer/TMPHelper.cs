using TMPro;

namespace Editor.ThaiFontFixer
{
    public class TMPHelper
    {
        public static bool GetGlyphIndex(TMP_FontAsset fontAsset, char character, out uint glyphIndex, out int characterIndex)
        {
            glyphIndex = 0;
            characterIndex = -1;
            if (fontAsset == null)
                return false;
            fontAsset.TryAddCharacters(character.ToString());
            var characterData = fontAsset.characterLookupTable[character];
            if (characterData == null)
                return false;

            glyphIndex = characterData.glyphIndex;
            characterIndex = fontAsset.characterTable.IndexOf(characterData);
            return true;
        }
        
        public static TMP_GlyphPairAdjustmentRecord GetGlyphAdjustmentRecord(TMP_FontAsset fontAsset, uint leftGlyphIndex, uint rightGlyphIndex)
        {
            var adjustmentRecords = fontAsset.fontFeatureTable.glyphPairAdjustmentRecords;
            foreach (var record in adjustmentRecords)
            {
                if (record.firstAdjustmentRecord.glyphIndex == leftGlyphIndex && record.secondAdjustmentRecord.glyphIndex == rightGlyphIndex)
                    return record;
            }

            var firstAdj = new TMP_GlyphAdjustmentRecord(leftGlyphIndex, new TMP_GlyphValueRecord());
            var secondAdj = new TMP_GlyphAdjustmentRecord(rightGlyphIndex, new TMP_GlyphValueRecord());
            var newRecord = new TMP_GlyphPairAdjustmentRecord(firstAdj, secondAdj);
            adjustmentRecords.Add(newRecord);
            return newRecord;
        }
    }
}