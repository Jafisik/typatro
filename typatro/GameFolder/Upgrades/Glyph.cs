using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace typatro.GameFolder.Upgrades{

    public enum Glyph
    {
        [Description("No description available")]
        NoGlyphsLeft,
        [Description("Displays the text upside down")]
        ReverseText,
        [Description("Shakes screen everytime a letter is pressed")]
        GameShake,
        InvisibleLetters,
        TimeSpeedUp
    }

    public static class GlyphManager
    {
        private static HashSet<Glyph> activeGlyphs = new HashSet<Glyph>();

        public static void Add(Glyph glyph) => activeGlyphs.Add(glyph);
        public static void Remove(Glyph glyph) => activeGlyphs.Remove(glyph);
        public static bool IsActive(Glyph glyph) => activeGlyphs.Contains(glyph);

        public static Glyph GetRandomUnusedGlyph()
        {
            Random random = new Random();
            var allGlyphs = Enum.GetValues(typeof(Glyph)).Cast<Glyph>();
            var unusedGlyphs = allGlyphs.Except(activeGlyphs).ToList();

            if (unusedGlyphs.Count == 0)
                return Glyph.NoGlyphsLeft;

            int index = random.Next(unusedGlyphs.Count);
            return unusedGlyphs[index];
        }

        public static string GetDescription(Glyph? glyph){
            var field = glyph.GetType().GetField(glyph.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? "No description available" : attribute.Description;
        }
    }
}