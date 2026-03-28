using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace typatro.GameFolder.UI{
    public static class ThemeColors
    {
        public static Color Background;
        public static Color Foreground;
        public static Color Selected;
        public static Color NotSelected;
        public static Color ShopReroll;
        public static Color ExitShop;
        public static Color Text;
        public static Color Mouse;
        public static Color Correct;
        public static Color Wrong;
        public static Color Blocked;

        public static void Apply(int themeName)
        {
            switch (themeName)
            {
                case 0: //GREEN
                    Background = new Color(24, 32, 24, 80);
                    Foreground = new Color(49, 64, 38, 210);
                    Selected = new Color(115, 149, 89, 220);
                    NotSelected = new Color(82, 106, 64, 210);
                    ShopReroll = new Color(70, 100, 70, 220);
                    ExitShop = new Color(30, 60, 30, 220);
                    Text = new Color(190, 240, 145, 230);
                    Mouse = Color.White;
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    Blocked = Color.Yellow;
                    break;

                case 1: //PINK
                    Background = new Color(48, 48, 48, 150);
                    Foreground = new Color(165, 62, 118, 210);
                    Selected = new Color(226, 87, 151, 220);
                    NotSelected = new Color(142, 53, 99, 210);
                    ShopReroll = new Color(110, 60, 80, 220);
                    ExitShop = new Color(80, 30, 50, 220);
                    Text = Color.White;
                    Mouse = Color.Purple;
                    Correct = Color.Green;
                    Wrong = Color.LightBlue;
                    Blocked = Color.Yellow;
                    break;

                case 2: //BLUE
                    Background = new Color(24, 24, 32, 80);
                    Foreground = new Color(44, 72, 117, 210);
                    Selected = new Color(188, 80, 176, 220);
                    NotSelected = new Color(138, 80, 143, 210);
                    ShopReroll = new Color(60, 60, 110, 220);
                    ExitShop = new Color(30, 30, 80, 220);
                    Text = Color.White;
                    Mouse = Color.LightSkyBlue;
                    Correct = Color.Green;
                    Wrong = Color.Red;
                    Blocked = Color.Yellow;
                    break;


                case 3: //RED
                    Background = new Color(32, 16, 16, 80);
                    Foreground = new Color(177, 30, 72, 210);
                    Selected = new Color(100, 85, 85, 220);
                    NotSelected = new Color(80, 33, 44, 210);
                    ShopReroll = new Color(130, 40, 40, 220);
                    ExitShop = new Color(120, 15, 15, 220);
                    Text = new Color(255, 255, 220);
                    Mouse = Color.LightCoral;
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    Blocked = Color.Yellow;
                    break;

                default:
                    Background = Color.White;
                    Foreground = Color.Gray;
                    Selected = Color.LightGray;
                    NotSelected = Color.DarkGray;
                    ShopReroll = Color.Green;
                    ExitShop = Color.DarkGreen;
                    Text = Color.Silver;
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    Blocked = Color.Yellow;
                    break;
            }
        }

        public static void DrawGlowCorners(Rectangle rect, Color color, int cornerLen = 36, int thickness = 3)
        {
            // top-left
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(rect.X, rect.Y, cornerLen, thickness), color);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(rect.X, rect.Y, thickness, cornerLen / 2), color);
            // top-right
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(rect.Right - cornerLen, rect.Y, cornerLen, thickness), color);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(rect.Right - thickness, rect.Y, thickness, cornerLen / 2), color);
            // bottom-left
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(rect.X, rect.Bottom - thickness, cornerLen, thickness), color);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(rect.X, rect.Bottom - cornerLen / 2, thickness, cornerLen / 2), color);
            // bottom-right
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(rect.Right - cornerLen, rect.Bottom - thickness, cornerLen, thickness), color);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(rect.Right - thickness, rect.Bottom - cornerLen / 2, thickness, cornerLen / 2), color);
        }
    }
}