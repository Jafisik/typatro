using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder.Rooms{
    class Card{
        public char letter;
        public bool mult;
        public long value, cost;
        public Card(char letter, bool mult, long value, long cost){
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
        readonly int rows = 3, cols = 4;
        List<Card> cards;
        List<Glyph> glyphs;
        int selectedRow = 0, selectedCol = 0;
        readonly int horizontalSpacing = 160, verticalSpacing = 100, cardHeight = 80, cardWidth = 150, cardTextTopOffset = 10, cardTextLeftOffset = 5;
        readonly int topOffset = 100, leftOffset = 30, cardCount = 5, glyphCount = 2;
        Vector2 descPos = new Vector2(50,400);
        int rerollCost = 5, wordCost = 10, damageRedCost = 10, startingScoreCost = 1, glyphCost = 50;

        bool topMove = true, downMove = true, leftMove = true, rightMove = true, enterPressed = false;
        Enhancements enhancements;

        public Shop(SpriteBatch spriteBatch, SpriteFont font, Texture2D texture, Enhancements enhancements)
        {
            this.spriteBatch = spriteBatch;
            this.font = font;
            this.texture = texture;
            this.enhancements = enhancements;
            cards = new List<Card>(cardCount);
            glyphs = new List<Glyph>(glyphCount);
            if(SaveManager.size == 0){
                rows = 3;
                cols = 4;
                leftOffset = 80;
            }
            else{
                rows = 2;
                cols = 5;
                leftOffset = 30;
            }
        }

        public Card GenerateCard()
        {
            if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("GenerateCard",""));
            char letter = (char)(GameLogic.seededRandom.Next(0, 26) + 'a');
            bool mult = GameLogic.seededRandom.Next(1, 101) >= 75;
            long value = mult ? GameLogic.seededRandom.Next(2, 5) : GameLogic.seededRandom.Next(2, 11);
            return new Card(letter, mult, value, mult ? (value + enhancements.GetLetterScore(letter))*3 : (value + enhancements.GetLetterScore(letter)));
        }

        private void GenerateShop(){
            cards.Clear();
            glyphs.Clear();
            for(int i = 0; i < cardCount; i++){
                cards.Add(GenerateCard());
            }
            for(int i = 0; i < glyphCount; i++){
                Glyph glyph = GlyphManager.GetRandomUnusedGlyph();
                if(i == 1){
                    while(glyph == glyphs[0]){
                        glyph = GlyphManager.GetRandomUnusedGlyph();
                    }
                }
                glyphs.Add(glyph);
            }
            glyphCost = 40 + 10 * GlyphManager.GetGlyphCount();
        }

        public void NewShop(){
            GenerateShop();
            rerollCost = 5;
            selectedCol = 0;
            selectedRow = 0;
        }

        public bool DisplayShop(ref long coins)
        {
            MoveSelection();
            if(Buying(ref coins)) return true;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int cardIndex = row * cols + col;
                    int selectedCardIndex =  selectedRow * cols + selectedCol;
                    Color cardColor = (row == selectedRow && col == selectedCol) ? ThemeColors.Selected : ThemeColors.NotSelected;

                    spriteBatch.Draw(texture, new Rectangle(col * horizontalSpacing + leftOffset, row * verticalSpacing+ topOffset, cardWidth, cardHeight), cardColor);
                    
                    if(cardIndex < cardCount){
                        cards[cardIndex].cost = cards[cardIndex].mult ? (cards[cardIndex].value + enhancements.GetLetterScore(cards[cardIndex].letter))*3 : (cards[cardIndex].value + enhancements.GetLetterScore(cards[cardIndex].letter));
                        spriteBatch.DrawString(font, $" {cards[cardIndex].letter}:  {enhancements.GetLetterScore(cards[cardIndex].letter)}       {(cards[cardIndex].mult ? "*" : "+")} {cards[cardIndex].value}\n\n Cost:        {cards[cardIndex].cost}", 
                        new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if(selectedCardIndex < cardCount){
                            Card selectedCard = cards[selectedCardIndex];
                            string description;
                            if(selectedCard.mult) description = $"Muliplies letter value of '{selectedCard.letter}' by *{selectedCard.value}\n\nCurrent letter score value:{enhancements.GetLetterScore(selectedCard.letter)}    Price of upgrade:{selectedCard.cost}";
                            else description = $"Adds +{selectedCard.value} to the letter value of '{selectedCard.letter}'\n\nCurrent letter score value:{enhancements.GetLetterScore(selectedCard.letter)}    Price of upgrade:{selectedCard.cost}";
                            //spriteBatch.DrawString(font, description, descPos, ThemeColors.Text);
                            if(selectedCardIndex == cardIndex) spriteBatch.DrawString(font, description, descPos, ThemeColors.Text);
                        }
                    }
                    else if(cardIndex == 5){
                        spriteBatch.DrawString(font, $" reroll\n\n cost:  {rerollCost}", new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if(selectedCardIndex == 5)spriteBatch.DrawString(font, $"Rerolls all items in the shop for {rerollCost}", descPos, ThemeColors.Text);
                    }
                    else if(cardIndex == 6){
                        spriteBatch.DrawString(font, $" Word\n score:  +1\n Cost: {wordCost}", new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if(selectedCardIndex == 6)spriteBatch.DrawString(font, $"Adds +1 score per each correct word\n\nCurrent bonus: {enhancements.wordScore}    Cost:{wordCost}", descPos, ThemeColors.Text);
                    }
                    else if(cardIndex == 7){
                        spriteBatch.DrawString(font, $" Incoming\n damage:  -1\n Cost: {damageRedCost}", new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if(selectedCardIndex == 7)spriteBatch.DrawString(font, $"Reduces the incoming damage from enemies\n\nCurrent bonus: {enhancements.damageResist}    Cost:{damageRedCost}", descPos, ThemeColors.Text);
                    }
                    else if(cardIndex == 8){
                        spriteBatch.DrawString(font, $" Startng\n score:  +10\n Cost: {startingScoreCost}", new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if(selectedCardIndex == 8)spriteBatch.DrawString(font, $"Adds +10 to the score at begining of the fight\n\nCurrent bonus: {enhancements.startingScore}    Cost:{startingScoreCost}", descPos, ThemeColors.Text);
                    }
                    else if(cardIndex == 9){
                        spriteBatch.Draw(GlyphManager.GetGlyphImage(glyphs[0]), new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), cardColor);
                        spriteBatch.DrawString(font, $"   Cost:\n\n     {glyphCost}", new Vector2(64+col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if(selectedCardIndex == 9) spriteBatch.DrawString(font, GlyphManager.GetDescription(glyphs[0]), descPos, ThemeColors.Text);
                    } 
                    else if(cardIndex == 10){
                        spriteBatch.Draw(GlyphManager.GetGlyphImage(glyphs[1]), new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), cardColor);
                        spriteBatch.DrawString(font, $"   Cost:\n\n     {glyphCost}", new Vector2(64+col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if(selectedCardIndex == 10) spriteBatch.DrawString(font, GlyphManager.GetDescription(glyphs[1]), descPos, ThemeColors.Text);
                    }
                    else if(cardIndex == 11) spriteBatch.DrawString(font, "\n exit shop", new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                }
            }
            return false;
        }

        private bool Buying(ref long coins){
            KeyboardState state = Keyboard.GetState();
            if(state.IsKeyDown(Keys.Enter) && !enterPressed){
                enterPressed = true;
                int selectionIndex = selectedRow*cols+selectedCol;

                if(selectionIndex < cardCount){
                    Card card = cards[selectionIndex];
                    if(card.cost <= coins){
                        coins -= card.cost;
                        if (card.mult)
                            enhancements.MultiplyLetterScore(card.letter, card.value);
                        else
                            enhancements.AddLetterScore(card.letter, card.value);
                        cards[selectionIndex] = GenerateCard();
                    }
                }
                if(selectionIndex == 5 && coins >= rerollCost){
                    coins -= rerollCost;
                    rerollCost += 2;
                    GenerateShop();
                }
                if(selectionIndex == 6 && coins >= wordCost){
                    coins -= wordCost;
                    wordCost += enhancements.wordScore*2;
                    enhancements.AddToWordScore(1);
                }
                if(selectionIndex == 7 && coins >= damageRedCost){
                    coins -= damageRedCost;
                    damageRedCost += enhancements.damageResist*3;
                    enhancements.damageResist += 1;
                }
                if(selectionIndex == 8 && coins >= startingScoreCost){
                    coins -= startingScoreCost;
                    startingScoreCost += 5;
                    enhancements.startingScore += 10;
                }
                if(selectionIndex == 9 && coins>= glyphCost){
                    coins -= glyphCost;
                    Glyph glyph = glyphs[0];
                    GlyphManager.Add(glyph);
                    if(glyph == Glyph.Hundred) coins += 100;
                    enhancements.AddGlyphEnhancementsUpdate(glyph);
                    glyphs[0] = GlyphManager.GetRandomUnusedGlyph();
                    glyphCost = 40 + 10 * GlyphManager.GetGlyphCount();
                }
                if(selectionIndex == 10 && coins>= glyphCost){
                    coins -= glyphCost;
                    Glyph glyph = glyphs[1];
                    GlyphManager.Add(glyph);
                    if(glyph == Glyph.Hundred) coins += 100;
                    enhancements.AddGlyphEnhancementsUpdate(glyph);
                    glyphs[1] = GlyphManager.GetRandomUnusedGlyph();
                    glyphCost = 40 + 10 * GlyphManager.GetGlyphCount();
                }
                if(selectionIndex == 11) return true;
                
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