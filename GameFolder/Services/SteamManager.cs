using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static typatro.GameFolder.Services.UnlockManager;
namespace typatro.GameFolder.Services
{
    public static class SteamManager
    {
        private static bool _initialized = false;

        public enum SteamStats
        {
            GameMinutes,
            Deaths,
            RunsWon,
            Letters,
            Words,
            FightsWon
        }

        public static string StatsToString(SteamStats stat) => stat switch
        {
            SteamStats.GameMinutes => "game_minutes",
            SteamStats.Deaths => "deaths",
            SteamStats.RunsWon => "runs_won",
            SteamStats.Letters => "letters",
            SteamStats.Words => "words",
            SteamStats.FightsWon => "fights_won",

            _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
        };

        public static void UnlockAchievement(string achievementName)
        {
            if (!_initialized) return;

            SteamUserStats.SetAchievement(achievementName);
            SteamUserStats.StoreStats();
        }

        public static void IncrementStat(SteamStats stat, int amount = 1)
        {
            if (!_initialized) return;
            string statName = StatsToString(stat);
            if (SteamUserStats.GetStat(statName, out int current))
            {
                CheckForAchievments(stat, current + amount);
                SteamUserStats.SetStat(statName, current + amount);
                SteamUserStats.StoreStats();
            }
        }

        public static void StoreStat()
        {
            if (!_initialized) return;
            SteamUserStats.StoreStats();
        }

        public static void SetStat(string stat, int value)
        {
            if (!_initialized) return;
            SteamUserStats.SetStat(stat, value);
        }

        public static bool GetStat(string stat, out int value)
        {
            if (!_initialized) {  value =  0; return false; }
            return SteamUserStats.GetStat(stat, out value);
        }

        private static void CheckForAchievments(SteamStats stat, int value)
        {
            switch (stat)
            {
                case SteamStats.GameMinutes:
                    if(value >= 240) UnlockManager.UnlockUnlock(UnlockManager.UnlockType.House);
                    break;
                case SteamStats.Deaths:
                    if (value >= 1) UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Anubis);
                    if (value >= 9) UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Cat);
                    break;
                case SteamStats.RunsWon:
                    if (value >= 1) UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Man);
                    if (value >= 5) UnlockManager.UnlockUnlock(UnlockManager.UnlockType.King);
                    break;
                case SteamStats.Letters:
                    if (value >= 1000) UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Thousand);
                    break;
                case SteamStats.Words:
                    if (value >= 100) UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Hundred);
                    break;
                case SteamStats.FightsWon:
                    if (value >= 10) UnlockManager.UnlockUnlock(UnlockManager.UnlockType.N);
                    if (value >= 100) UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Water);
                    break;
            }
        }

        public static void Shutdown()
        {
            if (!_initialized) return;
            SteamAPI.Shutdown();
        }

        public static bool Init()
        {
            try{
                _initialized = SteamAPI.Init();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Nepodařilo se připojit na steam: {e.Message}");
            }
            return _initialized;
        }
    }
}
