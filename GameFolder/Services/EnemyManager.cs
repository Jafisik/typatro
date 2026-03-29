using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using typatro.GameFolder.Models;

namespace typatro.GameFolder.Services
{
    public enum EnemyType
    {
        None,
        A, B, C, E, F, G, H, I, J, M, O, T, U, W,
        Elite, Boss
    }

    public static class EnemyManager
    {
        public static EnemyType ActiveEnemy { get; private set; } = EnemyType.None;

        public static void SetActive(EnemyType type) => ActiveEnemy = type;
        public static void ClearActive() => ActiveEnemy = EnemyType.None;
        public static bool Is(EnemyType type) => ActiveEnemy == type;

        public static Enemy[] Normal { get; private set; }
        public static Enemy[] Elite { get; private set; }
        public static Enemy[] Boss { get; private set; }

        public static void Load(ContentManager content)
        {
            Normal = new Enemy[]
            {
                new(content.Load<Texture2D>("Images/a"), "Apnea: letter 'a' scores 0", EnemyType.A),
                new(content.Load<Texture2D>("Images/b"), "Baul: lose 10 coins for each mistake", EnemyType.B),
                new(content.Load<Texture2D>("Images/e"), "Echidna: letter 'e' scores 0", EnemyType.E),
                new(content.Load<Texture2D>("Images/f"), "Fright: absolutely nothing ;)", EnemyType.F),
                new(content.Load<Texture2D>("Images/g"), "Geist: the screen rotates", EnemyType.G),
                
                new(content.Load<Texture2D>("Images/i"), "Ictus: letter 'i' scores 0", EnemyType.I),
                new(content.Load<Texture2D>("Images/j"), "Jaguar: breaks your streak if you're slow", EnemyType.J),
                
                new(content.Load<Texture2D>("Images/o"), "Oculus: letter 'o' scores 0", EnemyType.O),
                
                new(content.Load<Texture2D>("Images/u"), "Urtica: letter 'u' scores 0", EnemyType.U),
                new(content.Load<Texture2D>("Images/w"), "Wendigo: fills the screen with bugs", EnemyType.W),
            };

            Elite = new Enemy[]
            {
                new(content.Load<Texture2D>("Images/c"), "Cuscuta: hides your scorebar", EnemyType.C),
                new(content.Load<Texture2D>("Images/m"), "Moloch: turns the screen black every 4 seconds", EnemyType.M),
                new(content.Load<Texture2D>("Images/t"), "Torso: each correct letter gives it +1, incorrect letters give -10", EnemyType.T),
            };
            Boss = new Enemy[]
            {
                new(content.Load<Texture2D>("Images/h"), "kHeper: blocks all damage every 5 seconds", EnemyType.H),
            };
        }
    }
}
