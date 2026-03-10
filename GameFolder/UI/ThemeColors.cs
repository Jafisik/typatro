using Microsoft.Xna.Framework;

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
                    Foreground = new Color(49, 64, 38, 80);
                    Selected = new Color(115, 149, 89, 80);
                    NotSelected = new Color(82, 106, 64, 80);
                    ShopReroll = new Color(70, 100, 70, 160);
                    ExitShop = new Color(30, 60, 30, 160);
                    Text = new Color(190, 240, 145, 230);
                    Mouse = Color.White;
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    Blocked = Color.Yellow;
                    break;

                case 1: //PINK
                    Background = new Color(48, 48, 48, 150);
                    Foreground = new Color(165, 62, 118, 150);
                    Selected = new Color(226, 87, 151, 150);
                    NotSelected = new Color(142, 53, 99, 150);
                    ShopReroll = new Color(110, 60, 80, 160);
                    ExitShop = new Color(80, 30, 50, 160);
                    Text = Color.White;
                    Mouse = Color.Purple;
                    Correct = Color.Green;
                    Wrong = Color.LightBlue;
                    Blocked = Color.Yellow;
                    break;

                case 2: //BLUE
                    Background = new Color(24, 24, 32, 80);
                    Foreground = new Color(44, 72, 117, 80);
                    Selected = new Color(188, 80, 176, 80);
                    NotSelected = new Color(138, 80, 143, 80);
                    ShopReroll = new Color(60, 60, 110, 160);
                    ExitShop = new Color(30, 30, 80, 160);
                    Text = Color.White;
                    Mouse = Color.LightSkyBlue;
                    Correct = Color.Green;
                    Wrong = Color.Red;
                    Blocked = Color.Yellow;
                    break;


                case 3: //RED
                    Background = new Color(32, 16, 16, 80);
                    Foreground = new Color(177, 30, 72, 80);
                    Selected = new Color(100, 85, 85, 120);
                    NotSelected = new Color(80, 33, 44, 120);
                    ShopReroll = new Color(130, 40, 40, 160);
                    ExitShop = new Color(120, 15, 15, 180);
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
    }
}