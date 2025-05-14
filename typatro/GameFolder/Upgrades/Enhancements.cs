using System;
using System.Collections.Generic;
using System.Linq;
using typatro.GameFolder.UI;

namespace typatro.GameFolder.Upgrades{

    public class Enhancements{
        public long[] letters = new long[26];
        public long[] lettersChange = new long[26];
        public int wordScore = 2;
        public int damageResist = 0;
        public int startingScore = 0;
        public Enhancements(){
            for(int letter = 0; letter < letters.Length; letter++){
                letters[letter] = 1;
            }
        }

        public void ResetLettersChange(){
            lettersChange = new long[26];
        }

        public long GetLetterScore(char letter){
            return letters[letter-'a'];
        }

        public void AddLetterScore(char letter, long score){
            letters[letter-'a'] += score;
            lettersChange[letter-'a'] += score;
            foreach(long lette in letters){
                if(lette < 0) SaveManager.UnlockAchievement("halagaz0");
            }
        }

        public void AllLettersAddScore(long score){
            for(int i = 0; i < letters.Length; i++){
                letters[i] += score;
                lettersChange[i] += score;
            }
        }

        public void MultiplyLetterScore(char letter, double score){
            lettersChange[letter-'a'] += (int)(letters[letter-'a'] * score);
            letters[letter-'a'] = (int)(letters[letter-'a'] * score);
            foreach(long lette in letters){
                if(lette < 0) SaveManager.UnlockAchievement("halagaz0");
            }
        }

        public void AllLettersMultiplyScore(double score){
            for(int i = 0; i < letters.Length; i++){
                lettersChange[i] += (int)(letters[i] * score);
                letters[i] = (int)(letters[i] * score);
            }
        }

        public void AddToWordScore(int score){
            wordScore += score;
        }

        public void AddGlyphEnhancementsUpdate(Glyph glyph){
            switch(glyph){
                case Glyph.A:
                    AddLetterScore('a',5);
                    AddLetterScore('e',5);
                    AddLetterScore('i',5);
                    AddLetterScore('o',5);
                    AddLetterScore('u',5);
                    break;
                case Glyph.D:
                    GameLogic.actions.Add(new UserAction("randomLetter",""));
                    GameLogic.actions.Add(new UserAction("randomLetter",""));
                    MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0,26)+'a'),5);
                    MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0,26)+'a'),5);
                    break;
                case Glyph.H:
                    for(int i = 0; i < 10; i++){
                        GameLogic.actions.Add(new UserAction("randomLetter",""));
                        MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0,26)+'a'),5);
                    }
                    break;
                case Glyph.J:
                    AllLettersAddScore(10);
                    break;
                case Glyph.Water:
                    for(int i = 0; i < 5; i++){
                        GameLogic.actions.Add(new UserAction("randomLetter",""));
                        AddLetterScore((char)(GameLogic.seededRandom.Next(0,26)+'a'),-5);
                    }
                    break;
                case Glyph.King:
                    long maxVal = letters.Max();
                    long minVal = letters.Min();
                    bool setZero = maxVal != minVal;
                    for(int i = 0; i < letters.Length; i++){
                        if(letters[i] == maxVal){
                            lettersChange[i] = letters[i] * 5;
                            letters[i] *= 5;
                        }
                        if(setZero && letters[i] == minVal){
                            lettersChange[i] -= letters[i];
                            letters[i] = 0;
                        }
                    }
                    break;
                case Glyph.Cat:
                    for(int i = 0; i < 9; i++){
                        GameLogic.actions.Add(new UserAction("randomLetter",""));
                        MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0,26)+'a'),2);
                    }
                    break;
                case Glyph.Crocodile:
                GameLogic.actions.Add(new UserAction("randomLetter",""));
                    MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0,26)+'a'),20);
                    GameLogic.actions.Add(new UserAction("randomLetter",""));
                    char letter = (char)GameLogic.seededRandom.Next(0,26);
                    lettersChange[letter] -= letters[letter];
                    letters[letter] = 0;
                    break;
                case Glyph.One:
                    AllLettersAddScore(1);
                    break;
                case Glyph.Ten:
                GameLogic.actions.Add(new UserAction("randomLetter",""));
                    MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0,26)+'a'),10);
                    break;
                case Glyph.Bread:
                    for(int i = 0; i < 6; i++){
                        GameLogic.actions.Add(new UserAction("randomLetter",""));
                        MultiplyLetterScore((char)(GameLogic.seededRandom.Next(0,26)+'a'),3);
                    }
                    break;
                case Glyph.Star:
                    AllLettersMultiplyScore(20);
                    break;
            }
            foreach(long letter in letters){
                if(letter < 0) SaveManager.UnlockAchievement("halagaz0");
            }
        }



    }

}