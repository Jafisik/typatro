using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace typatro.GameFolder.UI{
    public static class ThemeColors
    {
        public static Color Background;
        public static Color Foreground;
        public static Color Selected;
        public static Color NotSelected;
        public static Color Text;
        public static Color Correct;
        public static Color Wrong;

        public static void Apply(int themeName)
        {
            switch (themeName)
            {
                case 0: //BLACK
                    Background = new Color(0x0d1511);
                    Foreground = new Color(0x264031);
                    Selected = new Color(0x599573);
                    NotSelected = new Color(0x406a52);
                    Text = new Color(0x73bf94);
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    break;

                case 1: //PINK
                    Background = new Color(255,133,222);
                    Foreground = new Color(215,75,177);
                    Selected = new Color(255,186,225);
                    NotSelected = new Color(215,75,177);
                    Text = Color.White;
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    break;

                case 2: //BLUE
                    Background = new Color(0x5c3f00);
                    Foreground = new Color(0x75482c);
                    Selected = new Color(0xb050bc);
                    NotSelected = new Color(0x8f508a);
                    Text = new Color(0x80d3af);
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    break;


                case 3: //RED
                    Background = new Color(0x242cae);
                    Foreground = new Color(0x181eb1);
                    Selected = new Color(0x342502);
                    NotSelected = new Color(0x0c1137);
                    Text = Color.Black;
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    break;

                default:
                    Background = Color.White;
                    Foreground = Color.Gray;
                    Selected = Color.LightGray;
                    NotSelected = Color.DarkGray;
                    Text = Color.Silver;
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    break;
            }
        }
    }

    public static class SettingsManager
    {
        public static int theme = 0;
        public static int volume = 10;
        private static readonly string path = "settings.json";

        public static void Save(int theme, int volume)
        {
            int[] save = new int[] {theme, volume};
            var json = JsonSerializer.Serialize(save, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static int[] Load()
        {
            if (!File.Exists(path)) return new int[]{0,10};
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<int[]>(json);
        }
    }
}