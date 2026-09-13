using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.Services;
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
        streakMult,
        damageResist,
        mistakeBlock,
        shinyChance,
        stoneChance,
        bloomChance,
        shinyScore,
        stoneScore,
        bloomScore
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
        readonly int rows = 2, cols = 6;
        List<LetterUpgrade> cards;
        List<Glyph> glyphs;
        List<EnhancementsUpgrade> enhancementsUpgrades;
        int selectedRow = 0, selectedCol = 0;
        readonly int horizontalSpacing = 185, verticalSpacing = 130, cardHeight = 95, cardWidth = 165;
        readonly int topOffset, leftOffset, cardCount = 5, glyphCount = 2, enhancementsCount = 3;
        readonly int rerollIndex = 5, glyph1Index = 7;
        readonly int glyph2Index = 6, enh1Index = 8, enh3Index = 10, exitIndex = 11;
        Vector2 descPos;
        int rerollCost = 5, glyphCost = 50;

        bool topMove = true, downMove = true, leftMove = true, rightMove = true, enterPressed = false;
        Enhancements enhancements;

        // "-cost" popups that float up and fade out after a purchase, and a quick shake on
        // the card the player just tried (and failed) to afford.
        class FloatingText { public string Text; public Vector2 Pos; public float Timer; }
        readonly List<FloatingText> floatingTexts = new();
        float shakeTimer = 0f;
        Vector2 coinsPos;

        public Shop(Enhancements enhancements)
        {
            this.enhancements = enhancements;
            cards = new List<LetterUpgrade>(cardCount);
            glyphs = new List<Glyph>(glyphCount);
            enhancementsUpgrades = new List<EnhancementsUpgrade>(enhancementsCount);
            // Center the whole panel horizontally on screen instead of pinning it near the
            // left edge with a lot of dead space on the right.
            int panelWidth = (cols - 1) * horizontalSpacing + cardWidth + 30 + 40;
            leftOffset = (MainGame.screenWidth - panelWidth) / 2 + 20;
            topOffset = MainGame.screenHeight / 3;
            descPos = new Vector2(leftOffset, topOffset - 20 + (rows - 1) * verticalSpacing + cardHeight + 55);
        }

        public LetterUpgrade GenerateCard()
        {
            char letter = (char)(GameLogic.contextRandom.Next(0, 26) + 'a');
            bool mult = GameLogic.contextRandom.Next(1, 101) >= 75;
            long value = mult ? GameLogic.contextRandom.Next(2, 5) : GameLogic.contextRandom.Next(2, 11);
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
                EnhancementsType.streakMult => (int)(enhancements.streakMult * 100 + 5),
                EnhancementsType.damageResist => enhancements.damageResist * 3 + 5,
                EnhancementsType.mistakeBlock => enhancements.mistakeBlock * 10 + 10,
                EnhancementsType.shinyChance => (int)(enhancements.shinyChance * 100 * 10 + 10),
                EnhancementsType.stoneChance => (int)(enhancements.stoneChance * 100 * 2 + 7),
                EnhancementsType.bloomChance => (int)(enhancements.bloomChance * 100 * 8 + 10),
                EnhancementsType.shinyScore => (int)(enhancements.shinyScore * 30),
                EnhancementsType.stoneScore => enhancements.stoneScore / 10 + 10,
                // Expensive and gets steeper each purchase - a flat, permanent letter boost
                // on every bloom word is strong, so it shouldn't be cheap to stack.
                EnhancementsType.bloomScore => enhancements.bloomScore * 80 + 100,
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
            UpdateEffects();

            // Freeze selection/hover/clicks while a tutorial bubble is up so it can't reach
            // through it and buy/exit early - the shop still renders live underneath though.
            bool inputLocked = TutorialManager.IsShowing();
            if (!inputLocked) MoveSelection();
            bool mouseOnShopCard = false;

            // Backing panel behind the whole grid so the cards read as one group against the
            // busy background instead of floating loose on top of it - lighter than the raw
            // background so it still reads as a distinct "panel", not just more darkness.
            Rectangle panelRect = new Rectangle(leftOffset - 20, topOffset - 20,
                (cols - 1) * horizontalSpacing + cardWidth + 30 + 40, (rows - 1) * verticalSpacing + cardHeight + 40);
            Color panelColor = Color.Lerp(ThemeColors.Background, ThemeColors.Text, 0.15f);
            panelColor.A = 235;
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, panelRect, panelColor);

            string coinsText = $"coins:{coins}";
            Vector2 coinsSize = MainGame.Gfx.menuFont.MeasureString(coinsText);
            coinsPos = Round(new Vector2(panelRect.X + panelRect.Width / 2f - coinsSize.X / 2f, panelRect.Y - coinsSize.Y - 15));
            int coinsShakeOffset = shakeTimer > 0 ? (int)(Math.Sin(shakeTimer * 60) * 6) : 0;
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, coinsText, coinsPos + new Vector2(coinsShakeOffset, 0), ThemeColors.Text);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int cardIndex = row * cols + col;
                    int selectedCardIndex = selectedRow * cols + selectedCol;

                    int selectedOffset = 50;
                    Color selected = ThemeColors.Selected;
                    Color notSelected = ThemeColors.NotSelected;

                    if (cardIndex == rerollIndex)
                    {
                        notSelected = ThemeColors.ShopReroll;
                    }
                    else if (cardIndex == exitIndex)
                    {
                        notSelected = ThemeColors.ExitShop;
                    }
                    else if (cardIndex >= enh1Index && cardIndex <= enh3Index)
                    {
                        notSelected = Color.Lerp(ThemeColors.NotSelected, ThemeColors.ShopReroll, 0.5f);
                    }
                    selected = new Color(notSelected.R + selectedOffset, notSelected.G + selectedOffset, notSelected.B + selectedOffset);
                    Color cardColor = (row == selectedRow && col == selectedCol) ? selected : notSelected;

                    Rectangle cardRect = GetCardRect(cardIndex);

                    if (!inputLocked && cardRect.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
                    {
                        selectedCol = col;
                        selectedRow = row;
                        mouseOnShopCard = true;
                    }
                    MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, cardRect, cardColor);
                    if (row == selectedRow && col == selectedCol)
                        ThemeColors.DrawGlowCorners(cardRect, ThemeColors.Text);

                    SpriteFont sfCard = MainGame.Gfx.smallTextFont;

                    if (cardIndex < cardCount)
                    {
                        cards[cardIndex].cost = cards[cardIndex].mult ? (cards[cardIndex].value + enhancements.GetLetterScore(cards[cardIndex].letter)) * 3 : (cards[cardIndex].value + enhancements.GetLetterScore(cards[cardIndex].letter));

                        string letterWithScore = $"{cards[cardIndex].letter}: {enhancements.GetLetterScore(cards[cardIndex].letter)}";
                        string upgradeStr = $"{(cards[cardIndex].mult ? "*" : "+")} {cards[cardIndex].value}";

                        // Current value (left half) vs. what the upgrade adds (right half) -
                        // two even halves flush against each other and the card's own edges,
                        // so it reads as one bar split in two, not floating little badges.
                        DrawSplitTopBar(sfCard, letterWithScore, upgradeStr, cardRect);

                        DrawCostBar(sfCard, cards[cardIndex].cost, cardRect, notSelected);

                        if (selectedCardIndex < cardCount)
                        {
                            LetterUpgrade selectedCard = cards[selectedCardIndex];
                            string description;
                            if (selectedCard.mult) description = $"Muliplies letter value of '{selectedCard.letter}' by *{selectedCard.value}\n\nCurrent letter score value:{enhancements.GetLetterScore(selectedCard.letter)}    Price of upgrade:{selectedCard.cost}";
                            else description = $"Adds +{selectedCard.value} to the letter value of '{selectedCard.letter}'\n\nCurrent letter score value:{enhancements.GetLetterScore(selectedCard.letter)}    Price of upgrade:{selectedCard.cost}";
                            if (selectedCardIndex == cardIndex) MainGame.Gfx.spriteBatch.DrawString(sfCard, description, descPos, ThemeColors.Text);
                        }
                    }
                    else if (cardIndex == rerollIndex)
                    {
                        DrawCenteredTitle(sfCard, "Reroll", cardRect);
                        DrawCostBar(sfCard, rerollCost, cardRect, notSelected);
                        if (selectedCardIndex == rerollIndex) MainGame.Gfx.spriteBatch.DrawString(sfCard, $"Rerolls all items in the shop for {rerollCost} coins", descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == glyph1Index)
                    {
                        DrawCenteredGlyph(glyphs[0], cardRect, cardColor);
                        if (glyphs[0] != Glyph.NoGlyphsLeft)
                        {
                            DrawCostBar(sfCard, glyphCost, cardRect, notSelected);
                        }
                        if (selectedCardIndex == glyph1Index) MainGame.Gfx.spriteBatch.DrawString(sfCard, GlyphManager.GetDescription(glyphs[0]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == glyph2Index)
                    {
                        DrawCenteredGlyph(glyphs[1], cardRect, cardColor);
                        if (glyphs[1] != Glyph.NoGlyphsLeft)
                        {
                            DrawCostBar(sfCard, glyphCost, cardRect, notSelected);
                        }
                        if (selectedCardIndex == glyph2Index)
                            MainGame.Gfx.spriteBatch.DrawString(sfCard, GlyphManager.GetDescription(glyphs[1]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex >= enh1Index && cardIndex <= enh3Index)
                    {
                        int enhIdx = cardIndex - enh1Index;
                        DrawCenteredTitle(sfCard, EnhancementsTypeTitle(enhancementsUpgrades[enhIdx]), cardRect);
                        DrawCostBar(sfCard, enhancementsUpgrades[enhIdx].cost, cardRect, notSelected);
                        if (selectedCardIndex == cardIndex) MainGame.Gfx.spriteBatch.DrawString(sfCard, EnhancementsTypeDesc(enhancementsUpgrades[enhIdx]), descPos, ThemeColors.Text);
                    }
                    else if (cardIndex == exitIndex) DrawCenteredTitle(sfCard, "Exit shop", cardRect);
                }
            }

            foreach (FloatingText floatingText in floatingTexts)
            {
                Color color = ThemeColors.Wrong;
                color.A = (byte)(MathHelper.Clamp(floatingText.Timer, 0f, 1f) * 255);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, floatingText.Text, Round(floatingText.Pos), color);
            }

            return Buying(ref coins, ref mousePressed, ref mouseState, mouseOnShopCard);
        }

        private void UpdateEffects()
        {
            for (int i = floatingTexts.Count - 1; i >= 0; i--)
            {
                floatingTexts[i].Timer -= 1f / 60f;
                floatingTexts[i].Pos.Y -= 0.6f;
                floatingTexts[i].Pos.X += 0.8f;
                if (floatingTexts[i].Timer <= 0) floatingTexts.RemoveAt(i);
            }
            if (shakeTimer > 0) shakeTimer -= 1f / 60f;
        }

        // Floats up from the coins display, same font/size as the coins text itself.
        private void SpawnFloatingCost(long cost)
        {
            floatingTexts.Add(new FloatingText { Text = $"-{cost}", Pos = Round(coinsPos + new Vector2(MainGame.Gfx.menuFont.MeasureString($"coins:").X, 0)), Timer = 1f });
        }

        private void TriggerShake()
        {
            shakeTimer = 0.3f;
        }

        private void DrawCenteredGlyph(Glyph glyph, Rectangle cardRect, Color color)
        {
            Texture2D image = GlyphManager.GetGlyphImage(glyph);
            int contentHeight = cardRect.Height - 24;
            Vector2 pos = Round(new Vector2(
                cardRect.X + cardRect.Width / 2f - image.Width / 2f,
                cardRect.Y + contentHeight / 2f - image.Height / 2f));
            MainGame.Gfx.spriteBatch.Draw(image, pos, color);
        }

        // Title text centered in the space above the cost bar - used by reroll/exit/enhancement
        // cards, which (unlike letter cards) only have one thing to say up top.
        private void DrawCenteredTitle(SpriteFont sf, string text, Rectangle cardRect)
        {
            Vector2 size = sf.MeasureString(text);
            int contentHeight = cardRect.Height - 24;
            Vector2 pos = Round(new Vector2(cardRect.X + cardRect.Width / 2f - size.X / 2f, cardRect.Y + contentHeight / 2f - size.Y / 2f));
            MainGame.Gfx.spriteBatch.DrawString(sf, text, pos, ThemeColors.Text);
        }

        // Two even halves flush against each other and the card's edges - current value on
        // the left, what the upgrade adds on the right, with the right half accented so the
        // two meanings stay visually distinct even though they're touching.
        private void DrawSplitTopBar(SpriteFont sf, string leftText, string rightText, Rectangle cardRect)
        {
            int barHeight = 36;
            Rectangle leftRect = new Rectangle(cardRect.X, cardRect.Y, cardRect.Width / 2, barHeight);
            Rectangle rightRect = new Rectangle(cardRect.X + cardRect.Width / 2, cardRect.Y, cardRect.Width - cardRect.Width / 2, barHeight);

            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, leftRect, ThemeColors.NotSelected);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, rightRect, ThemeColors.Selected);

            Vector2 leftSize = sf.MeasureString(leftText);
            Vector2 rightSize = sf.MeasureString(rightText);
            MainGame.Gfx.spriteBatch.DrawString(sf, leftText,
                Round(new Vector2(leftRect.X + leftRect.Width / 2f - leftSize.X / 2f, leftRect.Y + barHeight / 2f - leftSize.Y / 2f)), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(sf, rightText,
                Round(new Vector2(rightRect.X + rightRect.Width / 2f - rightSize.X / 2f, rightRect.Y + barHeight / 2f - rightSize.Y / 2f)), ThemeColors.Text);
        }

        // Bitmap fonts render crisp only at whole-pixel positions - centering math (width/2,
        // textSize/2, ...) constantly lands on X.5 fractions, which is what was causing the
        // occasional shifted/blurry-looking letter.
        private static Vector2 Round(Vector2 v) => new Vector2((float)Math.Round(v.X), (float)Math.Round(v.Y));

        private Rectangle GetCardRect(int cardIndex)
        {
            int col = cardIndex % cols, row = cardIndex / cols;
            int rerollExitOffset = (cardIndex == rerollIndex || cardIndex == exitIndex) ? 30 : 0;
            return new Rectangle(col * horizontalSpacing + leftOffset + rerollExitOffset, row * verticalSpacing + topOffset, cardWidth, cardHeight);
        }

        private bool Buying(ref long coins, ref bool mousePressed, ref MouseState mouseState, bool mouseOnShopCard)
        {
            KeyboardState state = Keyboard.GetState();
            if (UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.ShopTutorial) && state.IsKeyDown(Keys.Enter) && !enterPressed ||
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
                        SpawnFloatingCost(card.cost);
                    }
                    else TriggerShake();
                }
                if (selectionIndex == rerollIndex)
                {
                    if (coins >= rerollCost)
                    {
                        coins -= rerollCost;
                        SpawnFloatingCost(rerollCost);
                        rerollCost += 2;
                        GenerateShop();
                    }
                    else TriggerShake();
                }
                if (selectionIndex == glyph1Index)
                {
                    if (coins >= glyphCost) { coins -= glyphCost; SpawnFloatingCost(glyphCost); BuyGlyph(0, ref coins); }
                    else TriggerShake();
                }
                if (selectionIndex == glyph2Index)
                {
                    if (coins >= glyphCost) { coins -= glyphCost; SpawnFloatingCost(glyphCost); BuyGlyph(1, ref coins); }
                    else TriggerShake();
                }

                for (int i = 0; i < enhancementsCount; i++)
                {
                    if (selectionIndex == enh1Index + i)
                    {
                        if (coins >= enhancementsUpgrades[i].cost)
                        {
                            coins -= enhancementsUpgrades[i].cost;
                            SpawnFloatingCost(enhancementsUpgrades[i].cost);
                            EnhancementsTypeUpgrade(enhancementsUpgrades[i]);
                            enhancementsUpgrades[i] = GenerateEnhancement();
                        }
                        else TriggerShake();
                    }
                }
                if (coins == 0)
                {
                    UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Crocodile);
                }
                if (selectionIndex == exitIndex) return true;

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
                EnhancementsType.streakMult => "Streak mult",
                EnhancementsType.damageResist => "Damage resist",
                EnhancementsType.mistakeBlock => "Mistake block",
                EnhancementsType.shinyChance => "Shiny chance",
                EnhancementsType.stoneChance => "Stone chance",
                EnhancementsType.bloomChance => "Bloom chance",
                EnhancementsType.shinyScore => "Shiny mult",
                EnhancementsType.stoneScore => "Stone score",
                EnhancementsType.bloomScore => "Bloom score",
                _ => "",
            };
        }

        // A full-width price tag along the bottom of the card, instead of a small badge
        // stuffed into a corner - reads as one consistent "this is what it costs" bar
        // across every item type in the shop. Darkened from the card's own color rather
        // than the panel color, so it stays visibly distinct from the panel behind it.
        private void DrawCostBar(SpriteFont sf, long cost, Rectangle cardRect, Color cardBaseColor)
        {
            int barHeight = 24;
            Rectangle barRect = new Rectangle(cardRect.X, cardRect.Bottom - barHeight, cardRect.Width, barHeight);
            Color barColor = Color.Lerp(cardBaseColor, Color.Black, 0.5f);
            barColor.A = 255;
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, barRect, barColor);

            string text = $"Cost: {cost}";
            Vector2 size = sf.MeasureString(text);
            MainGame.Gfx.spriteBatch.DrawString(sf, text,
                Round(new Vector2(barRect.X + barRect.Width / 2f - size.X / 2f, barRect.Y + barHeight / 2f - size.Y / 2f)), ThemeColors.Text);
        }

        private string EnhancementsTypeDesc(EnhancementsUpgrade type)
        {
            return type.enhancementsType switch
            {
                EnhancementsType.streakMult => $"Adds +0.05 mult per each correct word\n\nCurrent bonus: {enhancements.streakMult}    Cost: {type.cost}",
                EnhancementsType.damageResist => $"Reduces the incoming damage from enemies\n\nCurrent bonus: {enhancements.damageResist}    Cost: {type.cost}",
                EnhancementsType.mistakeBlock => $"Lets you block +1 mistake and keep your streak\n\nCurrent bonus: {enhancements.mistakeBlock}    Cost: {type.cost}",
                EnhancementsType.shinyChance => $"Adds 1% to the chance of spawning a shiny word\n(adds a 1.2x multiplier to all scores in a fight)\n\nCurrent chance: {(enhancements.shinyChance * 100).ToString("0.##")}%    Cost: {type.cost}",
                EnhancementsType.stoneChance => $"Adds 3% to the chance of spawning a stone word (adds 50 to score)\n\nCurrent chance: {(enhancements.stoneChance * 100).ToString("0.##")}%    Cost: {type.cost}",
                EnhancementsType.bloomChance => $"Adds 2% to the chance of spawning a bloom word\n(upgrades all the letters in the word by {enhancements.bloomScore})\n\nCurrent chance: {(enhancements.bloomChance * 100).ToString("0.##")}%    Cost: {type.cost}",
                EnhancementsType.stoneScore => $"Adds +20 to each written stone word\n\nCurrent score: {enhancements.stoneScore}    Cost: {type.cost}",
                EnhancementsType.shinyScore => $"Adds +0.25 to the shiny multiplier\n\nCurrent multiplier: {enhancements.shinyScore.ToString("0.##")}    Cost: {type.cost}",
                EnhancementsType.bloomScore => $"Adds +1 to how much each letter in a bloom word goes up\n\nCurrent boost: {enhancements.bloomScore}    Cost: {type.cost}",
                _ => "",
            };
        }

        private void EnhancementsTypeUpgrade(EnhancementsUpgrade upgrade)
        {
            switch (upgrade.enhancementsType)
            {
                case EnhancementsType.streakMult:
                    enhancements.AddToWordScore(0.05);
                    break;
                case EnhancementsType.damageResist:
                    enhancements.AddToDamageResist(1);
                    break;
                case EnhancementsType.mistakeBlock:
                    enhancements.AddToMistakeBlock(1);
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
                    enhancements.AddShinyScore(0.25);
                    break;
                case EnhancementsType.stoneScore:
                    enhancements.AddStoneScore(20);
                    break;
                case EnhancementsType.bloomScore:
                    enhancements.AddBloomScore(1);
                    break;
            }
            EnhancementCostUpdate();
        }

        private void BuyGlyph(int slotIndex, ref long coins)
        {
            Glyph glyph = glyphs[slotIndex];
            GlyphManager.Add(glyph);
            if (glyph == Glyph.Hundred) coins += 100;
            enhancements.AddGlyphEnhancementsUpdate(glyph);
            for (int j = 0; j <= 20; j++)
            {
                if (glyph == glyphs[slotIndex] || GlyphManager.IsActive(glyph))
                    glyph = GlyphManager.GetRandomUnusedGlyph();
                else
                    break;
            }
            if (glyph == glyphs[slotIndex] || GlyphManager.IsActive(glyph))
                glyph = Glyph.NoGlyphsLeft;
            glyphs[slotIndex] = glyph;
            glyphCost = 40 + 10 * GlyphManager.GetGlyphCount();
        }
    }
}