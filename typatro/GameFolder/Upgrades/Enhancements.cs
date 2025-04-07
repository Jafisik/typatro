using System.Collections.Generic;

namespace typatro.GameFolder.Upgrades{

    class Enhancements{
        public long[] letters = new long[26];
        public Enhancements(){
            for(int letter = 0; letter < letters.Length; letter++){
                letters[letter] = 1;
            }
        }

        public void AddLetterScore(char letter, int score){
            letters[letter-'a'] += score;
        }

        public void MultiplyLetterScore(char letter, int score){
            letters[letter-'a'] *= score;
        }

    }

}