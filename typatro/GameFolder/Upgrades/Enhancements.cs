using System;
using System.Collections.Generic;
using System.Linq;

namespace typatro.GameFolder.Upgrades{

    class Enhancements{
        public long[] letters = new long[26];
        public int wordScore = 2;
        public int damageResist = 0;
        public int startingScore = 0;
        Random random = new Random();
        public Enhancements(){
            for(int letter = 0; letter < letters.Length; letter++){
                letters[letter] = 1;
            }
        }

        public long GetLetterScore(char letter){
            return letters[letter-'a'];
        }

        public void AddLetterScore(char letter, long score){
            letters[letter-'a'] += score;
        }

        public void AllLettersAddScore(long score){
            for(int i = 0; i < letters.Length; i++){
                letters[i] += score;
            }
        }

        public void MultiplyLetterScore(char letter, double score){
            letters[letter-'a'] = (int)(letters[letter-'a'] * score);
        }

        public void AllLettersMultiplyScore(double score){
            for(int i = 0; i < letters.Length; i++){
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
                    MultiplyLetterScore('d',4);
                    break;
                case Glyph.H:
                    for(int i = 0; i < 10; i++){
                        MultiplyLetterScore((char)(random.Next(0,26)+'a'),5);
                    }
                    break;
                case Glyph.K:
                    AllLettersAddScore(10);
                    break;
                case Glyph.Water:
                    for(int i = 0; i < 5; i++){
                        AddLetterScore((char)(random.Next(0,26)+'a'),-5);
                    }
                    break;
                case Glyph.King:
                    long maxVal = letters.Max();
                    long minVal = letters.Min();
                    bool setZero = maxVal != minVal;
                    for(int i = 0; i < letters.Length; i++){
                        if(letters[i] == maxVal) letters[i] *= 5;
                        if(setZero && letters[i] == minVal) letters[i] = 0;
                    }
                    break;
                case Glyph.Cat:
                    for(int i = 0; i < 9; i++){
                        MultiplyLetterScore((char)(random.Next(0,26)+'a'),2);
                    }
                    break;
                case Glyph.Crocodile:
                    MultiplyLetterScore((char)(random.Next(0,26)+'a'),20);
                    letters[random.Next(0,26)] = 0;
                    break;
                case Glyph.One:
                    AllLettersAddScore(1);
                    break;
                case Glyph.Ten:
                    MultiplyLetterScore((char)(random.Next(0,26)+'a'),10);
                    break;
                case Glyph.Bread:
                    for(int i = 0; i < 6; i++){
                        MultiplyLetterScore((char)(random.Next(0,26)+'a'),3);
                    }
                    break;
                case Glyph.Star:
                    AllLettersMultiplyScore(20);
                    break;
            }
        }



    }

}