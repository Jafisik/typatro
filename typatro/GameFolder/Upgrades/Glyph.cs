using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using typatro.GameFolder.UI;

namespace typatro.GameFolder.Upgrades{

    public enum Glyph
    {
        [Description("You can't buy any more glyphs")]
        NoGlyphsLeft,
        [Description("Meaning: \"Sound for 'A', used for names or verbs.\"\n\n+ Adds 5 to all vowels")]
        A,

        [Description("Meaning: \"Sound for 'B', symbol for foot or place.\"\n\n+ Adds +5 to the starting score each fight")]
        B,

        [Description("Meaning: \"Sound for 'D', symbol for hand.\"\n\n+ Multiplies 2 random letter scores by *5")]
        D,

        [Description("Meaning: \"Sound for 'H', symbolizing shelter or building.\"\n\n+ Multiplies 10 random letters by *5\n- Rotates text upside down every 10 seconds")]
        H,

        [Description("Meaning: \"Sound for 'J', also symbol for reed or tall grass.\"\n\n+ Add +10 to all letter scores\n- Screen shakes with every input")]
        J,

        [Description("Meaning: \"Sound for 'M', can symbolize owl, silence or wisdom.\"\n\n+ Gains 10 coins every 10 seconds in a fight\n- Text changes position every 5 seconds")]
        M,

        [Description("Meaning: \"Sound for 'N', ripple of water, also 'to' or 'for'.\"\n\n+ Words written under 3 seconds are doubled\n- Words written over 3 seconds break streak")]
        N,

        [Description("Meaning: \"Sound for 'R', symbolizing mouth or one whole thing.\"\n\n+ Lets you correct your mistakes\n- Mistakes count as -5")]
        R,

        [Description("Meaning: \"Sound for 'S', also sufffix for she, her, hers.\"\n\n+ Elite and boss fights deal less damage")]
        S,

        [Description("Meaning: \"'Sun', representing Ra, the sun god.\"\n\n+ Disables all visual glyphs\n- Letters are less visible")]
        Sun,

        [Description("Meaning: \"'House', represents domestic life or temples.\"\n\n+ Take only quarter damage\n- Every 8 seconds your keyboard stops working")]
        House,

        [Description("Meaning: \"'Water', often representing rivers or offerings.\"\n\n+ Multiplies final score by *2\n- Add -5 to 5 random letter scores")]
        Water,

        [Description("Meaning: \"'King', signifying royalty or divine authority.\"\n\n+ Multiplies ALL highest letter scores by *5\n- Sets ALL letters with lowest letter score to 0")]
        King,

        [Description("Meaning: \"The Eye of Horus, symbolizing protection and health.\"\n\n+ 2 mistakes per round do not count\n- You blink every 5 seconds")]
        EyeOfHorus,

        [Description("Meaning: \"Osiris, the god of the afterlife and resurrection.\"\n\n+ Revive once per run\n- After ressurection all letters multiplied by *0.8")]
        Osiris,

        [Description("Meaning: \"'Woman', symbolizes feminine names or nurture.\"\n\n+ Multiplies random letter scoreby *2 after each fight\n- Earn only 80% coins")]
        Woman,

        [Description("Meaning: \"'Man', used for writing the word 'person'.\"\n\n+ Multiplies coin rewards by *1.5\n- Letter 'x' counts as a mistake")]
        Man,

        [Description("Meaning: \"'Flower', meaning 'to be' or 'to exist'.\"\n\n+ Adds +0.1 to the final score multiplier for each active glyph")]
        Flower,

        [Description("Meaning: \"'Cat', representing the goddess Bastet, protector of the home.\"\n\n+ Multiplies 9 random letter values by *2\n- A cat will be sleeping on a random location")]
        Cat,

        [Description("Meaning: \"Anubis, the god of mummification and protector of tombs.\"\n\n+ Gain 1 coin per 5 words")]
        Anubis,

        [Description("Meaning: \"'Scarab', associated with rebirth and regeneration.\"\n\n+ Streak bonus increases by +3 every consecutive correct word")]
        Scarab,

        [Description("Meaning: \"'Snake', often signifying danger or protection.\"\n\n+ Mistakes do not subtract from score\n- Some letters are wrong")]
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
            if(activeGlyphs.Count >= 7 && !GameLogic.achievmentBools["WOMAN"])
            {
                GameLogic.achievmentBools["WOMAN"] = true;
                GameLogic.writeAchievment = true;
            }
        }
        public static void Remove(Glyph glyph) => activeGlyphs.Remove(glyph);
        public static void RemoveRandom()
        {
            if (activeGlyphs.Count > 0)
            {
                Remove(activeGlyphs.ElementAt(GameLogic.seededRandom.Next(0, activeGlyphs.Count)));
            }
        }
        public static bool IsActive(Glyph glyph) => activeGlyphs.Contains(glyph);

        public static Glyph GetRandomUnusedGlyph()
        {
            if (!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("GetRandomUnusedGlyph", ""));
            var unusedGlyphs = unlockedGlyphs.Except(activeGlyphs).ToList();
            Console.WriteLine(unusedGlyphs.Count);
            if (unusedGlyphs.Count == 0)
            {
                return Glyph.NoGlyphsLeft;
            }
            int index = GameLogic.seededRandom.Next(unusedGlyphs.Count);
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
            
            Dictionary<string, bool> unlocks = SaveManager.LoadUnlocks();
            if (unlocks != null)
            {
                if (unlocks["ANUBIS"]) unlockedGlyphs.Add(Glyph.Anubis);
                if (unlocks["CAT"]) unlockedGlyphs.Add(Glyph.Cat);
                if (unlocks["PAPYRUS"]) unlockedGlyphs.Add(Glyph.Papyrus);
                if (unlocks["THOUSAND"]) unlockedGlyphs.Add(Glyph.Thousand);
                if (unlocks["J"]) unlockedGlyphs.Add(Glyph.J);
                if (unlocks["S"]) unlockedGlyphs.Add(Glyph.S);
                if (unlocks["CROCODILE"]) unlockedGlyphs.Add(Glyph.Crocodile);
                if (unlocks["EYEOFHORUS"]) unlockedGlyphs.Add(Glyph.EyeOfHorus);
                if (unlocks["H"]) unlockedGlyphs.Add(Glyph.H);
                if (unlocks["HEART"]) unlockedGlyphs.Add(Glyph.Heart);
                if (unlocks["HOUSE"]) unlockedGlyphs.Add(Glyph.House);
                if (unlocks["HUNDRED"]) unlockedGlyphs.Add(Glyph.Hundred);
                if (unlocks["KING"]) unlockedGlyphs.Add(Glyph.King);
                if (unlocks["M"]) unlockedGlyphs.Add(Glyph.M);
                if (unlocks["MAN"]) unlockedGlyphs.Add(Glyph.Man);
                if (unlocks["N"]) unlockedGlyphs.Add(Glyph.N);
                if (unlocks["OSIRIS"]) unlockedGlyphs.Add(Glyph.Osiris);
                if (unlocks["R"]) unlockedGlyphs.Add(Glyph.R);
                if (unlocks["SNAKE"]) unlockedGlyphs.Add(Glyph.Snake);
                if (unlocks["STAR"]) unlockedGlyphs.Add(Glyph.Star);
                if (unlocks["SUN"]) unlockedGlyphs.Add(Glyph.Sun);
                if (unlocks["WATER"]) unlockedGlyphs.Add(Glyph.Water);
                if (unlocks["WOMAN"]) unlockedGlyphs.Add(Glyph.Woman);
            }
        }
    }
}