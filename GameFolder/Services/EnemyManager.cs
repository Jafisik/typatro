using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using typatro.GameFolder.Models;

namespace typatro.GameFolder.Services
{
    public enum EnemyType
    {
        None,
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        Elite, Boss
    }

    public static class EnemyManager
    {
        public static EnemyType ActiveEnemy { get; private set; } = EnemyType.None;

        public static void SetActive(EnemyType type) => ActiveEnemy = type;
        public static void ClearActive() => ActiveEnemy = EnemyType.None;
        public static bool Is(EnemyType type) => ActiveEnemy == type;

        public static Enemy Get(EnemyType type)
        {
            foreach (Enemy enemy in Normal) if (enemy.Type == type) return enemy;
            foreach (Enemy enemy in Elite) if (enemy.Type == type) return enemy;
            foreach (Enemy enemy in Boss) if (enemy.Type == type) return enemy;
            return null;
        }

        public static Enemy[] Normal { get; private set; }
        public static Enemy[] Elite { get; private set; }
        public static Enemy[] Boss { get; private set; }

        public static void Load(ContentManager content)
        {
            Normal = new Enemy[]
            {
                new(content.Load<Texture2D>("Images/a"), "Apnea: the text wavers like underwater", EnemyType.A),
                new(content.Load<Texture2D>("Images/b"), "Baul: lose 10 coins for each mistake", EnemyType.B),
                new(content.Load<Texture2D>("Images/e"), "Echidna: letter 'e' scores 0", EnemyType.E),
                new(content.Load<Texture2D>("Images/f"), "Fright: absolutely nothing ;)", EnemyType.F),
                new(content.Load<Texture2D>("Images/g"), "Geist: the screen rotates", EnemyType.G),
                new(content.Load<Texture2D>("Images/i"), "Ictus: the screen randomly flashes inverted", EnemyType.I),
                new(content.Load<Texture2D>("Images/j"), "Jaguar: breaks your streak if you're slow", EnemyType.J),
                new(content.Load<Texture2D>("Images/o"), "Oculus: letter 'o' scores 0", EnemyType.O),
                new(content.Load<Texture2D>("Images/u"), "Urtica: your mistakes don't show up", EnemyType.U),
                new(content.Load<Texture2D>("Images/w"), "Wendigo: fills the screen with bugs", EnemyType.W),
                new(content.Load<Texture2D>("Images/v"), "Vrykolakas: multiplier decreases 0.025x per sec", EnemyType.V),
                new(content.Load<Texture2D>("Images/y"), "Yuki: adds random spaces into words", EnemyType.Y),
                new(content.Load<Texture2D>("Images/d"), "Dybbuk: only shows the word you're currently typing", EnemyType.D),
                new(content.Load<Texture2D>("Images/x"), "Xiphos: 1 mistake is worth 5", EnemyType.X),
            };

            Elite = new Enemy[]
            {
                new(content.Load<Texture2D>("Images/c"), "Cuscuta: hides your scorebar", EnemyType.C),
                new(content.Load<Texture2D>("Images/m"), "Moloch: turns the screen black every 4 seconds", EnemyType.M),
                new(content.Load<Texture2D>("Images/t"), "Torso: each correct letter gives it +1, incorrect letters give -10", EnemyType.T),
                new(content.Load<Texture2D>("Images/s"), "Samael: finish in a minute or you die from poison", EnemyType.S),
                new(content.Load<Texture2D>("Images/k"), "Kudlanka: removes the last letter from each word", EnemyType.K),
                new(content.Load<Texture2D>("Images/l"), "Leecher: drains 1 coin every 3 seconds", EnemyType.L),
                new(content.Load<Texture2D>("Images/p"), "Poleman: respawns when killed", EnemyType.P),
                new(content.Load<Texture2D>("Images/q"), "Quietus: the smoke thickens over a minute", EnemyType.Q),
            };
            Boss = new Enemy[]
            {
                new(content.Load<Texture2D>("Images/h"), "kHeper: blocks all damage every 5 seconds", EnemyType.H),
                new(content.Load<Texture2D>("Images/n"), "Nix: mistake costs you 1 coin, if you reach 0 coins you die", EnemyType.N),
                new(content.Load<Texture2D>("Images/r"), "Revenant: the text is written backwards", EnemyType.R),
                new(content.Load<Texture2D>("Images/z"), "Zmei: changes some similar looking letters", EnemyType.Z),
            };
        }
    }
}
