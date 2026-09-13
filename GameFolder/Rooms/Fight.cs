using System;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder.Rooms{
    public class Fight
    {
        public int cashGain, difficulty, speed, scoreNeeded, words;
        public static float multPerDiff = 1.5f;

        public Fight(int difficulty, int words, int cashGain, int speed, int scoreNeeded)
        {
            this.difficulty = difficulty;
            this.words = words;
            this.cashGain = cashGain;
            this.speed = speed;
            this.scoreNeeded = scoreNeeded;
        }

        public static Fight Create(int difficulty, int level, int floor, int row)
        {
            return new Fight(
                difficulty,
                WordsGen(difficulty, level, floor, row),
                CashGainGen(level, floor, difficulty, row),
                SpeedGen(level, floor, difficulty, row),
                ScoreNeededGen(level, floor, difficulty)
            );
        }

        // Deterministic per (level, floor, row, difficulty, salt) so the map preview always
        // matches what you actually get when you enter the fight, but still varies between
        // runs since it factors in the run seed. Row is needed too: at the start of a map all
        // 4 starting paths share the same level/floor/difficulty and only differ by row, so
        // without it they'd all roll the identical factor. Uses the MurmurHash3 finalizer
        // instead of System.Random(seed) - nearby seeds (like salt 1,2,3 for the three
        // stats of the same node) produce near-identical first draws from System.Random,
        // which made reward/length/damage all look the same.
        private static float RandomFactor(int level, int floor, int row, int difficulty, int salt)
        {
            unchecked
            {
                uint x = (uint)(GameLogic.seed * 374761393 + level * 668265263 + floor * 2246822519 + row * 3403627957 + difficulty * 3266489917 + salt * 2654435761);
                x ^= x >> 15;
                x *= 0x85ebca6bu;
                x ^= x >> 13;
                x *= 0xc2b2ae35u;
                x ^= x >> 16;
                return 0.5f + (x / (float)uint.MaxValue);
            }
        }

        public static int CashGainGen(int level, int floor, int difficulty, int row)
        {
            // Tutorial map's elite node (seed is always 10 there): the formula happens to
            // roll low for that exact spot, showing less reward than the easier normal fight
            // right next to it - fake a better number instead of reworking the formula for
            // one node. This runs through the same CashGainGen the map preview calls, so the
            // preview and the actual fight still always agree.
            if (GameLogic.seed == 10 && level == 1 && floor == 2 && difficulty == 2 && row == 0)
                return 25;

            float factor = RandomFactor(level, floor, row, difficulty, 1);
            return (int)((level * 5 + floor) * (difficulty * multPerDiff) * factor);
        }

        public static int SpeedGen(int level, int floor, int difficulty, int row)
        {
            float factor = RandomFactor(level, floor, row, difficulty, 2);
            return (int)((level * level * 0.8 + floor * 0.2) * (difficulty * multPerDiff * (GlyphManager.IsActive(Glyph.S) ? 0.5 : 0.8)) * factor);
        }

        public static int ScoreNeededGen(int level, int floor, int difficulty)
        {
            return (int)(50 + ((level - 1) * (level - 1) * (level - 1) * 80 + floor * floor * 1.5) * (difficulty * multPerDiff * 1.2));
        }

        public static int WordsGen(int difficulty, int level, int floor, int row)
        {
            int baseWords = difficulty switch
            {
                1 => 15,
                2 => 25,
                3 => 45,
                _ => 10
            };
            float factor = RandomFactor(level, floor, row, difficulty, 3);
            return (int)(baseWords * factor);
        }
    }
}
