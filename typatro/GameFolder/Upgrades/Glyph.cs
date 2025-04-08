using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace typatro.GameFolder.Upgrades{

    public enum Glyph
    {
        [Description("No description available")]
        NoGlyphsLeft,
        [Description("Represents the sound 'A', used for names or verbs.")]
        A,

        [Description("Represents the sound 'B', symbol for 'house' or 'building'.")]
        B,

        [Description("Represents the sound 'D', used in names or words with this sound.")]
        D,

        [Description("Represents the sound 'H', symbolizing movement or breath.")]
        H,

        [Description("Represents the sound 'K', used in names and with meaning of strength.")]
        K,

        [Description("Represents the sound 'F', symbol for 'fish', representing water.")]
        F,

        [Description("Represents the sound 'M', symbol for 'mother' or 'woman'.")]
        M,

        [Description("Represents the sound 'N', used for words like 'water' or 'running'.")]
        N,

        [Description("Represents the sound 'R', symbolizing the king or the sun.")]
        R,

        [Description("Represents the sound 'W', symbolizing gods or power.")]
        W,

        [Description("Represents the sun symbol, representing Ra, the sun god.")]
        Sun,

        [Description("Symbol for 'house', used to represent domestic life or temples.")]
        House,

        [Description("Symbol for water, often representing rivers or offerings.")]
        Water,

        [Description("Symbol for the king, signifying royalty or divine authority.")]
        King,

        [Description("The Eye of Horus, symbolizing protection and health.")]
        EyeOfHorus,

        [Description("Symbol for Osiris, the god of the afterlife and resurrection.")]
        Osiris,

        [Description("Symbol for 'man', used for writing the word 'person'.")]
        Man,

        [Description("Symbol for existence, used for the verb 'to be' or 'exist'.")]
        Existence,

        [Description("Symbol for cat, representing the goddess Bastet, protector of the home.\n+ \n- A cat will be sleeping on a random location")]
        Cat,

        [Description("Symbol for Anubis, the god of mummification and protector of tombs.")]
        Anubis,

        [Description("Symbol for scarab, associated with rebirth and regeneration.")]
        Scarab,

        [Description("Symbol for snake, often signifying danger or protection.")]
        Snake,

        [Description("Symbol for life, representing the eternal or divine life force.")]
        Life,

        [Description("Symbol for heart, associated with soul and divine judgment.")]
        Heart,

        [Description("Symbol for crocodile, associated with danger, often from the Nile.")]
        Crocodile,

        [Description("Symbol for the number one, a fundamental part of their numeric system.")]
        One,

        [Description("Symbol for the number ten, used in the Egyptian counting system.")]
        Ten,

        [Description("Symbol for the number one hundred, used in the Egyptian numeral system.")]
        Hundred,

        [Description("Symbol for the number one thousand, used in large counting or measurements.")]
        Thousand,

        [Description("Symbol for bread, representing basic sustenance and offerings.")]
        Bread,

        [Description("Symbol for papyrus, representing writing, knowledge, and records.")]
        Papyrus,

        [Description("Symbol for a star, often associated with the divine or celestial bodies.")]
        Star
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