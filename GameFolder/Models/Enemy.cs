using Microsoft.Xna.Framework.Graphics;
using typatro.GameFolder.Services;

namespace typatro.GameFolder.Models
{
    public class Enemy
    {
        public Texture2D Texture { get; }
        public string Description { get; }
        public EnemyType Type { get; }

        public Enemy(Texture2D texture, string description, EnemyType type)
        {
            Texture = texture;
            Description = description;
            Type = type;
        }
    }
}
