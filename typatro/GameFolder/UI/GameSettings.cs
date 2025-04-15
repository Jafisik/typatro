using System.IO;
using System.Text.Json;

namespace typatro.GameFolder.UI{
    public class GameSettings{
        public string Theme { get; set; } = "light";
        public float Volume { get; set; } = 1.0f;
    }

    public static class SettingsManager
    {
        private static readonly string path = "settings.json";

        public static void Save(GameSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static GameSettings Load()
        {
            if (!File.Exists(path)) return new GameSettings();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<GameSettings>(json);
        }
    }
}