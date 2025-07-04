using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using typatro.GameFolder.UI;

namespace typatro.GameFolder.Upgrades{

    public class Enhancements{
        public long[] letters = new long[26];
        public long[] lettersChange = new long[26];
        public int wordScore = 2;
        public int damageResist = 0;
        public int startingScore = 0;
        public int wordChange, damageChange, startChange;

        public double shinyChance = 0.01, stoneChance = 0.05, bloomChance = 0.02;
        public double shChange, stChange, blChange;

        public double shinyScore = 1.2, shinyScoreChange;
        public int stoneScore = 50, stoneScoreChange;

        public bool overHundred = false;

        public Enhancements()
        {
            for (int letter = 0; letter < letters.Length; letter++)
            {
                letters[letter] = 1;
            }
        }

        public void ResetChange()
        {
            lettersChange = new long[26];
            wordChange = damageChange = startChange = 0;
            shChange = stChange = blChange = 0;
            shinyScoreChange = 0;
            stoneScoreChange = 0;
        }

        public long GetLetterScore(char letter){
            return letters[letter-'a'];
        }

        public void AddLetterScore(char letter, long score)
        {
            letters[letter - 'a'] += score;
            lettersChange[letter - 'a'] += score;

            if (letters[letter - 'a'] < 0 && !GameLogic.achievmentBools["HALAGAZ0"])
            {
                GameLogic.achievmentBools["HALAGAZ0"] = true;
                GameLogic.writeAchievment = true;
            }
            if (letters[letter - 'a'] >= 100 && !GameLogic.achievmentBools["J"])
            {
                GameLogic.achievmentBools["J"] = true;
                GameLogic.writeAchievment = true;
            }

            if (letters[letter - 'a'] >= 100) overHundred = true;
        }

        public void AllLettersAddScore(long score){
            for (int i = 0; i < letters.Length; i++)
            {
                letters[i] += score;
                lettersChange[i] += score;
                if (letters[i] >= 100 && !GameLogic.achievmentBools["J"])
                {
                    SaveManager.UnlockUnlock("J");
                    SteamUserStats.SetAchievement("J");
                } 

                if (letters[i] >= 100) overHundred = true;
            }
        }

        public void MultiplyLetterScore(char letter, double score)
        {
            long tempLet = letters[letter - 'a'];
            letters[letter - 'a'] = (int)(letters[letter - 'a'] * score);
            lettersChange[letter - 'a'] += letters[letter - 'a'] - tempLet;
            foreach (long lette in letters)
            {
                if (lette < 0) SaveManager.UnlockUnlock("halagaz0");
            }
            if (letters[letter - 'a'] >= 100) overHundred = true;
        }

        public void AllLettersMultiplyScore(double score){
            for (int i = 0; i < letters.Length; i++)
            {
                long tempLet = letters[i];
                letters[i] = (int)(letters[i] * score);
                lettersChange[i] += letters[i] - tempLet;
                if (letters[i] >= 100) overHundred = true;
            }
        }

        public void AddToWordScore(int score)
        {
            wordScore += score;
            wordChange += score;
        }

        public void AddToStartingScore(int score)
        {
            startingScore += score;
            startChange += score;
            if (startingScore >= 100 && !GameLogic.achievmentBools["H"])
            {
                GameLogic.achievmentBools["H"] = true;
                GameLogic.writeAchievment = true;
            }
        }

        public void AddToDamageResist(int score)
        {
            damageResist += score;
            damageChange += score;
        }

        public void AddShinyChance(double chance)
        {
            shinyChance += chance;
            shChange += chance;
        }

        public void AddStoneChance(double chance)
        {
            stoneChance += chance;
            stChange += chance;
        }

        public void AddBloomChance(double chance)
        {
            bloomChance += chance;
            blChange += chance;
        }

        public void AddShinyScore(double score)
        {
            shinyScore += score;
            shinyScoreChange += score;
        }

        public void AddStoneScore(int score)
        {
            stoneScore += score;
            stoneScoreChange += score;
            if (stoneScore >= 100 && !GameLogic.achievmentBools["H"])
            {
                GameLogic.achievmentBools["H"] = true;
                GameLogic.writeAchievment = true;
            }
        }

        public (char bestLetter, long bestLetterNum) HighestLetter()
        {
            char letter = 'a';
            long maxScore = 0;
            int i = 0;
            foreach (long score in letters)
            {
                if (score > maxScore)
                {
                    maxScore = score;
                    letter = (char)('a' + i);
                }
                i++;
            }
            return (letter, maxScore);
        }

        public void AddGlyphEnhancementsUpdate(Glyph glyph)
        {
            switch (glyph)
            {
                case Glyph.A:
                    AddLetterScore('a', 5);
                    AddLetterScore('e', 5);
                    AddLetterScore('i', 5);
                    AddLetterScore('o', 5);
                    AddLetterScore('u', 5);
                    break;
                case Glyph.D:
                    if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("randomLetter", ""));
                    if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("randomLetter", ""));
                    MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0, 26) + 'a'), 5);
                    MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0, 26) + 'a'), 5);
                    break;
                case Glyph.H:
                    for (int i = 0; i < 10; i++)
                    {
                        if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("randomLetter", ""));
                        MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0, 26) + 'a'), 5);
                    }
                    break;
                case Glyph.J:
                    AllLettersAddScore(10);
                    break;
                case Glyph.Water:
                    for (int i = 0; i < 5; i++)
                    {
                        if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("randomLetter", ""));
                        AddLetterScore((char)(GameLogic.seededRandom.Next(0, 26) + 'a'), -5);
                    }
                    break;
                case Glyph.King:
                    long maxVal = letters.Max();
                    long minVal = letters.Min();
                    bool setZero = maxVal != minVal;
                    for (int i = 0; i < letters.Length; i++)
                    {
                        if (letters[i] == maxVal)
                        {
                            lettersChange[i] = letters[i] * 5;
                            letters[i] *= 5;
                        }
                        if (setZero && letters[i] == minVal)
                        {
                            lettersChange[i] -= letters[i];
                            letters[i] = 0;
                        }
                    }
                    break;
                case Glyph.Cat:
                    for (int i = 0; i < 9; i++)
                    {
                        if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("randomLetter", ""));
                        MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0, 26) + 'a'), 2);
                    }
                    break;
                case Glyph.Crocodile:
                    if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("randomLetter", ""));
                    MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0, 26) + 'a'), 20);
                    if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("randomLetter", ""));
                    char letter = (char)GameLogic.seededRandom.Next(0, 26);
                    lettersChange[letter] -= letters[letter];
                    letters[letter] = 0;
                    break;
                case Glyph.One:
                    AllLettersAddScore(1);
                    break;
                case Glyph.Ten:
                    if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("randomLetter", ""));
                    MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0, 26) + 'a'), 10);
                    break;
                case Glyph.Bread:
                    for (int i = 0; i < 6; i++)
                    {
                        if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("randomLetter", ""));
                        AddLetterScore((char)(GameLogic.seededRandom.Next(0, 26) + 'a'), 3);
                    }
                    break;
                case Glyph.Star:
                    AllLettersMultiplyScore(20);
                    break;
            }
            foreach (long letter in letters)
            {
                if (letter < 0) SaveManager.UnlockUnlock("halagaz0");
            }
        }



    }

}