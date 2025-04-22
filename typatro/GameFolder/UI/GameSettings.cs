using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace typatro.GameFolder.UI{
    public static class ThemeColors
    {
        public static Color Background;
        public static Color Foreground;
        public static Color Extra;
        public static Color Selected;
        public static Color NotSelected;
        public static Color NodeSelect;
        public static Color Text;
        public static Color Correct;
        public static Color Wrong;

        public static void Apply(string themeName)
        {
            switch (themeName)
            {
                case "pink":
                    Background = new Color(255,133,222);
                    Foreground = new Color(215,75,177);
                    Extra = new Color(179,33,139);
                    Selected = new Color(255,186,225);
                    NotSelected = new Color(215,75,177);
                    NodeSelect = new Color(159,207,251);
                    Text = Color.White;
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    break;

                case "red":
                    Background = new Color(0x0000ff);
                    Foreground = new Color(0x2e0a23);
                    Extra = new Color(0xb11678);
                    Selected = new Color(0xc00ef1);
                    NotSelected = new Color(0x000000);
                    NodeSelect = Color.Crimson;
                    Text = Color.White;
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    break;

                default:
                    Background = Color.White;
                    Foreground = Color.Gray;
                    Extra = Color.Black;
                    Selected = Color.LightGray;
                    NotSelected = Color.DarkGray;
                    NodeSelect = Color.Crimson;
                    Text = Color.Silver;
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    break;
            }
        }
    }

    public static class SettingsManager
    {
        public static string theme = "pink";
        public static int volume = 10;
        private static readonly string path = "settings.json";

        public static void Save(string theme, int volume)
        {
            string[] save = new string[] {theme, volume.ToString()};
            var json = JsonSerializer.Serialize(save, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static string[] Load()
        {
            if (!File.Exists(path)) return new string[]{"pink",10.ToString()};
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<string[]>(json);
        }
    }
}