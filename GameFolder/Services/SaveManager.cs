using System;
using System.Collections.Generic;
using System.Text.Json;
using typatro.GameFolder.Models;
using typatro.GameFolder.Upgrades;
using System.IO;


namespace typatro.GameFolder.Services
{
    public static class SaveManager
    {
        public static int theme = 0;
        public static int volume = 5;
        public static int size = 0;
        public static int fullscreen = 0;

        private static readonly string settingsPath = "settings.json";
        private static readonly string gameSavePath = "gameSave.json";

        public static void SaveSettings(int theme, int volume, int size, int fullScreen)
        {
            int[] save = new int[] { theme, volume, size, fullScreen };
            var json = JsonSerializer.Serialize(save, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, json);
        }

        public static int[] LoadSettings()
        {
            if (!File.Exists(settingsPath)) return new int[] { 0, 10, 0, 0 };
            var json = File.ReadAllText(settingsPath);
            return JsonSerializer.Deserialize<int[]>(json);
        }

        public static void SaveGame(int seed, int level, long coins, MapNode mapNode, Enhancements enhancements, int difficulty, int rune, List<int[]> visited)
        {
            int[] mapNodePos = mapNode.NodePos();
            if (mapNode.column == 12)
            {
                mapNodePos = new int[] { 0, 0 };
            }

            GameSaveData gameSaveData = new GameSaveData()
            {
                seed = seed,
                level = level,
                coins = coins,
                mapNode = mapNodePos,
                letterScores = enhancements.letters,
                enhancements = new int[] { enhancements.damageResist, enhancements.mistakeBlock },
                enhChances = new double[] { enhancements.shinyChance, enhancements.stoneChance, enhancements.bloomChance, enhancements.streakMult },
                glyphs = GlyphManager.GlyphNums(),
                difficulty = difficulty,
                rune = rune,
                visitedNodes = visited,
            };
            string json = JsonSerializer.Serialize(gameSaveData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(gameSavePath, json);
        }

        public static void RemoveGameData()
        {
            File.WriteAllText(gameSavePath, "");
        }

        public static GameSaveData LoadGame()
        {
            if (!File.Exists(gameSavePath)) return null;
            var json = File.ReadAllText(gameSavePath);
            try
            {
                return JsonSerializer.Deserialize<GameSaveData>(json);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Couldn't read game data: " + e.Message);
                return null;
            }
        }

    }
}
