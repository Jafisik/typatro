using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace typatro.GameFolder.Services
{
    public static class UnlockManager
    {

        private static readonly string unlocksPath = "unlocks.json";
        private static Dictionary<UnlockType, bool> unlocks = new();


        static UnlockManager()
        {
            LoadUnlocks();
            EnsureAllUnlocksExist();
            UnlockUnlock(UnlockType.Uruz0);
        }

        public enum UnlockType
        {
            CharacterTutorial,
            MapTutorial,
            FightTutorial,
            ShopTutorial,

            Anubis,
            Cat,
            Papyrus,
            Thousand,
            J,
            S,
            Crocodile,
            EyeOfHorus,
            H,
            Heart,
            House,
            Hundred,
            King,
            M,
            Man,
            N,
            Osiris,
            R,
            Snake,
            Star,
            Sun,
            Water,
            Woman,

            Uruz0, Uruz1, Uruz2, Uruz3, Uruz4, Uruz5, Uruz6,
            Halagaz0, Halagaz1, Halagaz2, Halagaz3, Halagaz4, Halagaz5, Halagaz6,
            Naudhiz0, Naudhiz1, Naudhiz2, Naudhiz3, Naudhiz4, Naudhiz5, Naudhiz6,
            Jera0, Jera1, Jera2, Jera3, Jera4, Jera5, Jera6,
        }

        public static string ToKey(UnlockType unlock) => unlock switch
        {
            UnlockType.CharacterTutorial => "characterTutorial",
            UnlockType.MapTutorial => "mapTutorial",
            UnlockType.FightTutorial => "fightTutorial",
            UnlockType.ShopTutorial => "shopTutorial",

            UnlockType.Anubis => "ANUBIS",
            UnlockType.Cat => "CAT",
            UnlockType.Papyrus => "PAPYRUS",
            UnlockType.Thousand => "THOUSAND",
            UnlockType.J => "J",
            UnlockType.S => "S",
            UnlockType.Crocodile => "CROCODILE",
            UnlockType.EyeOfHorus => "EYEOFHORUS",
            UnlockType.H => "H",
            UnlockType.Heart => "HEART",
            UnlockType.House => "HOUSE",
            UnlockType.Hundred => "HUNDRED",
            UnlockType.King => "KING",
            UnlockType.M => "M",
            UnlockType.Man => "MAN",
            UnlockType.N => "N",
            UnlockType.Osiris => "OSIRIS",
            UnlockType.R => "R",
            UnlockType.Snake => "SNAKE",
            UnlockType.Star => "STAR",
            UnlockType.Sun => "SUN",
            UnlockType.Water => "WATER",
            UnlockType.Woman => "WOMAN",

            UnlockType.Uruz0 => "URUZ0",
            UnlockType.Uruz1 => "URUZ1",
            UnlockType.Uruz2 => "URUZ2",
            UnlockType.Uruz3 => "URUZ3",
            UnlockType.Uruz4 => "URUZ4",
            UnlockType.Uruz5 => "URUZ5",
            UnlockType.Uruz6 => "URUZ6",

            UnlockType.Halagaz0 => "HALAGAZ0",
            UnlockType.Halagaz1 => "HALAGAZ1",
            UnlockType.Halagaz2 => "HALAGAZ2",
            UnlockType.Halagaz3 => "HALAGAZ3",
            UnlockType.Halagaz4 => "HALAGAZ4",
            UnlockType.Halagaz5 => "HALAGAZ5",
            UnlockType.Halagaz6 => "HALAGAZ6",

            UnlockType.Naudhiz0 => "NAUDHIZ0",
            UnlockType.Naudhiz1 => "NAUDHIZ1",
            UnlockType.Naudhiz2 => "NAUDHIZ2",
            UnlockType.Naudhiz3 => "NAUDHIZ3",
            UnlockType.Naudhiz4 => "NAUDHIZ4",
            UnlockType.Naudhiz5 => "NAUDHIZ5",
            UnlockType.Naudhiz6 => "NAUDHIZ6",

            UnlockType.Jera0 => "JERA0",
            UnlockType.Jera1 => "JERA1",
            UnlockType.Jera2 => "JERA2",
            UnlockType.Jera3 => "JERA3",
            UnlockType.Jera4 => "JERA4",
            UnlockType.Jera5 => "JERA5",
            UnlockType.Jera6 => "JERA6",

            _ => throw new ArgumentOutOfRangeException(nameof(unlock), unlock, null)
        };

        private static readonly HashSet<UnlockType> steamAchievements = new HashSet<UnlockType>()
        {
            UnlockType.Anubis,
            UnlockType.Cat,
            UnlockType.Papyrus,
            UnlockType.Thousand,
            UnlockType.J,
            UnlockType.S,
            UnlockType.Crocodile,
            UnlockType.EyeOfHorus,
            UnlockType.H,
            UnlockType.Heart,
            UnlockType.House,
            UnlockType.Hundred,
            UnlockType.King,
            UnlockType.M,
            UnlockType.Man,
            UnlockType.N,
            UnlockType.Osiris,
            UnlockType.R,
            UnlockType.Snake,
            UnlockType.Star,
            UnlockType.Sun,
            UnlockType.Water,
            UnlockType.Woman,

            UnlockType.Halagaz0,
            UnlockType.Naudhiz0,
            UnlockType.Jera0,
        };

        public static readonly Dictionary<(Runes.Runes rune, int difficulty), UnlockType> runeUnlocks = new()
        {
            { (Runes.Runes.Uruz,    0), UnlockType.Uruz0    },
            { (Runes.Runes.Uruz,    1), UnlockType.Uruz1    },
            { (Runes.Runes.Uruz,    2), UnlockType.Uruz2    },
            { (Runes.Runes.Uruz,    3), UnlockType.Uruz3    },
            { (Runes.Runes.Uruz,    4), UnlockType.Uruz4    },
            { (Runes.Runes.Uruz,    5), UnlockType.Uruz5    },
            { (Runes.Runes.Uruz,    6), UnlockType.Uruz6    },

            { (Runes.Runes.Halagaz, 0), UnlockType.Halagaz0 },
            { (Runes.Runes.Halagaz, 1), UnlockType.Halagaz1 },
            { (Runes.Runes.Halagaz, 2), UnlockType.Halagaz2 },
            { (Runes.Runes.Halagaz, 3), UnlockType.Halagaz3 },
            { (Runes.Runes.Halagaz, 4), UnlockType.Halagaz4 },
            { (Runes.Runes.Halagaz, 5), UnlockType.Halagaz5 },
            { (Runes.Runes.Halagaz, 6), UnlockType.Halagaz6 },

            { (Runes.Runes.Naudhiz, 0), UnlockType.Naudhiz0 },
            { (Runes.Runes.Naudhiz, 1), UnlockType.Naudhiz1 },
            { (Runes.Runes.Naudhiz, 2), UnlockType.Naudhiz2 },
            { (Runes.Runes.Naudhiz, 3), UnlockType.Naudhiz3 },
            { (Runes.Runes.Naudhiz, 4), UnlockType.Naudhiz4 },
            { (Runes.Runes.Naudhiz, 5), UnlockType.Naudhiz5 },
            { (Runes.Runes.Naudhiz, 6), UnlockType.Naudhiz6 },

            { (Runes.Runes.Jera,    0), UnlockType.Jera0    },
            { (Runes.Runes.Jera,    1), UnlockType.Jera1    },
            { (Runes.Runes.Jera,    2), UnlockType.Jera2    },
            { (Runes.Runes.Jera,    3), UnlockType.Jera3    },
            { (Runes.Runes.Jera,    4), UnlockType.Jera4    },
            { (Runes.Runes.Jera,    5), UnlockType.Jera5    },
            { (Runes.Runes.Jera,    6), UnlockType.Jera6    },
        };

        public static void UnlockUnlock(UnlockType id)
        {
            if (!unlocks[id])
            {
                unlocks[id] = true;
                SaveUnlocks();

                if (steamAchievements.Contains(id)) SteamManager.UnlockAchievement(ToKey(id));
            }

        }

        public static bool IsUnlockUnlocked(UnlockType id)
        {
            return unlocks.TryGetValue(id, out bool value) && value;
        }

        public static Dictionary<UnlockType, bool> GetAllUnlocks()
        {
            return new Dictionary<UnlockType, bool>(unlocks);
        }

        private static void SaveUnlocks()
        {
            var json = JsonSerializer.Serialize(unlocks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(unlocksPath, json);
        }

        public static Dictionary<UnlockType, bool> LoadUnlocks()
        {
            if (File.Exists(unlocksPath))
            {
                var json = File.ReadAllText(unlocksPath);
                unlocks = JsonSerializer.Deserialize<Dictionary<UnlockType, bool>>(json) ?? new();
                return unlocks;
            }
            return null;
        }

        private static void EnsureAllUnlocksExist()
        {
            foreach (UnlockType type in Enum.GetValues(typeof(UnlockType)))
            {
                if (!unlocks.ContainsKey(type))
                    unlocks[type] = false;
            }
            SaveUnlocks();
        }
    }
}
