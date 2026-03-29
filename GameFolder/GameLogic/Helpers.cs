using System.Collections.Generic;
using System.Text;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder
{
    public partial class GameLogic
    {
        public static void SetContext(int x, int y, int level = 1)
        {
            unchecked
            {
                int h = seed;
                h = h * 4321;
                h ^= x * 505;
                h ^= y * 807;
                h ^= level * 123;
                contextRandom = new System.Random(h);
            }
        }

        private string RandomTextGenerate(int length)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                if (unseededRandom.NextDouble() >= 1 - enhancements.shinyChance) shinyWords.Add(i);
                else if (unseededRandom.NextDouble() >= 1 - enhancements.bloomChance) bloomWords.Add(i);
                else if (unseededRandom.NextDouble() >= 1 - enhancements.stoneChance) stoneWords.Add(i);
                
                string word = jsonStrings[contextRandom.Next(0, jsonStrings.Count)];
                if (GlyphManager.IsActive(Glyph.Snake) && unseededRandom.Next(0, 16) == 12)
                {
                    char[] wordToChar = word.ToCharArray();
                    wordToChar[unseededRandom.Next(0, word.Length)] = (char)(unseededRandom.Next(0, 26) + 'a');
                    word = new string(wordToChar);
                }
                stringBuilder.Append(word + " ");
            }
            return stringBuilder.ToString();
        }

        public static string WrapText(Microsoft.Xna.Framework.Graphics.SpriteFont font, string text, float maxWidth)
        {
            StringBuilder result = new StringBuilder();
            StringBuilder line = new StringBuilder();
            foreach (string word in text.Split(' '))
            {
                string test = line.Length == 0 ? word : line + " " + word;
                if (font.MeasureString(test).X > maxWidth)
                {
                    if (result.Length > 0) result.Append('\n');
                    result.Append(line);
                    line.Clear();
                    line.Append(word);
                }
                else
                {
                    if (line.Length > 0) line.Append(' ');
                    line.Append(word);
                }
            }
            if (line.Length > 0)
            {
                if (result.Length > 0) result.Append('\n');
                result.Append(line);
            }
            return result.ToString();
        }

        public static bool IsFight(NodeType nodeType) =>
            nodeType is NodeType.FIGHT or NodeType.ELITE or NodeType.BOSS;

        private static LetterUpgrade GenerateRewardCard(List<char> usedChars, bool mult, int valMin, int valMax)
        {
            char ch = (char)(contextRandom.Next(0, 26) + 'a');
            while (usedChars.Contains(ch))
            {
                ch = (char)(contextRandom.Next(0, 26) + 'a');
            }
            return new LetterUpgrade(ch, mult, contextRandom.Next(valMin, valMax), 0);
        }
    }
}