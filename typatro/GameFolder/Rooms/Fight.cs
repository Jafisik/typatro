using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder.Rooms{
    abstract class Fight{
        public int cashGain, difficulty, speed, scoreNeeded;
        public abstract int words { get; set; }
        public static float multPerDiff = 1.5f;

        public Fight(int difficulty, int cashGain = 1, int speed = 1){
            this.cashGain = cashGain;
            this.difficulty = difficulty;
            this.speed = speed;
        }

        public string FightInfo(){
            return $"Reward: {cashGain}$\nDifficulty: {difficulty}\nSpeed: {speed}";
        }

        public static int CashGainGen(int level, int floor, int difficulty){
            return (int)((level*5 + floor) * (difficulty*multPerDiff));
        }

        public static int SpeedGen(int level, int floor, int difficulty){
            return (int)((level*level*0.8 + floor*0.2) * (difficulty*multPerDiff*(GlyphManager.IsActive(Glyph.S)?0.5:0.8)));
        }

        public static int ScoreNeddedGen(int level, int floor, int difficulty){
            return (int)(50+((level-1)*(level-1)*(level-1)*60 + floor*floor*1.2) * (difficulty*multPerDiff));
        }
    }

    class SpecialFight : Fight{
        int lett = 0;
        public override int words
        {
            get { return lett; }
            set { lett = value; }
        }
        public SpecialFight(int cashGain, int difficulty, int speed, int letters, int scoreNeeded) : base(cashGain,difficulty,speed){
            this.words = letters;
            this.scoreNeeded = scoreNeeded;
        }
    }

    class NormalFight : Fight{
        public override int words { get; set; } = 15;
        public NormalFight(int level, int floor) : base(1) {
            cashGain = CashGainGen(level, floor, difficulty);
            speed = SpeedGen(level, floor, difficulty);
            scoreNeeded = ScoreNeddedGen(level, floor, difficulty);
        }
    }

    class EliteFight : Fight{
        public override int words { get; set;} = 25;
        public EliteFight(int level, int floor) : base(2) {
            cashGain = CashGainGen(level, floor, difficulty);
            speed = SpeedGen(level, floor, difficulty);
            scoreNeeded = ScoreNeddedGen(level, floor, difficulty);
        }
    }

    class BossFight : Fight{
        public override int words { get; set;} = 45;

        public BossFight(int level, int floor) : base(3) {
            cashGain = CashGainGen(level, floor, difficulty);
            speed = SpeedGen(level, floor, difficulty);
            scoreNeeded = ScoreNeddedGen(level, floor, difficulty);
        }
    }
}