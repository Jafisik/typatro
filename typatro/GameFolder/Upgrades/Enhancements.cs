using System.Collections.Generic;

namespace typatro.GameFolder.Upgrades{

    class Enhancements{
        public long[] letters = new long[26];
        public Enhancements(){
            for(int letter = 0; letter < letters.Length; letter++){
                letters[letter] = 1;
            }
        }

        public long GetLetterScore(char letter){
            return letters[letter-'a'];
        }

        public void AddLetterScore(char letter, int score){
            letters[letter-'a'] += score;
        }

        public void AllLettersAddScore(int score){
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



    }

}