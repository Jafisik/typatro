using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace typatro.GameFolder.Rooms{
    abstract class Fight{
        public int cashGain, difficulty, speed, scoreNeeded;
        public abstract int letters { get; set; }
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
            return (int)((level*3 + floor*0.5) * (difficulty*multPerDiff));
        }

        public static int SpeedGen(int level, int floor, int difficulty){
            return (int)((level + floor*0.2) * (difficulty*multPerDiff*0.8));
        }

        public static int ScoreNeddedGen(int level, int floor, int difficulty){
            return (int)(20+(level*5 + floor*3) * (difficulty*multPerDiff));
        }
    }

    class SpecialFight : Fight{
        int lett = 0;
        public override int letters
        {
            get { return lett; }
            set { lett = value; }
        }
        public SpecialFight(int cashGain, int difficulty, int speed, int letters, int scoreNeeded) : base(cashGain,difficulty,speed){
            this.letters = letters;
            this.scoreNeeded = scoreNeeded;
        }
    }

    class NormalFight : Fight{
        public override int letters { get; set; } = 10;
        public NormalFight(int level, int floor) : base(1) {
            cashGain = CashGainGen(level, floor, difficulty);
            speed = SpeedGen(level, floor, difficulty);
            scoreNeeded = ScoreNeddedGen(level, floor, difficulty);
        }
    }

    class EliteFight : Fight{
        public override int letters { get; set;} = 20;
        public EliteFight(int level, int floor) : base(2) {
            cashGain = CashGainGen(level, floor, difficulty);
            speed = SpeedGen(level, floor, difficulty);
            scoreNeeded = ScoreNeddedGen(level, floor, difficulty);
        }
    }

    class BossFight : Fight{
        public override int letters { get; set;} = 40;

        public BossFight(int level, int floor) : base(3) {
            cashGain = CashGainGen(level, floor, difficulty);
            speed = SpeedGen(level, floor, difficulty);
            scoreNeeded = ScoreNeddedGen(level, floor, difficulty);
        }
    }
}