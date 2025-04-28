using System;
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
    }

    public static class SaveManager
    {
        public static int theme = 0;
        public static int volume = 10;
        private static readonly string settingsPath = "settings.json";
        private static readonly string gameSavePath = "gameSave.json";
        private static readonly string actionSavePath = "actionSave.json";

        public static void SaveSettings(int theme, int volume){
            int[] save = new int[] {theme, volume};
            var json = JsonSerializer.Serialize(save, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, json);
        }

        public static int[] LoadSettings(){
            if (!File.Exists(settingsPath)) return new int[]{0,10};
            var json = File.ReadAllText(settingsPath);
            return JsonSerializer.Deserialize<int[]>(json);
        }

        public static void SaveGame(int seed, int level, long coins, MapNode mapNode, Enhancements enhancements){
            GameSaveData gameSaveData = new GameSaveData(){
                seed = seed,
                level = level,
                coins = coins,
                mapNode = mapNode.NodePos(),
                letterScores = enhancements.letters,
                enhancements = new int[]{enhancements.wordScore, enhancements.damageResist, enhancements.startingScore},
                glyphs = GlyphManager.GlyphNums(),
            };
            string json = JsonSerializer.Serialize(gameSaveData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(gameSavePath, json);
        }

        public static GameSaveData LoadGame(){
            if (!File.Exists(gameSavePath)) return null;
            var json = File.ReadAllText(gameSavePath);
            GameSaveData ret = null;
            try{
                ret = JsonSerializer.Deserialize<GameSaveData>(json);
            } catch(Exception e){
                Console.WriteLine("Couldn't read game data: " + e.Message);
            }
            return ret;
        }

        public static void SaveActions(List<UserAction> actions){
            UserAction[] userActions = actions.ToArray();
            List<string[]> actionStrings = new List<string[]>();
            foreach(UserAction action in userActions){
                actionStrings.Add(action.ToStringArray());
            }
            string json = JsonSerializer.Serialize(actionStrings.ToArray(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(actionSavePath, json);
        }

        public static UserAction[] LoadActions(){
            if (!File.Exists(actionSavePath)) return null;

            var json = File.ReadAllText(actionSavePath);
            try {
                string[][] actionStrings = JsonSerializer.Deserialize<string[][]>(json);
                return actionStrings.Select(UserAction.FromStringArray).ToArray();
            } catch(Exception e){
                Console.WriteLine("Couldn't read game data: " + e.Message);
                return null;
            }
        }
    }
}