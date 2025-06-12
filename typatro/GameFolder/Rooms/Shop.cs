using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Steamworks;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder.Rooms{
    class LetterUpgrade{
        public char letter;
        public bool mult;
        public long value, cost;
        public LetterUpgrade(char letter, bool mult, long value, long cost){
            this.letter = letter;
            this.mult = mult;
            this.value = value;
            this.cost = cost;
        }

    }

    enum EnhancementsType
    {
        wordScore,
        damageResist,
        startingScore,
        shinyChance,
        stoneChance,
        bloomChance,
    }

    class EnhancementsUpgrade
    {
        public EnhancementsType enhancementsType;
        public int cost;
        public EnhancementsUpgrade(EnhancementsType enhancementsType, int cost)
        {
            this.enhancementsType = enhancementsType;
            this.cost = cost;
        }
    }
    class Shop
    {
        MainGame.Gfx gfx;
        readonly int rows = 3, cols = 4;
        List<LetterUpgrade> cards;
        List<Glyph> glyphs;
        List<EnhancementsUpgrade> enhancementsUpgrades;
        int selectedRow = 0, selectedCol = 0;
        readonly int horizontalSpacing = 160, verticalSpacing = 100, cardHeight = 80, cardWidth = 150, cardTextTopOffset = 10, cardTextLeftOffset = 5;
        readonly int topOffset = 100, leftOffset = 30, cardCount = 5, glyphCount = 2, enhancementsCount = 3;
        Vector2 descPos;
        int rerollCost = 5, wordCost = 10, damageRedCost = 10, startingScoreCost = 1, glyphCost = 50;

        bool topMove = true, downMove = true, leftMove = true, rightMove = true, enterPressed = false;
        Enhancements enhancements;

        public Shop(MainGame.Gfx gfx, Enhancements enhancements)
        {
            this.gfx = gfx;
            this.enhancements = enhancements;
            cards = new List<LetterUpgrade>(cardCount);
            glyphs = new List<Glyph>(glyphCount);
            enhancementsUpgrades = new List<EnhancementsUpgrade>(enhancementsCount);
            if (SaveManager.size == 0)
            {
                rows = 3;
                cols = 4;
                leftOffset = 80;
                descPos = new Vector2(80, 400);
            }
            else
            {
                rows = 2;
                cols = 6;
                leftOffset = 30;
                descPos = new Vector2(50, 300);
            }
        }

        public LetterUpgrade GenerateCard()
        {
            if (!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("GenerateCard", ""));
            char letter = (char)(GameLogic.seededRandom.Next(0, 26) + 'a');
            bool mult = GameLogic.seededRandom.Next(1, 101) >= 75;
            long value = mult ? GameLogic.seededRandom.Next(2, 5) : GameLogic.seededRandom.Next(2, 11);
            return new LetterUpgrade(letter, mult, value, mult ? (value / 2 + enhancements.GetLetterScore(letter)) * 3 : (value / 2 + enhancements.GetLetterScore(letter)));
        }

        public EnhancementsUpgrade GenerateEnhancement()
        {
            EnhancementsType type = (EnhancementsType)GameLogic.unseededRandom.Next(Enum.GetValues(typeof(EnhancementsType)).Length);
            int cost = 10;
            switch (type)
            {
                case EnhancementsType.wordScore:
                    cost = enhancements.wordScore + 5;
                    break;
                case EnhancementsType.damageResist:
                    cost = enhancements.damageResist * 5 + 5;
                    break;
                case EnhancementsType.startingScore:
                    cost = enhancements.startingScore/5 + 5;
                    break;
                case EnhancementsType.shinyChance:
                    cost = (int)(enhancements.shinyChance*100*10+10);
                    break;
                case EnhancementsType.stoneChance:
                    cost = (int)(enhancements.stoneChance*100*2+7);
                    break;
                case EnhancementsType.bloomChance:
                    cost = (int)(enhancements.bloomChance*100*8+10);
                    break;
            }
            return new EnhancementsUpgrade(type, cost);
        }

        private void GenerateShop()
        {
            cards.Clear();
            glyphs.Clear();
            enhancementsUpgrades.Clear();
            for (int i = 0; i < cardCount; i++)
            {
                cards.Add(GenerateCard());
            }

            for (int i = 0; i < enhancementsCount; i++)
            {
                enhancementsUpgrades.Add(GenerateEnhancement());
            }

            for (int i = 0; i < glyphCount; i++)
            {
                Glyph glyph = GlyphManager.GetRandomUnusedGlyph();
                if (i == 1)
                {
                    while (glyph == glyphs[0])
                    {
                        glyph = GlyphManager.GetRandomUnusedGlyph();
                    }
                }
                glyphs.Add(glyph);
            }
            glyphCost = 40 + 10 * GlyphManager.GetGlyphCount();
        }

        public void NewShop()
        {
            GenerateShop();
            rerollCost = 5;
            selectedCol = 0;
            selectedRow = 0;
        }

        public bool DisplayShop(ref long coins)
        {
            MoveSelection();
            if (Buying(ref coins)) return true;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int cardIndex = row * cols + col;
                    int selectedCardIndex = selectedRow * cols + selectedCol;
                    Color cardColor = (row == selectedRow && col == selectedCol) ? ThemeColors.Selected : ThemeColors.NotSelected;

                    gfx.spriteBatch.Draw(gfx.texture, new Rectangle(col * horizontalSpacing + leftOffset, row * verticalSpacing + topOffset, cardWidth, cardHeight), cardColor);

                    if (cardIndex < cardCount)
                    {
                        cards[cardIndex].cost = cards[cardIndex].mult ? (cards[cardIndex].value + enhancements.GetLetterScore(cards[cardIndex].letter)) * 3 : (cards[cardIndex].value + enhancements.GetLetterScore(cards[cardIndex].letter));
                        gfx.spriteBatch.DrawString(gfx.smallTextFont, $" {cards[cardIndex].letter}:  {enhancements.GetLetterScore(cards[cardIndex].letter)}       {(cards[cardIndex].mult ? "*" : "+")} {cards[cardIndex].value}\n\n Cost:        {cards[cardIndex].cost}",
                        new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex < cardCount)
                        {
                            LetterUpgrade selectedCard = cards[selectedCardIndex];
                            string description;
                            if (selectedCard.mult) description = $"Muliplies letter value of '{selectedCard.letter}' by *{selectedCard.value}\n\nCurrent letter score value:{enhancements.GetLetterScore(selectedCard.letter)}    Price of upgrade:{selectedCard.cost}";
                            else description = $"Adds +{selectedCard.value} to the letter value of '{selectedCard.letter}'\n\nCurrent letter score value:{enhancements.GetLetterScore(selectedCard.letter)}    Price of upgrade:{selectedCard.cost}";
                            //gfx.spriteBatch.DrawString(gfx.smallTextFont, description, descPos, ThemeColors.Text);
                            if (selectedCardIndex == cardIndex) gfx.spriteBatch.DrawString(gfx.smallTextFont, description, descPos, ThemeColors.Text);
                        }
                    }
                    else if (cardIndex == 5)
                    {
                        gfx.spriteBatch.DrawString(gfx.smallTextFont, $" reroll\n\n cost:  {rerollCost}", new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex == 5) gfx.spriteBatch.DrawString(gfx.smallTextFont, $"Rerolls all items in the shop for {rerollCost}", descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == 6)
                    {
                        gfx.spriteBatch.DrawString(gfx.smallTextFont, $" {enhancementsUpgrades[0].enhancementsType}{enhancementsUpgrades[0].cost}", new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex == 6) gfx.spriteBatch.DrawString(gfx.smallTextFont, EnhancementsTypeDesc(enhancementsUpgrades[0]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == 7)
                    {
                        gfx.spriteBatch.DrawString(gfx.smallTextFont, $" {enhancementsUpgrades[1].enhancementsType}{enhancementsUpgrades[1].cost}", new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex == 7) gfx.spriteBatch.DrawString(gfx.smallTextFont, EnhancementsTypeDesc(enhancementsUpgrades[1]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == 8)
                    {
                        gfx.spriteBatch.DrawString(gfx.smallTextFont, $" {enhancementsUpgrades[2].enhancementsType}{enhancementsUpgrades[2].cost}", new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex == 8) gfx.spriteBatch.DrawString(gfx.smallTextFont, EnhancementsTypeDesc(enhancementsUpgrades[2]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == 9)
                    {
                        gfx.spriteBatch.Draw(GlyphManager.GetGlyphImage(glyphs[0]), new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), cardColor);
                        gfx.spriteBatch.DrawString(gfx.smallTextFont, $"   Cost:\n\n     {glyphCost}", new Vector2(64 + col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex == 9) gfx.spriteBatch.DrawString(gfx.smallTextFont, GlyphManager.GetDescription(glyphs[0]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == 10)
                    {
                        gfx.spriteBatch.Draw(GlyphManager.GetGlyphImage(glyphs[1]), new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), cardColor);
                        gfx.spriteBatch.DrawString(gfx.smallTextFont, $"   Cost:\n\n     {glyphCost}", new Vector2(64 + col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex == 10) gfx.spriteBatch.DrawString(gfx.smallTextFont, GlyphManager.GetDescription(glyphs[1]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == 11) gfx.spriteBatch.DrawString(gfx.smallTextFont, "\n exit shop", new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                }
            }
            return false;
        }

        private bool Buying(ref long coins)
        {
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Enter) && !enterPressed)
            {
                enterPressed = true;
                int selectionIndex = selectedRow * cols + selectedCol;

                if (selectionIndex < cardCount)
                {
                    LetterUpgrade card = cards[selectionIndex];
                    if (card.cost <= coins)
                    {
                        coins -= card.cost;
                        if (card.mult)
                            enhancements.MultiplyLetterScore(card.letter, card.value);
                        else
                            enhancements.AddLetterScore(card.letter, card.value);
                        cards[selectionIndex] = GenerateCard();
                    }
                }
                if (selectionIndex == 5 && coins >= rerollCost)
                {
                    coins -= rerollCost;
                    rerollCost += 2;
                    GenerateShop();
                }
                if (selectionIndex == 6 && coins >= wordCost)
                {
                    coins -= wordCost;
                    wordCost += enhancements.wordScore * 2;
                    enhancements.AddToWordScore(1);
                }
                if (selectionIndex == 7 && coins >= damageRedCost)
                {
                    coins -= damageRedCost;
                    damageRedCost += enhancements.damageResist * 3;
                    enhancements.damageResist += 1;
                }
                if (selectionIndex == 8 && coins >= startingScoreCost)
                {
                    coins -= startingScoreCost;
                    startingScoreCost += 5;
                    enhancements.AddToStartingScore(10);
                }
                if (selectionIndex == 9 && coins >= glyphCost)
                {
                    coins -= glyphCost;
                    Glyph glyph = glyphs[0];
                    GlyphManager.Add(glyph);
                    if (glyph == Glyph.Hundred) coins += 100;
                    enhancements.AddGlyphEnhancementsUpdate(glyph);
                    glyphs[0] = GlyphManager.GetRandomUnusedGlyph();
                    glyphCost = 40 + 10 * GlyphManager.GetGlyphCount();
                }
                if (selectionIndex == 10 && coins >= glyphCost)
                {
                    coins -= glyphCost;
                    Glyph glyph = glyphs[1];
                    GlyphManager.Add(glyph);
                    if (glyph == Glyph.Hundred) coins += 100;
                    enhancements.AddGlyphEnhancementsUpdate(glyph);
                    glyphs[1] = GlyphManager.GetRandomUnusedGlyph();
                    glyphCost = 40 + 10 * GlyphManager.GetGlyphCount();
                }
                if (coins == 0 && !GameLogic.achievmentBools["CROCODILE"])
                {
                    GameLogic.achievmentBools["CROCODILE"] = true;
                    GameLogic.writeAchievment = true;
                }
                if (selectionIndex == 11) return true;

            }
            else if (state.IsKeyUp(Keys.Enter)) enterPressed = false;
            return false;
        }

        public void MoveSelection()
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Up) && topMove)
            {
                if (selectedRow > 0) selectedRow--;
                topMove = false;
            }
            else if (state.IsKeyUp(Keys.Up)) topMove = true;

            if (state.IsKeyDown(Keys.Down) && downMove)
            {
                if (selectedRow < rows - 1) selectedRow++;
                downMove = false;
            }
            else if (state.IsKeyUp(Keys.Down)) downMove = true;

            if (state.IsKeyDown(Keys.Left) && leftMove)
            {
                if (selectedCol > 0) selectedCol--;
                leftMove = false;
            }
            else if (state.IsKeyUp(Keys.Left)) leftMove = true;

            if (state.IsKeyDown(Keys.Right) && rightMove)
            {
                if (selectedCol < cols - 1) selectedCol++;
                rightMove = false;
            }
            else if (state.IsKeyUp(Keys.Right)) rightMove = true;
        }

        private string EnhancementsTypeDesc(EnhancementsUpgrade type)
        {
            return type.enhancementsType switch
            {
                EnhancementsType.wordScore => $"Adds +1 score per each correct word\n\nCurrent bonus: {enhancements.wordScore}    Cost:{wordCost}",
                EnhancementsType.damageResist => $"Reduces the incoming damage from enemies\n\nCurrent bonus: {enhancements.damageResist}    Cost:{damageRedCost}",
                EnhancementsType.startingScore => $"Adds +10 to the score at begining of the fight\n\nCurrent bonus: {enhancements.startingScore}    Cost:{startingScoreCost}",
                EnhancementsType.shinyChance => "shinyChance",
                EnhancementsType.stoneChance => "stoneChance",
                EnhancementsType.bloomChance => "bloomChance",
                _ => "",
            };
        }
    }
}