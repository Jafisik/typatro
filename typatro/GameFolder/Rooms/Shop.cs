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
        shinyScore,
        stoneScore
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
        readonly int rows = 3, cols = 4;
        List<LetterUpgrade> cards;
        List<Glyph> glyphs;
        List<EnhancementsUpgrade> enhancementsUpgrades;
        int selectedRow = 0, selectedCol = 0;
        readonly int horizontalSpacing = 160, verticalSpacing = 100, cardHeight = 80, cardWidth = 150, cardTextTopOffset = 10, cardTextLeftOffset = 5;
        readonly int topOffset = 100, leftOffset = 30, cardCount = 5, glyphCount = 2, enhancementsCount = 3;
        Vector2 descPos;
        int rerollCost = 5, glyphCost = 50;

        bool topMove = true, downMove = true, leftMove = true, rightMove = true, enterPressed = false;
        Enhancements enhancements;

        public Shop(Enhancements enhancements)
        {
            this.enhancements = enhancements;
            cards = new List<LetterUpgrade>(cardCount);
            glyphs = new List<Glyph>(glyphCount);
            enhancementsUpgrades = new List<EnhancementsUpgrade>(enhancementsCount);
            if (SaveManager.size == 0)
            {
                rows = 3;
                cols = 4;
                leftOffset = 80;
                topOffset = 100;
                descPos = new Vector2(80, 400);
            }
            else if(SaveManager.size == 1)
            {
                rows = 2;
                cols = 6;
                leftOffset = 80;
                topOffset = MainGame.screenHeight / 3;
                descPos = new Vector2(leftOffset, topOffset*2);
            }
            else if (SaveManager.size == 2)
            {
                rows = 2;
                cols = 6;
                leftOffset = MainGame.screenWidth / 10;
                topOffset = MainGame.screenHeight / 3;
                descPos = new Vector2(leftOffset, topOffset * 2 - 30);
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
            return new EnhancementsUpgrade(type, EnhancementCost(type));
        }

        private int EnhancementCost(EnhancementsType type)
        {
            return type switch
            {
                EnhancementsType.wordScore => enhancements.wordScore * 2 + 5,
                EnhancementsType.damageResist => enhancements.damageResist * 3 + 5,
                EnhancementsType.startingScore => enhancements.startingScore / 5 + 5,
                EnhancementsType.shinyChance => (int)(enhancements.shinyChance * 100 * 10 + 10),
                EnhancementsType.stoneChance => (int)(enhancements.stoneChance * 100 * 2 + 7),
                EnhancementsType.bloomChance => (int)(enhancements.bloomChance * 100 * 8 + 10),
                EnhancementsType.shinyScore => (int)(enhancements.shinyScore * enhancements.shinyScore * 10),
                EnhancementsType.stoneScore => enhancements.stoneScore / 10 + 10,
                _ => 100,
            };
        }

        private void EnhancementCostUpdate()
        {
            foreach (EnhancementsUpgrade upgrade in enhancementsUpgrades)
            {
                upgrade.cost = EnhancementCost(upgrade.enhancementsType);
            }
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
                    for (int j = 0; j <= 10; j++)
                    {
                        if (glyph == glyphs[0])
                        {
                            glyph = GlyphManager.GetRandomUnusedGlyph();
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (glyph == glyphs[0])
                    {
                        glyph = Glyph.NoGlyphsLeft;
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

        public bool DisplayShop(ref long coins, ref bool mousePressed)
        {
            MouseState mouseState = Mouse.GetState();
            
            if(mouseState.LeftButton == ButtonState.Released)
            {
                mousePressed = false;
            }
            MoveSelection();
            bool mouseOnShopCard = false;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int cardIndex = row * cols + col;
                    int selectedCardIndex = selectedRow * cols + selectedCol;

                    int selectedOffset = 50;
                    Color selected = ThemeColors.Selected;
                    Color notSelected = ThemeColors.NotSelected;
                    if(cardIndex == (SaveManager.size == 0 ? 7 : 5))
                    {
                        notSelected = ThemeColors.ShopReroll;
                        selected = new Color(notSelected.R+selectedOffset, notSelected.G+selectedOffset, notSelected.B+selectedOffset);
                    } else if(cardIndex == 11)
                    {
                        notSelected = ThemeColors.ExitShop;
                        selected = new Color(notSelected.R+selectedOffset, notSelected.G+selectedOffset, notSelected.B+selectedOffset);
                    }
                    Color cardColor = (row == selectedRow && col == selectedCol) ? selected : notSelected;

                    int rerollExitOffset = 0;
                    if (SaveManager.size != 0 && (cardIndex == 5 || cardIndex == 11)) rerollExitOffset = 30;
                    Rectangle cardRect = new Rectangle(col * horizontalSpacing + leftOffset + rerollExitOffset, row * verticalSpacing + topOffset, cardWidth, cardHeight);

                    if (cardRect.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
                    {
                        selectedCol = col;
                        selectedRow = row;
                        mouseOnShopCard = true;
                    }
                    MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, cardRect, cardColor);

                    if (cardIndex < cardCount)
                    {
                        cards[cardIndex].cost = cards[cardIndex].mult ? (cards[cardIndex].value + enhancements.GetLetterScore(cards[cardIndex].letter)) * 3 : (cards[cardIndex].value + enhancements.GetLetterScore(cards[cardIndex].letter));
                        MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $" {cards[cardIndex].letter}:  {enhancements.GetLetterScore(cards[cardIndex].letter)}       {(cards[cardIndex].mult ? "*" : "+")} {cards[cardIndex].value}\n\n Cost:        {cards[cardIndex].cost}",
                        new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex < cardCount)
                        {
                            LetterUpgrade selectedCard = cards[selectedCardIndex];
                            string description;
                            if (selectedCard.mult) description = $"Muliplies letter value of '{selectedCard.letter}' by *{selectedCard.value}\n\nCurrent letter score value:{enhancements.GetLetterScore(selectedCard.letter)}    Price of upgrade:{selectedCard.cost}";
                            else description = $"Adds +{selectedCard.value} to the letter value of '{selectedCard.letter}'\n\nCurrent letter score value:{enhancements.GetLetterScore(selectedCard.letter)}    Price of upgrade:{selectedCard.cost}";
                            //MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, description, descPos, ThemeColors.Text);
                            if (selectedCardIndex == cardIndex) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, description, descPos, ThemeColors.Text);
                        }
                    }
                    else if (cardIndex == (SaveManager.size == 0 ? 7 : 5))
                    {
                        MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $" Reroll\n\n Cost:        {rerollCost}", new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset + rerollExitOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex == (SaveManager.size == 0 ? 7 : 5)) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"Rerolls all items in the shop for {rerollCost} coins", descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == 6)
                    {
                        MainGame.Gfx.spriteBatch.Draw(GlyphManager.GetGlyphImage(glyphs[0]), new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), cardColor);
                        if(glyphs[0] != Glyph.NoGlyphsLeft) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"   Cost:\n\n     {glyphCost}", new Vector2(64 + col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex == 6) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, GlyphManager.GetDescription(glyphs[0]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == (SaveManager.size == 0 ? 5 : 7))
                    {
                        MainGame.Gfx.spriteBatch.Draw(GlyphManager.GetGlyphImage(glyphs[1]), new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), cardColor);
                        if(glyphs[1] != Glyph.NoGlyphsLeft) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"   Cost:\n\n     {glyphCost}", new Vector2(64 + col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex == (SaveManager.size == 0 ? 5 : 7)) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, GlyphManager.GetDescription(glyphs[1]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == 8)
                    {
                        MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, EnhancementsTypeTitle(enhancementsUpgrades[0]), new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex == 8) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, EnhancementsTypeDesc(enhancementsUpgrades[0]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == 9)
                    {
                        MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, EnhancementsTypeTitle(enhancementsUpgrades[1]), new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex == 9) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, EnhancementsTypeDesc(enhancementsUpgrades[1]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == 10)
                    {
                        MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, EnhancementsTypeTitle(enhancementsUpgrades[2]), new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                        if (selectedCardIndex == 10) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, EnhancementsTypeDesc(enhancementsUpgrades[2]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == 11) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, "\n Exit shop", new Vector2(col * horizontalSpacing + cardTextLeftOffset + leftOffset + rerollExitOffset + 18, row * verticalSpacing + cardTextTopOffset + topOffset), ThemeColors.Text);
                }
            }
            if (Buying(ref coins, ref mousePressed, ref mouseState, mouseOnShopCard)) return true;
            return false;
        }

        private bool Buying(ref long coins, ref bool mousePressed, ref MouseState mouseState, bool mouseOnShopCard)
        {
            KeyboardState state = Keyboard.GetState();
            if (SaveManager.IsUnlockUnlocked("shopTutorial") && state.IsKeyDown(Keys.Enter) && !enterPressed ||
                (mouseState.LeftButton == ButtonState.Pressed && !mousePressed && mouseOnShopCard && !GameLogic.keyboardUsed))
            {
                mousePressed = true;
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
                if (selectionIndex == 6 && coins >= glyphCost)
                {
                    coins -= glyphCost;
                    Glyph glyph = glyphs[0];
                    GlyphManager.Add(glyph);
                    if (glyph == Glyph.Hundred) coins += 100;
                    enhancements.AddGlyphEnhancementsUpdate(glyph);
                    for (int j = 0; j <= 20; j++)
                    {
                        if (glyph == glyphs[1] || GlyphManager.IsActive(glyph))
                        {
                            glyph = GlyphManager.GetRandomUnusedGlyph();
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (glyph == glyphs[1] || GlyphManager.IsActive(glyph))
                    {
                        glyph = Glyph.NoGlyphsLeft;
                    }
                    glyphs[0] = glyph;
                    glyphCost = 40 + 10 * GlyphManager.GetGlyphCount();
                }
                if (selectionIndex == 7 && coins >= glyphCost)
                {
                    coins -= glyphCost;
                    Glyph glyph = glyphs[1];
                    GlyphManager.Add(glyph);
                    if (glyph == Glyph.Hundred) coins += 100;
                    enhancements.AddGlyphEnhancementsUpdate(glyph);
                    for (int j = 0; j <= 20; j++)
                    {
                        if (glyph == glyphs[0] || GlyphManager.IsActive(glyph))
                        {
                            glyph = GlyphManager.GetRandomUnusedGlyph();
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (glyph == glyphs[0] || GlyphManager.IsActive(glyph))
                    {
                        glyph = Glyph.NoGlyphsLeft;
                    }
                    glyphs[1] = glyph;
                    glyphCost = 40 + 10 * GlyphManager.GetGlyphCount();
                }
                if (selectionIndex == 8 && coins >= enhancementsUpgrades[0].cost)
                {
                    coins -= enhancementsUpgrades[0].cost;
                    EnhancementsTypeUpgrade(enhancementsUpgrades[0]);
                    enhancementsUpgrades[0] = GenerateEnhancement();
                }
                if (selectionIndex == 9 && coins >= enhancementsUpgrades[1].cost)
                {
                    coins -= enhancementsUpgrades[1].cost;
                    EnhancementsTypeUpgrade(enhancementsUpgrades[1]);
                    enhancementsUpgrades[1] = GenerateEnhancement();
                }
                if (selectionIndex == 10 && coins >= enhancementsUpgrades[2].cost)
                {
                    coins -= enhancementsUpgrades[2].cost;
                    EnhancementsTypeUpgrade(enhancementsUpgrades[2]);
                    enhancementsUpgrades[2] = GenerateEnhancement();
                }
                if (coins == 0 && !GameLogic.achievmentBools["CROCODILE"])
                {
                    GameLogic.achievmentBools["CROCODILE"] = true;
                    GameLogic.writeAchievment = true;
                }
                if (selectionIndex == 11) return true;

            }
            else if (state.IsKeyUp(Keys.Enter)) enterPressed = false;
            else enterPressed = true;
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

        private string EnhancementsTypeTitle(EnhancementsUpgrade type)
        {
            return type.enhancementsType switch
            {
                EnhancementsType.wordScore => $"Word score\n\nCost:        {type.cost}",
                EnhancementsType.damageResist => $"Damage resist\n\nCost:        {type.cost}",
                EnhancementsType.startingScore => $"Starting score\n\nCost:        {type.cost}",
                EnhancementsType.shinyChance => $"Shiny chance\n\nCost:        {type.cost}",
                EnhancementsType.stoneChance => $"Stone chance\n\nCost:        {type.cost}",
                EnhancementsType.bloomChance => $"Bloom chance\n\nCost:        {type.cost}",
                EnhancementsType.shinyScore => $"Shiny score\n\nCost:        {type.cost}",
                EnhancementsType.stoneScore => $"Stone score\n\nCost:        {type.cost}",
                _ => "",
            };
        }

        private string EnhancementsTypeDesc(EnhancementsUpgrade type)
        {
            return type.enhancementsType switch
            {
                EnhancementsType.wordScore => $"Adds +1 score per each correct word\n\nCurrent bonus: {enhancements.wordScore}    Cost: {type.cost}",
                EnhancementsType.damageResist => $"Reduces the incoming damage from enemies\n\nCurrent bonus: {enhancements.damageResist}    Cost: {type.cost}",
                EnhancementsType.startingScore => $"Adds +10 to the score at begining of the fight\n\nCurrent bonus: {enhancements.startingScore}    Cost: {type.cost}",
                EnhancementsType.shinyChance => $"Adds 1% to the chance of spawning a shiny word\n(adds a 1.2x multiplier to all scores in a fight)\n\nCurrent chance: {(enhancements.shinyChance * 100).ToString("0.##")}%    Cost: {type.cost}",
                EnhancementsType.stoneChance => $"Adds 3% to the chance of spawning a stone word (adds 50 to score)\n\nCurrent chance: {(enhancements.stoneChance * 100).ToString("0.##")}%    Cost: {type.cost}",
                EnhancementsType.bloomChance => $"Adds 2% to the chance of spawning a bloom word\n(upgrades all the letters in the word by 1)\n\nCurrent chance: {(enhancements.bloomChance * 100).ToString("0.##")}%    Cost: {type.cost}",
                EnhancementsType.stoneScore => $"Adds +20 to each written stone word\n\nCurrent score: {enhancements.stoneScore}    Cost: {type.cost}",
                EnhancementsType.shinyScore => $"Adds +0.1 to the shiny multiplier\n\nCurrent multiplier: {enhancements.shinyScore.ToString("0.#")}    Cost: {type.cost}",
                _ => "",
            };
        }

        private void EnhancementsTypeUpgrade(EnhancementsUpgrade upgrade)
        {
            switch (upgrade.enhancementsType)
            {
                case EnhancementsType.wordScore:
                    enhancements.AddToWordScore(1);
                    break;
                case EnhancementsType.damageResist:
                    enhancements.AddToDamageResist(1);
                    break;
                case EnhancementsType.startingScore:
                    enhancements.AddToStartingScore(10);
                    break;
                case EnhancementsType.shinyChance:
                    enhancements.AddShinyChance(0.01);
                    break;
                case EnhancementsType.stoneChance:
                    enhancements.AddStoneChance(0.03);
                    break;
                case EnhancementsType.bloomChance:
                    enhancements.AddBloomChance(0.02);
                    break;
                case EnhancementsType.shinyScore:
                    enhancements.AddShinyScore(0.1);
                    break;
                case EnhancementsType.stoneScore:
                    enhancements.AddStoneScore(20);
                    break;
            }
            EnhancementCostUpdate();
        }
    }
}