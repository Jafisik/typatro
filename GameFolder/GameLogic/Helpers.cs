using System.Collections.Generic;
using System.Text;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;
using typatro.GameFolder.Services;
using static typatro.GameFolder.Services.EnemyManager;

namespace typatro.GameFolder
{
    public partial class GameLogic
    {
        private static readonly Dictionary<char, char> zmeiLookalikes = new()
        {
            ['i'] = 'l', ['l'] = 'i',
            ['m'] = 'n', ['n'] = 'm',
            ['u'] = 'v', ['v'] = 'u',
            ['c'] = 'e', ['e'] = 'c',
            ['g'] = 'q', ['q'] = 'g',
            ['b'] = 'd', ['d'] = 'b',
        };

        // Applies the active enemy's word-generation effects (Kudlanka, Revenant, Yuki, Zmei)
        private static string MutateWordForEnemy(string word)
        {
            if (word.Length == 0) return word;

            if (Is(EnemyType.K) && word.Length > 1)
                word = word.Substring(0, word.Length - 1);

            if (Is(EnemyType.R))
            {
                char[] reversed = word.ToCharArray();
                System.Array.Reverse(reversed);
                word = new string(reversed);
            }

            if (Is(EnemyType.Z))
            {
                char[] chars = word.ToCharArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    if (zmeiLookalikes.TryGetValue(chars[i], out char lookalike) && unseededRandom.NextDouble() < 0.2)
                        chars[i] = lookalike;
                }
                word = new string(chars);
            }

            if (Is(EnemyType.Y) && word.Length > 3 && unseededRandom.NextDouble() < 0.3)
            {
                int insertAt = unseededRandom.Next(1, word.Length);
                word = word.Substring(0, insertAt) + " " + word.Substring(insertAt);
            }

            return word;
        }

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

        private string RandomTextGenerate(int length, bool allowSpecialWords = true)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                if (allowSpecialWords)
                {
                    if (unseededRandom.NextDouble() >= 1 - enhancements.shinyChance) shinyWords.Add(i);
                    else if (unseededRandom.NextDouble() >= 1 - enhancements.bloomChance) bloomWords.Add(i);
                    else if (unseededRandom.NextDouble() >= 1 - enhancements.stoneChance) stoneWords.Add(i);
                }

                string word = jsonStrings[contextRandom.Next(0, jsonStrings.Count)];
                if (GlyphManager.IsActive(Glyph.Snake) && unseededRandom.Next(0, 16) == 12)
                {
                    char[] wordToChar = word.ToCharArray();
                    wordToChar[unseededRandom.Next(0, word.Length)] = (char)(unseededRandom.Next(0, 26) + 'a');
                    word = new string(wordToChar);
                }
                word = MutateWordForEnemy(word);
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

        // Truncates the hint text right after the word currently being typed, hiding
        // everything further ahead (Dybbuk). Reveals the next word as soon as the current
        // one's letters are all typed, even before the space is pressed. Only affects what's
        // drawn, not the text Writer compares against, so typing correctness is unaffected.
        public static string MaskUpcomingWords(string text, int typedCount)
        {
            if (typedCount >= text.Length) return text;
            int wordEnd = text.IndexOf(' ', typedCount);
            if (wordEnd < 0) return text;
            if (typedCount == wordEnd)
            {
                int nextWordEnd = text.IndexOf(' ', wordEnd + 1);
                wordEnd = nextWordEnd < 0 ? text.Length : nextWordEnd;
            }
            return text.Substring(0, wordEnd);
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
