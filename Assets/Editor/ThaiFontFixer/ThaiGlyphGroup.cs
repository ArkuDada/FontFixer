/*
ThaiGlyphGroup.cs

This file defines the ThaiGlyphGroup and ThaiGlyphGroupPair classes, which are used to manage and represent groups of Thai glyphs.
It includes functionality for grouping glyphs, calculating offsets, and generating formal names for glyph pairs.

Classes:
ThaiGlyphGroup: Represents a group of Thai glyphs with associated metadata such as type and placement offsets.
ThaiGlyphGroupPair: Represents a pair of ThaiGlyphGroup objects and provides methods for retrieving their combined names and counts.

Author: Pada Cherdchoothai
Created: 2023-10-06

Usage:
Use ThaiGlyphGroup to define and manage individual glyph groups.
Use ThaiGlyphGroupPair to combine two glyph groups and retrieve their formal names or counts. */
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Editor.ThaiFontFixer
{
    [Serializable]
    public class ThaiGlyphGroupPair
    {
        [SerializeField]
        public bool IsEnabled = true;
        [SerializeField]
        public ThaiGlyphGroup Left = new ThaiGlyphGroup();
        [SerializeField]
        public ThaiGlyphGroup Right = new ThaiGlyphGroup();

        public ThaiGlyphGroupPair()
        {
        }

        public ThaiGlyphGroupPair(ThaiGlyphGroupPair pair)
        {
            Left = new ThaiGlyphGroup(pair.Left);
            Right = new ThaiGlyphGroup(pair.Right);
        }

        public string GetFormalName(bool isThai = false)
        {
            return ThaiCharacterCollection.GetThaiGlyphGroupName(Left.typeEnum,isThai) + " + " +
                   ThaiCharacterCollection.GetThaiGlyphGroupName(Right.typeEnum,isThai);
        }
        public string Name => Left.Name + " + " + Right.Name;

        public int Count => Left.Count * Right.Count;
    }

    [Serializable]
    public class ThaiGlyphGroup
    {
        [FormerlySerializedAs("Preset")]
        [SerializeField]
        public ThaiGlyphTypeEnum typeEnum = ThaiGlyphTypeEnum.None;
        
        [NonSerialized]
        public string Glyphs = "";
        
        [HideInInspector]
        [SerializeField]
        public Vector2 offset = Vector2.zero;
        public float XPlacement
        {
            get => offset.x;
            set => offset.x = value;
        }
        public float YPlacement
        {
            get => offset.y;
            set => offset.y = value;
        }
        

        public int Count => Glyphs.Length;

        public string Name
        {
            get
            {
                var displayedChars = Glyphs.Select(c => ThaiCharacterCollection.GetDisplayedString(c.ToString()))
                    .ToArray();
                if(displayedChars.Length == 1)
                    return displayedChars.FirstOrDefault();

                return "[ " + string.Join(", ", displayedChars) + " ]";
            }
        }
        
        public ThaiGlyphGroup()
        {
            this.offset = Vector2.zero;
            SetGroup(ThaiGlyphTypeEnum.None);
        }
        
        public ThaiGlyphGroup(ThaiGlyphTypeEnum typeEnum , Vector2 offset)
        {
            this.offset = offset;
            SetGroup(typeEnum);
        }
        

        public ThaiGlyphGroup(ThaiGlyphGroup g)
        {
            typeEnum = g.typeEnum;
            Glyphs = g.Glyphs;
            XPlacement = g.XPlacement;
            YPlacement = g.YPlacement;
        }

        public void SetGroup(ThaiGlyphTypeEnum typeEnum)
        {
            this.typeEnum = typeEnum;
            Glyphs = string.Join("", ThaiCharacterCollection.GetGlyphs(typeEnum));
        }
    }
}