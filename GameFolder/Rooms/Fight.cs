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

        public string FightInfo()
        {
            return $"Reward: {cashGain}$\nDifficulty: {difficulty}\nSpeed: {speed}";
        }

        public static Fight Create(int difficulty, int level, int floor)
        {
            return new Fight(
                difficulty,
                WordsGen(difficulty),
                CashGainGen(level, floor, difficulty),
                SpeedGen(level, floor, difficulty),
                ScoreNeddedGen(level, floor, difficulty)
            );
        }

        public static int CashGainGen(int level, int floor, int difficulty)
        {
            return (int)((level * 5 + floor) * (difficulty * multPerDiff));
        }

        public static int SpeedGen(int level, int floor, int difficulty)
        {
            return (int)((level * level * 0.8 + floor * 0.2) * (difficulty * multPerDiff * (GlyphManager.IsActive(Glyph.S) ? 0.5 : 0.8)));
        }

        public static int ScoreNeddedGen(int level, int floor, int difficulty)
        {
            return (int)(50 + ((level - 1) * (level - 1) * (level - 1) * 80 + floor * floor * 1.5) * (difficulty * multPerDiff * 1.2));
        }

        public static int WordsGen(int difficulty)
        {
            return difficulty switch
            {
                1 => 15,
                2 => 25,
                3 => 45,
                _ => 10
            };
        }
    }
}
