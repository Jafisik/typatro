using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.Upgrades;

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
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Texture2D texture;
        private readonly int rows = 2;
        private readonly int cols = 3;
        private List<Card> cards;
        private int selectedRow = 0;
        private int selectedCol = 0;
        private readonly int cardWidth = 150;
        private readonly int cardHeight = 80;
        private readonly int horizontalSpacing = 160;
        private readonly int verticalSpacing = 100;
        private readonly int topOffset = 100, leftOffset = 50;

        private bool topMove = true, downMove = true, leftMove = true, rightMove = true, enterPressed = false;
        Enhancements enhancements;

        public Shop(SpriteBatch spriteBatch, SpriteFont font, Texture2D texture, Enhancements enhancements)
        {
            this.spriteBatch = spriteBatch;
            this.font = font;
            this.texture = texture;
            this.enhancements = enhancements;
            cards = new List<Card>(6);
            GenerateShop();
        }

        public Card GenerateCard()
        {
            char letter = (char)(new Random().Next(0, 26) + 'a');
            bool mult = new Random().Next(1, 101) >= 75;
            int value = mult ? new Random().Next(2, 5) : new Random().Next(2, 11);
            return new Card(letter, mult, value, mult ? value*5 : value);
        }

        public void GenerateShop(){
            cards.Clear();
            for(int i = 0; i < 6; i++){
                cards.Add(GenerateCard());
            }
        }

        public bool DisplayShop(ref int coins)
        {
            MoveSelection();
            Buying(ref coins);
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int cardIndex = row * cols + col;
                    if (cardIndex >= cards.Count) break;

                    Color cardColor = (row == selectedRow && col == selectedCol) ? Color.Red : Color.White;
                    spriteBatch.Draw(texture, new Rectangle(col * horizontalSpacing + leftOffset, row * verticalSpacing+ topOffset, cardWidth, cardHeight), cardColor);
                    spriteBatch.DrawString(font, $"   {cards[cardIndex].letter}\n {(cards[cardIndex].mult ? "*" : "+")} {cards[cardIndex].value}\nCost:{cards[cardIndex].cost}", new Vector2(col * horizontalSpacing + 10 + leftOffset, row * verticalSpacing + 10 + topOffset), Color.Black);
                
                }
            }

            return false;
        }

        private bool Buying(ref int coins){
            KeyboardState state = Keyboard.GetState();
            if(state.IsKeyDown(Keys.Enter) && !enterPressed){
                enterPressed = true;
                Card card = cards[selectedRow*cols+selectedCol];
                if(card.cost <= coins){
                    coins -= card.cost;
                    if (card.mult)
                        enhancements.MultiplyLetterScore(card.letter, card.value);
                    else
                        enhancements.AddLetterScore(card.letter, card.value);
                    cards[selectedRow*cols+selectedCol] = GenerateCard();
                    return true;
                }
            }
            else if(state.IsKeyUp(Keys.Enter)) enterPressed = false;
            return false;
        }

        public void MoveSelection()
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Up) && topMove){
                if (selectedRow > 0) selectedRow--;
                topMove = false;
            } else if(state.IsKeyUp(Keys.Up)) topMove = true;

            if (state.IsKeyDown(Keys.Down) && downMove){
                if (selectedRow < rows - 1) selectedRow++;
                downMove = false;
            } else if(state.IsKeyUp(Keys.Down)) downMove = true;

            if (state.IsKeyDown(Keys.Left) && leftMove){
                if (selectedCol > 0) selectedCol--;
                leftMove = false;
            } else if(state.IsKeyUp(Keys.Left)) leftMove = true;

            if (state.IsKeyDown(Keys.Right) && rightMove){
                if (selectedCol < cols - 1) selectedCol++;
                rightMove = false;
            } else if(state.IsKeyUp(Keys.Right)) rightMove = true;
        }
    

    }
}