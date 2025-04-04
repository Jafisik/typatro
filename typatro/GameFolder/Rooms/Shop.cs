using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace typatro.GameFolder.Rooms{
    class Card{
        public char letter;
        public bool mult;
        public int value, cost;
        public Card(char letter, bool mult, int value, int cost){
            this.letter = letter;
            this.mult = mult;
            this.value = value;
            this.cost = cost;
        }

    }
    class Shop{
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D texture;
        readonly int cardCount = 3;
        List<Card> cards;
        static readonly HashSet<char> consonants = new HashSet<char> { 'a', 'e', 'i', 'o', 'u'};
        Random random = new Random();
        int leftSpacing = 150;
        int selectedCard = 0;

        public Shop(SpriteBatch spriteBatch, SpriteFont font, Texture2D texture){
            this.spriteBatch = spriteBatch;
            this.font = font;
            this.texture = texture;
        }

        public List<Card> GenerateCards(){
            cards = new List<Card>(cardCount);
            for(int card = 0; card < cardCount; card++){
                char letter = (char)(random.Next(0, 26) + 'a');

                bool mult = random.Next(1, 101) >= 75;
                int value = mult ? random.Next(2, 5) : random.Next(2, 11);

                int cost;
                if(consonants.Contains(letter)) cost = (5 + value) * (mult ? 2 : 1);
                else cost = (2 + value) * (mult ? 2 : 1);
                cards.Add(new Card(letter, mult, value, cost));
            }
            return cards;
        }

        public void DisplayShop(){
            for(int i = 0; i < cardCount; i++){
                spriteBatch.Draw(texture, new Rectangle(leftSpacing * i, 0, leftSpacing-30, 80), Color.White);
                spriteBatch.DrawString(font, $"   {cards[i].letter}\n {(cards[i].mult?"*":"+")} {cards[i].value}\nCost:{cards[i].cost}", new Vector2(leftSpacing * i, 10), Color.Black);
            }
        }

    }
}