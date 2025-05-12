using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace typatro.GameFolder.Upgrades{

    public enum Glyph
    {
        [Description("No description available")]
        NoGlyphsLeft,
        [Description("Sound for 'A', used for names or verbs.\n+ Adds 5 to all vowels")]
        A,

        [Description("Sound for 'B', symbol for foot or place.\n+ Adds +5 to the starting score each fight")]
        B,

        [Description("Sound for 'D', symbol for hand.\n+ Multiplies 2 random letter scores by *5")]
        D,

        [Description("Sound for 'H', symbolizing shelter or building.\n+ Multiplies 10 random letters by *5\n- Rotates text upside down every 10 seconds")]
        H,

        [Description("Sound for 'J', also symbol for reed or tall grass.\n+ Add +10 to all letter scores\n- Screen shakes with every input")]
        J,

        [Description("Sound for 'M', can symbolize owl, silence or wisdom.\n+ Gains 10 coins \nevery 10 seconds in a fight\n- Text changes position every 5 seconds")]
        M,

        [Description("Sound for 'N', ripple of water, also 'to' or 'for'.\n+ Words written under 3 seconds are doubled\n- Words written over 3 seconds break streak")]
        N,

        [Description("Sound for 'R', symbolizing mouth or one whole thing.\n+ Lets you correct your mistakes\n- Mistakes count as -5")]
        
        R,

        [Description("Sound for 'S', also sufffix for she, her, hers.\n+ Elite and boss fights deal less damage")]

        S,
        [Description("'Sun', representing Ra, the sun god.\n+ Disables all visual glyphs\n- Letters are less visible")]
        Sun,

        [Description("'House', represents domestic life or temples.\n+ Take only quarter damage\n- Every 8 seconds your keyboard stops working")]
        House,

        [Description("'Water', often representing rivers or offerings.\n+ Multiplies final score by *2\n- Add -5 to 5 random letter scores")]
        Water,

        [Description("'King', signifying royalty or divine authority.\n+ Multiplies ALL highest letter scores by *5\n- Sets ALL letters with lowest letter score to 0 (only if max score is different to min score)")]
        King,

        [Description("The Eye of Horus, symbolizing protection and health.\n+ 2 mistakes per round do not count\n- You blink every 5 seconds")]
        EyeOfHorus,

        [Description("Osiris, the god of the afterlife and resurrection.\n+ Revive once per run\n- After ressurection all letters multiplied by *0.8")]
        Osiris,

        [Description("'Woman', symbolizes feminine names or nurture.\n+ Multiplies random letter scoreby *2 after each fight\n- Earn only 80% coins")]
        Woman,

        [Description("'Man', used for writing the word 'person'.\n+ Multiplies coin rewards by *1.5\n- Letter 'x' counts as a mistake")]
        Man,

        [Description("'Flower', meaning 'to be' or 'to exist'.\n+ Adds +0.1 to the final score multiplier for each active glyph")]
        Flower,

        [Description("'Cat', representing the goddess Bastet, protector of the home.\n+ Multiplies 9 random letter values by *2\n- A cat will be sleeping on a random location")]
        Cat,

        [Description("Anubis, the god of mummification and protector of tombs.\n+ Gain 1 coin per 5 words")]
        Anubis,

        [Description("'Scarab', associated with rebirth and regeneration.\n+ Streak bonus increases by +3 every consecutive correct word")]
        Scarab,

        [Description("'Snake', often signifying danger or protection.\n+ Mistakes do not subtract from score\n- Some letters are wrong")]
        Snake,

        [Description("'Life', representing the eternal or divine life force.\n+ Add 5 to a random letter score each time you visit a shop")]
        Life,

        [Description("'Heart', associated with soul and divine judgment.\n+ Multiplies final score by *3 on perfect rounds\n- Halves score if you make a mistake")]
        Heart,

        [Description("'Crocodile', associated with danger.\n+ Multiplies random letter value by *20\n- Sets random letter value to 0")]
        Crocodile,

        [Description("Symbol for the number one.\n+ Adds +1 to every letter value")]
        One,

        [Description("Symbol for the number ten, cattle hobble.\n+ Multiplies random letter \nvalue by *10")]
        Ten,

        [Description("Symbol for the number one hundred, coil of rope.\n+ Adds 100 coins")]
        Hundred,

        [Description("Symbol for the number one thousand, water lily.\n+ Every 1000 letters automatically wins fight")]
        Thousand,

        [Description("'Bread', representing basic sustenance and offerings.\n+ Add +3 to 6 random letter scores")]
        Bread,

        [Description("'Papyrus', representing writing, knowledge, and records.\n+ Adds +20 extra words in fights")]
        Papyrus,

        [Description("'Star', often associated with the divine or celestial bodies.\n+ Multiplies all letter scores by *20\n- If you make a single mistake, you die instantly")]
        Star
    }

    public static class GlyphManager
    {
        private static HashSet<Glyph> activeGlyphs = new HashSet<Glyph>();
        public static List<Texture2D> glyphImage = new List<Texture2D>();

        public static void Add(Glyph glyph) => activeGlyphs.Add(glyph);
        public static void Remove(Glyph glyph) => activeGlyphs.Remove(glyph);
        public static bool IsActive(Glyph glyph) => activeGlyphs.Contains(glyph);

        public static Glyph GetRandomUnusedGlyph()
        {
            if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("GetRandomUnusedGlyph", ""));
            var allGlyphs = Enum.GetValues(typeof(Glyph)).Cast<Glyph>();
            var unusedGlyphs = allGlyphs.Except(activeGlyphs).ToList();

            if (unusedGlyphs.Count == 0)
                return Glyph.NoGlyphsLeft;

            int index = GameLogic.seededRandom.Next(unusedGlyphs.Count);
            return unusedGlyphs[index];
        }

        public static Glyph[] GetGlyphs(){
            return activeGlyphs.ToArray();
        }

        public static int[] GlyphNums(){
            Glyph[] glyphs = activeGlyphs.ToArray();
            List<int> ints = new List<int>();
            for(int i = 0; i < glyphs.Length; i++){
                ints.Add((int)glyphs[i]);
            }
            return ints.ToArray();
        }

        public static int GetGlyphCount(){
            return activeGlyphs.Count;
        }

        public static string GetDescription(Glyph? glyph){
            var field = glyph.GetType().GetField(glyph.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? "No description available" : attribute.Description;
        }

        public static Texture2D GetGlyphImage(Glyph glyph){
            return glyphImage[(int)glyph];
        }
    }
}