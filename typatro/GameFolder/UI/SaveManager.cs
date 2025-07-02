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
        public static Color ShopReroll;
        public static Color ExitShop;
        public static Color Text;
        public static Color Correct;
        public static Color Wrong;

        public static void Apply(int themeName)
        {
            switch (themeName)
            {
                case 0: //GREEN
                    Background = new Color(24,32,24,80);
                    Foreground = new Color(49,64,38,80);
                    Selected = new Color(115,149,89,80);
                    NotSelected = new Color(82, 106, 64, 80);
                    ShopReroll = new Color(70,100,70,160);
                    ExitShop = new Color(30,60,30,160);
                    Text = new Color(190,240,145,230);
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    break;

                case 1: //PINK
                    Background = new Color(48,48,48,150);
                    Foreground = new Color(165,62,118,150);
                    Selected = new Color(226,87,151,150);
                    NotSelected = new Color(142,53,99,150);
                    ShopReroll = new Color(110,60,80,160);
                    ExitShop = new Color(80,30,50,160);
                    Text = Color.White;
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    break;

                case 2: //BLUE
                    Background = new Color(24,24,32,80);
                    Foreground = new Color(44,72,117,80);
                    Selected = new Color(188,80,176,80);
                    NotSelected = new Color(138,80,143,80);
                    ShopReroll = new Color(60,60,110,160);
                    ExitShop = new Color(30,30,80,160);
                    Text = Color.White;
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
                    break;


                case 3: //RED
                    Background = new Color(32,16,16,80);
                    Foreground = new Color(177,30,72,80);
                    Selected = new Color(100,85,85,120);
                    NotSelected = new Color(80,33,44,120);
                    ShopReroll = new Color(130,40,40,160);
                    ExitShop = new Color(120,15,15,180);
                    Text = new Color(255,255,220);
                    Correct = Color.Green;
                    Wrong = Color.DarkRed;
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
                    break;
            }
        }
    }

    public class GameSaveData{
        public int[] mapNode {get;set;}
        public long[] letterScores {get;set;}
        public int[] enhancements {get;set;}
        public double[] enhChances { get; set; }
        public int[] glyphs { get; set; }
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
        private static readonly string unlocksPath = "unlocks.json";

        private static Dictionary<string, bool> unlocks = new();

        private static readonly string[] allUnlocks = new[]
        {
            "characterTutorial", "mapTutorial", "fightTutorial", "shopTutorial",

            "ANUBIS", "CAT", "PAPYRUS", "THOUSAND", "J", "S", "CROCODILE", "EYEOFHORUS",
            "H", "HEART", "HOUSE", "HUNDRED", "KING", "M", "MAN", "N", "OSIRIS", "R",
            "SNAKE", "STAR", "SUN", "WATER", "WOMAN",
            
            "URUZ0", "URUZ1", "URUZ2", "URUZ3", "URUZ4", "URUZ5", "URUZ6",
            "HALAGAZ0", "HALAGAZ1", "HALAGAZ2", "HALAGAZ3", "HALAGAZ4", "HALAGAZ5", "HALAGAZ6",
            "NAUDHIZ0", "NAUDHIZ1", "NAUDHIZ2", "NAUDHIZ3", "NAUDHIZ4", "NAUDHIZ5", "NAUDHIZ6",
            "JERA0", "JERA1", "JERA2", "JERA3", "JERA4", "JERA5", "JERA6"
        };

        static SaveManager()
        {
            LoadUnlocks();
            EnsureAllUnlocksExist();
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
                enhancements = new int[] { enhancements.wordScore, enhancements.damageResist, enhancements.startingScore },
                enhChances = new double[] { enhancements.shinyChance, enhancements.stoneChance, enhancements.bloomChance},
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

        public static void UnlockUnlock(string id)
        {
            if (unlocks.ContainsKey(id) && !unlocks[id])
            {
                unlocks[id] = true;
                SaveUnlocks();
                Console.WriteLine($"Unlock unlocked: {id}");
            }
        }

        public static bool IsUnlockUnlocked(string id)
        {
            return unlocks.TryGetValue(id, out bool value) && value;
        }

        public static Dictionary<string, bool> GetAllUnlocks()
        {
            return new Dictionary<string, bool>(unlocks);
        }

        private static void SaveUnlocks()
        {
            var json = JsonSerializer.Serialize(unlocks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(unlocksPath, json);
        }

        public static Dictionary<string, bool> LoadUnlocks()
        {
            if (File.Exists(unlocksPath))
            {
                var json = File.ReadAllText(unlocksPath);
                unlocks = JsonSerializer.Deserialize<Dictionary<string, bool>>(json) ?? new();
                return unlocks;
            }
            return null;
        }

        private static void EnsureAllUnlocksExist()
        {
            foreach (var id in allUnlocks)
            {
                if (!unlocks.ContainsKey(id))
                    unlocks[id] = false;
            }
            SaveUnlocks();
        }
    }
}