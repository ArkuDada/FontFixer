/*
 * ThaiCharacterCollection.cs
 *
 * This file defines the `ThaiCharacterCollection` class, which provides utilities for managing and categorizing Thai characters.
 * It includes methods for retrieving displayed strings, grouping Thai glyphs by type, and accessing specific character sets.
 *
 * Enumerations:
 * - `ThaiGlyphTypeEnum`: Represents different types of Thai glyphs (e.g., consonants, vowels, tone marks).
 * - `ThaiGlyphLanguage`: Represents supported languages for glyph group names (Thai or English).
 *
 * Author: Pada Cherdchoothai
 * Created: 2023-10-06
 *
 * Usage:
 * - Use `GetDisplayedString` to format Thai characters for display.
 * - Use `GetThaiGlyphGroupName` to retrieve the name of a glyph group in Thai or English.
 * - Use `GetGlyphs` to retrieve character arrays for specific glyph types.
 */

using System;
using System.Linq;

public class ThaiCharacterCollection
{
    static char[] behindDashGlyphs => allFollowingVowels
        .Concat(lowerVowels)
        .Concat(allFollowingVowels)
        .Concat(allUpperGlyphs)
        .ToArray();

    static readonly char[] lowerVowels = new[] { 'ุ', 'ู' };
    static readonly char[] allFollowingVowels = new[] { 'ะ', 'ำ', 'า', 'ๅ' };
    static readonly char[] leadingVowels = new[] { 'เ', 'แ', 'โ', 'ไ', 'ใ' };
    static readonly char[] upperVowels = new[] { 'ิ', 'ี', 'ึ', 'ื', '็', 'ั' };

    static readonly char[] allUpperGlyphs = new[] { 'ิ', 'ี', 'ึ', 'ื', '็', 'ั', '์', '่', '้', '๊', '๋' };
    static readonly char[] toneMarks = new[] { '่', '้', '๊', '๋' };
    static readonly char thanThaKhaat = '์';
    static readonly char saraAum = 'ำ';

    static readonly char[] allConsonants = new char[]
    {
        'ก', 'ข', 'ฃ', 'ค', 'ฅ', 'ฆ', 'ง', 'จ', 'ฉ', 'ช',
        'ซ', 'ฌ', 'ญ', 'ฎ', 'ฏ', 'ฐ', 'ฑ', 'ฒ', 'ณ', 'ด',
        'ต', 'ถ', 'ท', 'ธ', 'น', 'บ', 'ป', 'ผ', 'ฝ', 'พ',
        'ฟ', 'ภ', 'ม', 'ย', 'ร', 'ล', 'ว', 'ศ', 'ษ', 'ส',
        'ห', 'ฬ', 'อ', 'ฮ'
    };

    static readonly char[] descenderConsonants = new char[]
    {
        'ญ', 'ฎ', 'ฏ', 'ฐ'
    };

    static readonly char[] ascenderConsonants = new char[]
    {
        'ป', 'ฝ', 'ฟ', 'ฬ'
    };

    public static string GetDisplayedString(string characters)
    {
        characters = characters.Trim();
        if(string.IsNullOrEmpty(characters))
            return "";

        if(characters.Length > 1)
            return characters;

        if(behindDashGlyphs.Contains(characters[0]))
            return "-" + characters[0];

        if(leadingVowels.Contains(characters[0]))
            return characters[0] + "-";

        return characters;
    }

    public static string GetThaiGlyphGroupName(ThaiGlyphTypeEnum typeEnum, bool isThai = false)
    {
        switch(typeEnum)
        {
            case ThaiGlyphTypeEnum.LowerVowels:
                return isThai ? "สระล่าง" : "Lower Vowels";
            case ThaiGlyphTypeEnum.AllUpperGlyphs:
                return isThai ? "อักขระด้านบน" : "All Upper Glyphs";
            case ThaiGlyphTypeEnum.AllFollowingVowels:
                return isThai ? "สระหลัง" : "All Following Vowels";
            case ThaiGlyphTypeEnum.LeadingVowels:
                return isThai ? "สระหน้า" : "Leading Vowels";
            case ThaiGlyphTypeEnum.UpperVowels:
                return isThai ? "สระบน" : "Upper Vowels";
            case ThaiGlyphTypeEnum.ToneMarks:
                return isThai ? "วรรณยุกต์" : "Tone Marks";
            case ThaiGlyphTypeEnum.ThanThaKhaat:
                return isThai ? "ทัณฑฆาต" : "ThanThaKhaat";
            case ThaiGlyphTypeEnum.SaraAum:
                return isThai ? "สระอำ" : "SaraAum";
            case ThaiGlyphTypeEnum.AllConsonants:
                return isThai ? "พยัญชนะทั้งหมด" : "All Consonants";
            case ThaiGlyphTypeEnum.DescenderConsonants:
                return isThai ? "พยัญชนะหางล่าง" : "Descender Consonants";
            case ThaiGlyphTypeEnum.AscenderConsonants:
                return isThai ? "พยัญชนะหางบน" : "Ascender Consonants";
            default:
                throw new ArgumentOutOfRangeException(nameof(typeEnum), typeEnum, null);
        }
    }

    public static char[] GetGlyphs(ThaiGlyphTypeEnum typeEnum)
    {
        switch(typeEnum)
        {
            case ThaiGlyphTypeEnum.AllConsonants:
                return allConsonants;
            case ThaiGlyphTypeEnum.AscenderConsonants:
                return ascenderConsonants;
            case ThaiGlyphTypeEnum.DescenderConsonants:
                return descenderConsonants;
            case ThaiGlyphTypeEnum.AllUpperGlyphs:
                return allUpperGlyphs;
            case ThaiGlyphTypeEnum.UpperVowels:
                return upperVowels;
            case ThaiGlyphTypeEnum.ToneMarks:
                return toneMarks;
            case ThaiGlyphTypeEnum.ThanThaKhaat:
                return new char[] { thanThaKhaat };
            case ThaiGlyphTypeEnum.LeadingVowels:
                return leadingVowels;
            case ThaiGlyphTypeEnum.AllFollowingVowels:
                return allFollowingVowels;
            case ThaiGlyphTypeEnum.SaraAum:
                return new char[] { saraAum };
            case ThaiGlyphTypeEnum.LowerVowels:
                return lowerVowels;
            case ThaiGlyphTypeEnum.None:
                return new char[] { };
            default:
                throw new ArgumentOutOfRangeException(nameof(typeEnum), typeEnum, null);
        }
    }
}

public enum ThaiGlyphTypeEnum
{
    AllConsonants,
    AscenderConsonants,
    DescenderConsonants,
    AllUpperGlyphs,
    UpperVowels,
    ToneMarks,
    ThanThaKhaat,
    LeadingVowels,
    AllFollowingVowels,
    SaraAum,
    LowerVowels,
    None
}

public enum ThaiGlyphLanguage
{
    Thai,
    English
}