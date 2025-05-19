using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using typatro.GameFolder.Upgrades;

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
                    Foreground = new Color(0x481eb1);
                    Selected = new Color(0x444444);
                    NotSelected = new Color(0x2c2137);
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

    public class GameSaveData{
        public int[] mapNode {get;set;}
        public long[] letterScores {get;set;}
        public int[] enhancements {get;set;}
        public int[] glyphs {get;set;}
        public long coins {get;set;}
        public int level {get;set;}
        public int seed {get;set;}
        public int difficulty {get;set;}
        public int rune {get;set;}
        public List<int[]> visitedNodes {get;set;}
    }

    public static class SaveManager
    {
        public static int theme = 0;
        public static int volume = 5;
        public static int size = 0;
        public static int fullscreen = 0;

        private static readonly string settingsPath = "settings.json";
        private static readonly string gameSavePath = "gameSave.json";
        private static readonly string actionSavePath = "actionSave.json";
        private static readonly string achievementsPath = "achievements.json";

        private static Dictionary<string, bool> achievements = new();

        private static readonly string[] allAchievements = new[]
        {
            "characterTutorial", "mapTutorial", "fightTutorial", "shopTutorial",
            "first_kill",
            "uruz0", "uruz1", "uruz2", "uruz3", "uruz4", "uruz5", "uruz6",
            "halagaz0", "halagaz1", "halagaz2", "halagaz3", "halagaz4", "halagaz5", "halagaz6",
            "naudhiz0", "naudhiz1", "naudhiz2", "naudhiz3", "naudhiz4", "naudhiz5", "naudhiz6",
            "jera0", "jera1", "jera2", "jera3", "jera4", "jera5", "jera6"
        };

        static SaveManager()
        {
            LoadAchievements();
            EnsureAllAchievementsExist();
        }

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
            GameSaveData gameSaveData = new GameSaveData()
            {
                seed = seed,
                level = level,
                coins = coins,
                mapNode = mapNode.NodePos(),
                letterScores = enhancements.letters,
                enhancements = new int[] { enhancements.wordScore, enhancements.damageResist, enhancements.startingScore },
                glyphs = GlyphManager.GlyphNums(),
                difficulty = difficulty,
                rune = rune,
                visitedNodes = visited,
            };
            string json = JsonSerializer.Serialize(gameSaveData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(gameSavePath, json);
            Console.WriteLine("Game saved");
        }

        public static void RemoveGameData()
        {
            File.WriteAllText(gameSavePath, "");
            File.WriteAllText(actionSavePath, "");
        }

        public static GameSaveData LoadGame()
        {
            Console.WriteLine("Game loaded");
            if (!File.Exists(gameSavePath)) return null;
            var json = File.ReadAllText(gameSavePath);
            try
            {
                return JsonSerializer.Deserialize<GameSaveData>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't read game data: " + e.Message);
                return null;
            }
        }

        public static void SaveActions(List<UserAction> actions)
        {
            var actionStrings = actions.Select(a => a.ToStringArray()).ToArray();
            string json = JsonSerializer.Serialize(actionStrings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(actionSavePath, json);
            Console.WriteLine("Actions saved");
        }

        public static UserAction[] LoadActions()
        {
            if (!File.Exists(actionSavePath)) return null;
            Console.WriteLine("Actions loaded");
            var json = File.ReadAllText(actionSavePath);
            try
            {
                string[][] actionStrings = JsonSerializer.Deserialize<string[][]>(json);
                return actionStrings.Select(UserAction.FromStringArray).ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't read actions: " + e.Message);
                return null;
            }
        }

        public static void UnlockAchievement(string id)
        {
            if (achievements.ContainsKey(id) && !achievements[id])
            {
                achievements[id] = true;
                SaveAchievements();
                Console.WriteLine($"Achievement unlocked: {id}");
            }
        }

        public static bool IsAchievementUnlocked(string id)
        {
            return achievements.TryGetValue(id, out bool value) && value;
        }

        public static Dictionary<string, bool> GetAllAchievements()
        {
            return new Dictionary<string, bool>(achievements);
        }

        private static void SaveAchievements()
        {
            var json = JsonSerializer.Serialize(achievements, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(achievementsPath, json);
        }

        private static void LoadAchievements()
        {
            if (File.Exists(achievementsPath))
            {
                var json = File.ReadAllText(achievementsPath);
                achievements = JsonSerializer.Deserialize<Dictionary<string, bool>>(json) ?? new();
            }
        }

        private static void EnsureAllAchievementsExist()
        {
            foreach (var id in allAchievements)
            {
                if (!achievements.ContainsKey(id))
                    achievements[id] = false;
            }
            SaveAchievements();
        }
    }
}