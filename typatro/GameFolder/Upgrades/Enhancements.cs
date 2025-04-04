using System.Collections.Generic;

namespace typatro.GameFolder.Upgrades{

    class Enhancements{
        public long[] letters = new long[26];
        public Enhancements(){
            for(int letter = 0; letter < letters.Length; letter++){
                letters[letter] = 1;
            }
        }

        public void AddLetterScore(int letter, int score){
            letters[letter] += score;
        }

        public void MultiplyLetterScore(int letter, int score){
            letters[letter] *= score;
        }

    }

}