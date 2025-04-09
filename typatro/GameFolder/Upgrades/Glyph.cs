using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace typatro.GameFolder.Upgrades{

    public enum Glyph
    {
        [Description("No description available")]
        NoGlyphsLeft,
        [Description("Represents the sound 'A', used for names or verbs.\n+ Adds 5 to all vowels")]
        A,

        [Description("Represents the sound 'B', symbol for 'house' or 'building'.\nAdds +5 to the starting score each fight")]
        B,

        [Description("Represents the sound 'D', used in names or words with this sound.\n+ Multiplies d letter score by *4")]
        D,

        [Description("Represents the sound 'H', symbolizing movement or breath.\n+ Multiplies 10 random letters by *5\n- Rotates text every 10 seconds")]
        H,

        [Description("Represents the sound 'K', used in names and with meaning of strength.\n+ Add +10 to all letter scores\n- Screen shakes with every input")]
        K,

        [Description("Represents the sound 'F', symbol for 'fish', representing water.\n+ Gains 10 coins every 10 seconds in a fight\n- Text changes position every 5 seconds")]
        F,

        [Description("Represents the sound 'M', symbol for 'mother' or 'woman'.\n+ Multiplies random letter score by *2 after each fight\n- Earn only 80% coins")]
        M,

        [Description("Represents the sound 'N', used for words like 'water' or 'running'.\n+ Words written under 2 seconds are doubled\n- Words written over 4 seconds count as 0")]
        N,

        [Description("Represents the sound 'R', symbolizing the king or the sun.\n+ Lets you correct your mistakes\n- Mistakes count as -5")]
        
        R,

        [Description("Represents the sound 'W', symbolizing gods or power.\n+ Elite and boss fights deal less damage")]
        W,

        [Description("Represents the sun symbol, representing Ra, the sun god.\n+ Disables all visual glyphs\n- Letters are less visible")]
        Sun,

        [Description("Symbol for 'house', used to represent domestic life or temples.\n+ Take only quarter damage\n- Random letter stops working every 10 seconds for 5 seconds")]
        House,

        [Description("Symbol for water, often representing rivers or offerings.\n+ Multiplies final score by *2\n- Add -5 to 5 random letter scores")]
        Water,

        [Description("Symbol for the king, signifying royalty or divine authority.\n+ Multiplies ALL highest letter scores by *5\n- Sets ALL letters with lowest letter score to 0 (only if max score is different to min score)")]
        King,

        [Description("The Eye of Horus, symbolizing protection and health.\n+ 2 mistakes per round do not count\n- You blink every 5 seconds")]
        EyeOfHorus,

        [Description("Symbol for Osiris, the god of the afterlife and resurrection.\n+ Revive once per run\n- After ressurection all letters multiplied by *0.8")]
        Osiris,

        [Description("Symbol for 'man', used for writing the word 'person'.\n+ Multiplies coin rewards by *1.5\n- Letter 'x' counts as a mistake")]
        Man,

        [Description("Symbol for existence, used for the verb 'to be' or 'exist'.\n+ Adds +0.1 to the final score multiplier for each active glyph")]
        Existence,

        [Description("Symbol for cat, representing the goddess Bastet, protector of the home.\n+ Multiplies 9 random letter values by *2\n- A cat will be sleeping on a random location")]
        Cat,

        [Description("Symbol for Anubis, the god of mummification and protector of tombs.\n+ Gain 1 coin per 5 words")]
        Anubis,

        [Description("Symbol for scarab, associated with rebirth and regeneration.\n+ Streak bonus increases by +5 every consecutive correct word")]
        Scarab,

        [Description("Symbol for snake, often signifying danger or protection.\n+ Mistakes do not subtract from score\n- Some letters are wrong")]
        Snake,

        [Description("Symbol for life, representing the eternal or divine life force.\n+ Add 5 to a random letter score each time you visit a shop")]
        Life,

        [Description("Symbol for heart, associated with soul and divine judgment.\n+ Multiplies final score by *3 on perfect rounds\n- Halves score if you make a mistake")]
        Heart,

        [Description("Symbol for crocodile, associated with danger, often from the Nile.\n+ Multiplies random letter value by *20\n- Sets random letter value to 0")]
        Crocodile,

        [Description("Symbol for the number one, a fundamental part of their numeric system.\n+ Adds +1 to every letter value")]
        One,

        [Description("Symbol for the number ten, used in the Egyptian counting system.\n+ Multiplies random letter value by *10")]
        Ten,

        [Description("Symbol for the number one hundred, used in the Egyptian numeral system.\n+ Adds 100 coins")]
        Hundred,

        [Description("Symbol for the number one thousand, used in large counting or measurements.\n+ Every 1000 letters add +1000 to score")]
        Thousand,

        [Description("Symbol for bread, representing basic sustenance and offerings.\n+ Add +3 to 6 random letter scores")]
        Bread,

        [Description("Symbol for papyrus, representing writing, knowledge, and records.\n+ +20 extra words in fights")]
        Papyrus,

        [Description("Symbol for a star, often associated with the divine or celestial bodies.\n+ Multiplies all letter scores by *20\n- If you make a single mistake, you die instantly")]
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