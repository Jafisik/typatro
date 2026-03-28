using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using typatro.GameFolder.Services;
using typatro.GameFolder.UI;
using static typatro.GameFolder.Services.UnlockManager;

namespace typatro.GameFolder.Upgrades{

    public enum Glyph
    {
        [Description("You can't buy any more glyphs")]
        NoGlyphsLeft,
        [Description("Meaning: \"Sound for 'A', used for names or verbs.\"\n\n+ Adds 5 to all vowels")]
        A,

        [Description("Meaning: \"Sound for 'B', symbol for foot or place.\"\n\n+ Multiplies 5 random letters by *5\n- Rotates text upside down every 10 seconds")]
        B,

        [Description("Meaning: \"Sound for 'D', symbol for hand.\"\n\n+ Multiplies 2 random letter scores by *5")]
        D,

        [Description("Meaning: \"Sound for 'H', symbolizing shelter or building.\"\n\n+ Blocks +2 mistakes per fight")]
        H,

        [Description("Meaning: \"Sound for 'J', also symbol for reed or tall grass.\"\n\n+ Add +10 to all letter scores\n- Screen shakes with every input")]
        J,

        [Description("Meaning: \"Sound for 'M', can symbolize owl, silence or wisdom.\"\n\n+ Gains 10 coins every 10 seconds in a fight\n- Text changes position every 5 seconds")]
        M,

        [Description("Meaning: \"Sound for 'N', ripple of water, also 'to' or 'for'.\"\n\n+ Each correct stone word gives you +1% shiny chance\n- Score from correct stone words is negative (eg. +50 is now -50)")]
        N,

        [Description("Meaning: \"Sound for 'R', symbolizing mouth or one whole thing.\"\n\n+ Lets you correct your mistakes\n- Each mistake resets streak to 0.8x")]
        R,

        [Description("Meaning: \"Sound for 'S', also sufffix for she, her, hers.\"\n\n+ Bloom words give you +5 letter score per letter")]
        S,

        [Description("Meaning: \"'Sun', representing Ra, the sun god.\"\n\n+ Disables all visual glyphs\n- Letters are less visible")]
        Sun,

        [Description("Meaning: \"'House', represents domestic life or temples.\"\n\n+ Each correct stone word gives you +10 stone word score\n- Every 8 seconds your keyboard stops working")]
        House,

        [Description("Meaning: \"'Water', often representing rivers or offerings.\"\n\n+ Multiplies 5 random letter scores by *2\n- Add -10 to 5 random letter scores")]
        Water,

        [Description("Meaning: \"'King', signifying royalty or divine authority.\"\n\n+ Multiplies all highest letter scores by *50\n- Sets ALL other letter scores to 0")]
        King,

        [Description("Meaning: \"The Eye of Horus, symbolizing protection and health.\"\n\n+ Mistakes do not reset your streak\n- You blink every 5 seconds")]
        EyeOfHorus,

        [Description("Meaning: \"Osiris, the god of the afterlife and resurrection.\"\n\n+ Revive once per run\n- After ressurection all letters multiplied by *0.8")]
        Osiris,

        [Description("Meaning: \"'Woman', symbolizes feminine names or nurture.\"\n\n+ Multiplies random letter score by *2 after each fight\n- Earn only 80% coins")]
        Woman,

        [Description("Meaning: \"'Man', used for writing the word 'person'.\"\n\n+ Multiplies coin rewards by *1.5\n- Letter 'x' counts as a mistake")]
        Man,

        [Description("Meaning: \"'Flower', meaning 'to be' or 'to exist'.\"\n\n+ Adds +0.1 to the final score multiplier for each active glyph")]
        Flower,

        [Description("Meaning: \"'Cat', representing the goddess Bastet, protector of the home.\"\n\n+ Multiplies 9 random letter values by *2\n- A cat will be sleeping on a random location")]
        Cat,

        [Description("Meaning: \"Anubis, the god of mummification and protector of tombs.\"\n\n+ Multiplies coin rewards by *2\n- Bloom words are disabled")]
        Anubis,

        [Description("Meaning: \"'Scarab', associated with rebirth and regeneration.\"\n\n+ Streak bonus is doubled")]
        Scarab,

        [Description("Meaning: \"'Snake', often signifying danger or protection.\"\n\n+ Mistakes do not reset your streak\n- Some letters are wrong")]
        Snake,

        [Description("Meaning: \"'Life', representing the eternal or divine life force.\"\n\n+ Add 5 to a random letter score each time you visit a shop")]
        Life,

        [Description("Meaning: \"'Heart', associated with soul and divine judgment.\"\n\n+ Multiplies final score by *3 on perfect rounds\n- Halves score if you make a mistake")]
        Heart,

        [Description("Meaning: \"'Crocodile', associated with danger.\"\n\n+ Multiplies random letter value by *20\n- Sets random letter value to 0")]
        Crocodile,

        [Description("Meaning: \"Symbol for the number one.\"\n\n+ Adds +1 to every letter value")]
        One,

        [Description("Meaning: \"Symbol for the number ten, cattle hobble.\"\n\n+ Multiplies random letter value by *10")]
        Ten,

        [Description("Meaning: \"Symbol for the number one hundred, coil of rope.\"\n\n+ Adds 100 coins")]
        Hundred,

        [Description("Meaning: \"Symbol for the number one thousand, water lily.\"\n\n+ Every 1000 letters automatically wins fight")]
        Thousand,

        [Description("Meaning: \"'Bread', representing basic sustenance and offerings.\"\n\n+ Add +3 to 6 random letter scores")]
        Bread,

        [Description("Meaning: \"'Papyrus', representing writing, knowledge, and records.\"\n\n+ Adds +20 extra words in fights")]
        Papyrus,

        [Description("Meaning: \"'Star', often associated with the divine or celestial bodies.\"\n\n+ Multiplies all letter scores by *20\n- If you make a single mistake, you die instantly")]
        Star
    }

    public static class GlyphManager
    {
        private static HashSet<Glyph> activeGlyphs = new HashSet<Glyph>();
        public static HashSet<Glyph> unlockedGlyphs = new HashSet<Glyph>();
        public static List<Texture2D> glyphImage = new List<Texture2D>();

        public static void Add(Glyph glyph)
        {
            activeGlyphs.Add(glyph);
            if(activeGlyphs.Count >= 7)
            {
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Woman);
            }
        }
        public static void Remove(Glyph glyph) => activeGlyphs.Remove(glyph);
        public static void RemoveRandom()
        {
            if (activeGlyphs.Count > 0)
            {
                Remove(activeGlyphs.ElementAt(GameLogic.contextRandom.Next(0, activeGlyphs.Count)));
            }
        }
        public static bool IsActive(Glyph glyph) => activeGlyphs.Contains(glyph);

        public static Glyph GetRandomUnusedGlyph()
        {
            var unusedGlyphs = unlockedGlyphs.Except(activeGlyphs).ToList();
            if (unusedGlyphs.Count == 0)
            {
                return Glyph.NoGlyphsLeft;
            }
            int index = GameLogic.contextRandom.Next(unusedGlyphs.Count);
            return unusedGlyphs[index];
        }

        public static Glyph[] GetGlyphs()
        {
            return activeGlyphs.ToArray();
        }

        public static int[] GlyphNums()
        {
            Glyph[] glyphs = activeGlyphs.ToArray();
            List<int> ints = new List<int>();
            for (int i = 0; i < glyphs.Length; i++)
            {
                ints.Add((int)glyphs[i]);
            }
            return ints.ToArray();
        }

        public static int GetGlyphCount()
        {
            return activeGlyphs.Count;
        }

        public static string GetDescription(Glyph? glyph)
        {
            var field = glyph.GetType().GetField(glyph.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? "No description available" : attribute.Description;
        }

        public static Texture2D GetGlyphImage(Glyph glyph)
        {
            return glyphImage[(int)glyph];
        }

        public static void RemoveAllGlyphs()
        {
            activeGlyphs = new HashSet<Glyph>();
        }

        public static void SetUnlockedGlyphs()
        {
            //Base unlocked glyphs
            unlockedGlyphs.Add(Glyph.NoGlyphsLeft);
            unlockedGlyphs.Add(Glyph.A);
            unlockedGlyphs.Add(Glyph.B);
            unlockedGlyphs.Add(Glyph.D);
            unlockedGlyphs.Add(Glyph.Anubis);
            unlockedGlyphs.Add(Glyph.Bread);
            unlockedGlyphs.Add(Glyph.One);
            unlockedGlyphs.Add(Glyph.Life);
            unlockedGlyphs.Add(Glyph.Scarab);
            unlockedGlyphs.Add(Glyph.Ten);
            unlockedGlyphs.Add(Glyph.Flower);
            
            Dictionary<UnlockType, bool> unlocks = UnlockManager.LoadUnlocks();
            if (unlocks != null)
            {
                if (unlocks[UnlockType.Cat]) unlockedGlyphs.Add(Glyph.Cat);
                if (unlocks[UnlockType.Papyrus]) unlockedGlyphs.Add(Glyph.Papyrus);
                if (unlocks[UnlockType.Thousand]) unlockedGlyphs.Add(Glyph.Thousand);
                if (unlocks[UnlockType.J]) unlockedGlyphs.Add(Glyph.J);
                if (unlocks[UnlockType.S]) unlockedGlyphs.Add(Glyph.S);
                if (unlocks[UnlockType.Crocodile]) unlockedGlyphs.Add(Glyph.Crocodile);
                if (unlocks[UnlockType.EyeOfHorus]) unlockedGlyphs.Add(Glyph.EyeOfHorus);
                if (unlocks[UnlockType.H]) unlockedGlyphs.Add(Glyph.H);
                if (unlocks[UnlockType.Heart]) unlockedGlyphs.Add(Glyph.Heart);
                if (unlocks[UnlockType.House]) unlockedGlyphs.Add(Glyph.House);
                if (unlocks[UnlockType.Hundred]) unlockedGlyphs.Add(Glyph.Hundred);
                if (unlocks[UnlockType.King]) unlockedGlyphs.Add(Glyph.King);
                if (unlocks[UnlockType.M]) unlockedGlyphs.Add(Glyph.M);
                if (unlocks[UnlockType.Man]) unlockedGlyphs.Add(Glyph.Man);
                if (unlocks[UnlockType.N]) unlockedGlyphs.Add(Glyph.N);
                if (unlocks[UnlockType.Osiris]) unlockedGlyphs.Add(Glyph.Osiris);
                if (unlocks[UnlockType.R]) unlockedGlyphs.Add(Glyph.R);
                if (unlocks[UnlockType.Snake]) unlockedGlyphs.Add(Glyph.Snake);
                if (unlocks[UnlockType.Star]) unlockedGlyphs.Add(Glyph.Star);
                if (unlocks[UnlockType.Sun]) unlockedGlyphs.Add(Glyph.Sun);
                if (unlocks[UnlockType.Water]) unlockedGlyphs.Add(Glyph.Water);
                if (unlocks[UnlockType.Woman]) unlockedGlyphs.Add(Glyph.Woman);
            }
        }
    }
}