/*
 * TMPHelper.cs
 *
 * This file defines the `TMPHelper` class, which provides utility methods for working with TextMeshPro font assets.
 * It includes methods for retrieving glyph indices and managing glyph adjustment records.
 *
 * Methods:
 * - `GetGlyphIndex`: Retrieves the glyph index and character index for a given character in a font asset.
 * - `GetGlyphAdjustmentRecord`: Retrieves or creates a glyph pair adjustment record for two glyph indices.
 *
 * Author: Pada Cherdchoothai
 * Created: 2023-10-06
 *
 * Usage:
 * - Use `GetGlyphIndex` to find the glyph and character indices for a specific character in a `TMP_FontAsset`.
 * - Use `GetGlyphAdjustmentRecord` to retrieve or add adjustment records for glyph pairs in a font asset.
 */

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