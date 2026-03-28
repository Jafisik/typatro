using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.Services;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;
using static typatro.GameFolder.Services.UnlockManager;

namespace typatro.GameFolder
{
    public partial class GameLogic
    {
        public void Play()
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState kBState = Keyboard.GetState();

            //Return to menu on pressing escape
            if (kBState.IsKeyDown(Keys.Escape) && !gameFinished)
            {
                gameState = GameState.MENU;
                if (!dead)
                {
                    SaveManager.SaveGame(seed, level, coins, lastSelectedNode, enhancements, difficulty, selectedRune, visitedNodes);
                    gameSaveData = SaveManager.LoadGame();
                }
                dead = false;
                return;
            }

            //Checking if the player died
            if (dead)
            {
                HandleDeath(kBState, mouseState);
                return;
            }

            //roomSelected is true if a room is selected and then it does the room logic,
            // if roomSelected is false it does the map logic
            if (roomSelected)
            {
                //Can start typing only after enter from selecting the room is released
                if (!canStartFight && prevKBState.IsKeyUp(Keys.Enter))
                {
                    timeInSeconds = 0;
                    canStartFight = true;
                }

                if (canStartFight)
                {
                    RoomHandler(kBState, mouseState);
                    if (isFightFinished)
                    {
                        FightFinished(kBState);
                    }
                }
            }
            else
            {
                MapHandler(kBState);
            }

            prevKBState = kBState;
        }

        private bool IsConfirmPressed(KeyboardState kBState, MouseState mouseState) =>
            kBState.IsKeyDown(Keys.Enter) || (mouseState.LeftButton == ButtonState.Pressed && windowActive);

        private void HandleDeath(KeyboardState kBState, MouseState mouseState)
        {
            if (!deadCounted)
            {
                SteamManager.IncrementStat(SteamManager.SteamStats.Deaths);
                SaveManager.RemoveGameData();
                GlyphManager.RemoveAllGlyphs();
                gameSaveData = null;
                deadCounted = true;
            }

            string fightWon = "You are dead";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, fightWon, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(fightWon).X / 2, 70), ThemeColors.Text);
            DrawRunStats(100, 450);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "Press enter to continue", new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString("Press enter to continue").X / 2, 450), ThemeColors.Text);

            if (IsConfirmPressed(kBState, mouseState))
            {
                dead = false;
                Reset();
                gameState = GameState.MENU;
            }
        }

        Keys[] prevKeys = new Keys[0];
        float pitch = 0f;
        int prevMistakes = 0;
        private void RoomHandler(KeyboardState state, MouseState mouseState)
        {
            if (state.IsKeyDown(Keys.Tab) && (!IsFight(selectedNode.type) || timeInSeconds == 0))
            {
                gameUi.Inventory(state);
                gameUi.TopBannerDisplay(false);
                return;
            }

            switch (selectedNode.type)
            {
                case NodeType.FIGHT:
                case NodeType.ELITE:
                case NodeType.BOSS:
                    if (!afterFightScreen) HandleFightRoom(state, mouseState);
                    break;
                case NodeType.TREASURE:
                    gameUi.TopBannerDisplay(true);
                    if (!inventoryUp) isFightFinished = treasure.DisplayTreasure(ref coins, ref mousePressed);
                    break;
                case NodeType.SHOP:
                    if (!inventoryUp) isFightFinished = shop.DisplayShop(ref coins, ref mousePressed);
                    gameUi.TopBannerDisplay(true);
                    if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.ShopTutorial))
                        if (TutorialManager.Draw(state, mouseState))
                            UnlockManager.UnlockUnlock(UnlockManager.UnlockType.ShopTutorial);
                    break;
                case NodeType.CURSE:
                    gameUi.TopBannerDisplay(true);
                    if (!inventoryUp) isFightFinished = curseRoom.CurseRoomDisplay(ref coins, ref mousePressed);
                    break;
            }
        }

        private void HandleFightRoom(KeyboardState state, MouseState mouseState)
        {
            writer.WriteText(neededText, ThemeColors.Selected, shinyWords, stoneWords, bloomWords, isHintText: true, rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset);
            Vector2 lastCharPos = new Vector2();
            if (UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.FightTutorial))
                lastCharPos = writer.UserInputText(Writer.writtenText.ToArray(), enhancements.mistakeBlock, rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset);
            gameUi.TopBannerDisplay(false);
            scoreCalculator.CalculateScore(lastCharPos, ref fight, ref enhancements);
            gameUi.HealthBar(ref fight, scoreCalculator.currentScore);

            PlayTypeSound(state);

            if (currentEnemy != null)
            {
                int frameWidth = currentEnemy.Width / 4;
                int frameHeight = currentEnemy.Height;
                int frame = (int)(MainGame.time.TotalGameTime.TotalSeconds) % 4;
                int scale = 6;
                int border = 6;
                Rectangle sourceRect = new Rectangle(frame * frameWidth, 0, frameWidth, frameHeight);
                Rectangle destRect = new Rectangle(MainGame.screenWidth - frameWidth * scale - frameWidth, MainGame.screenHeight - frameHeight * scale - frameHeight, frameWidth * scale, frameHeight * scale);
                Rectangle borderRect = new Rectangle(destRect.X - border, destRect.Y - border, destRect.Width + border * 2, destRect.Height + border * 2);

                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(borderRect.X, borderRect.Y, borderRect.Width, border), ThemeColors.Foreground);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(borderRect.X, borderRect.Bottom - border, borderRect.Width, border), ThemeColors.Foreground);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(borderRect.X, borderRect.Y, border, borderRect.Height), ThemeColors.Foreground);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(borderRect.Right - border, borderRect.Y, border, borderRect.Height), ThemeColors.Foreground);

                MainGame.Gfx.spriteBatch.Draw(currentEnemy, destRect, sourceRect, Color.White);

                if (!startedTyping && currentEnemyDesc != null)
                {
                    string wrapped = WrapText(MainGame.Gfx.smallTextFont, currentEnemyDesc, borderRect.Width);
                    Vector2 descSize = MainGame.Gfx.smallTextFont.MeasureString(wrapped);
                    MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, wrapped,
                        new Vector2(borderRect.X + borderRect.Width / 2 - descSize.X / 2, borderRect.Y - descSize.Y - 5),
                        ThemeColors.Text);
                }
            }

            if (eyeOfHorusActive)
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(0, 0, MainGame.screenWidth, MainGame.screenHeight), Color.Black);
            if (!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.Cat))
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.catPic, new Rectangle((int)catPos.X, (int)catPos.Y, 120, 80), Color.White);

            if (Writer.writtenText.Count == neededText.Length || scoreCalculator.currentScore >= fight.scoreNeeded)
                isFightFinished = true;

            if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.FightTutorial))
                if (TutorialManager.Draw(state, mouseState))
                    UnlockManager.UnlockUnlock(UnlockManager.UnlockType.FightTutorial);
        }

        private void PlayTypeSound(KeyboardState state)
        {
            Keys[] currKeys = state.GetPressedKeys();
            if (Writer.diffIndexes.Count > prevMistakes)
            {
                pitch = 0f;
                prevMistakes = Writer.diffIndexes.Count;
            }
            foreach (var key in currKeys)
            {
                if (!prevKeys.Contains(key))
                {
                    sfx.typeSound.Play(0.1f, pitch, 0f);
                    pitch = Math.Min(pitch + 0.005f, 1.0f);
                    break;
                }
            }
            prevKeys = currKeys;
        }

        private void FightFinished(KeyboardState state)
        {
            MouseState mouseState = Mouse.GetState();
            lastSelectedNode = selectedNode;

            if (!IsFight(selectedNode.type))
            {
                roomSelected = false;
                canStartFight = false;
                SaveManager.SaveGame(seed, level, coins, lastSelectedNode, enhancements, difficulty, selectedRune, visitedNodes);
                return;
            }

            if (!afterFightScreen)
                ProcessFightResult();
            else
                DrawRewardScreen(state, mouseState);
        }

        private void ProcessFightResult()
        {
            double flowerMult = GlyphManager.IsActive(Glyph.Flower) ? (1 + 0.1 * GlyphManager.GetGlyphCount()) : 1;
            double waterMult  = GlyphManager.IsActive(Glyph.Water)  ? 2 : 1;
            double heartMult  = GlyphManager.IsActive(Glyph.Heart)  ? (Writer.diffIndexes.Count > 0 ? 3 : 0.5) : 1;
            scoreCalculator.currentScore *= (long)(flowerMult * waterMult * heartMult);

            bool playerSurvived;
            if (scoreCalculator.currentScore >= fight.scoreNeeded)
            {
                OnFightWon();
                playerSurvived = true;
            }
            else if (GlyphManager.IsActive(Glyph.Osiris))
            {
                enhancements.AllLettersMultiplyScore(0.8);
                playerSurvived = true;
            }
            else
            {
                dead = true;
                playerSurvived = false;
            }

            afterFightScreen = true;
            totalScore += scoreCalculator.currentScore;
            if (scoreCalculator.currentScore > maxScore) maxScore = scoreCalculator.currentScore;
            mistakesWritten += Writer.diffIndexes.Count;
            lettersWritten += Writer.writtenText.Count;
            wordsWritten += Writer.writtenText.Count(c => c == ' ') + 1;

            if (playerSurvived)
            {
                int valMin = 1, valMax = 4;
                bool mult = false;
                if (selectedNode.type == NodeType.ELITE) { valMin = 3; valMax = 6; }
                if (selectedNode.type == NodeType.BOSS)  { valMin = 2; mult = true; }

                List<char> usedChars = new List<char>();
                for (int i = 0; i < 3; i++)
                {
                    cards.Add(GenerateRewardCard(usedChars, mult, valMin, valMax));
                    usedChars.Add(cards[cards.Count - 1].letter);
                }
            }
        }

        private void OnFightWon()
        {
            double cashMultiply = (GlyphManager.IsActive(Glyph.Woman) ? 0.8 : 1) * (GlyphManager.IsActive(Glyph.Man) ? 1.5 : 1);
            int cashGained = (int)(fight.cashGain * cashMultiply);
            coins += cashGained;
            coinsGained += cashGained;
            if (coins > maxCoins) maxCoins = coins;
            if (coins >= 200) UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Jera0);
            if (coins >= 100) UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Hundred);
            if (GlyphManager.IsActive(Glyph.B)) enhancements.AddToMistakeBlock(5);
            if (GlyphManager.IsActive(Glyph.Woman))
            {
                    enhancements.MultiplyLetterScore((char)(contextRandom.Next(0, 26) + 'a'), 2);
            }
            if (Writer.writtenText.Count >= neededText.Length - 10)
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Heart);
        }

        private void DrawRewardScreen(KeyboardState state, MouseState mouseState)
        {
            if (afterFightMove && state.IsKeyDown(Keys.Left) && afterFightSelect > 0)
            {
                afterFightSelect--;
                afterFightMove = false;
            }
            if (afterFightMove && state.IsKeyDown(Keys.Right) && afterFightSelect < cards.Count - 1)
            {
                afterFightSelect++;
                afterFightMove = false;
            }
            if (state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right))
                afterFightMove = true;

            if (!state.IsKeyUp(Keys.Tab)) return;

            if (selectedNode.type == NodeType.BOSS && level == 3)
                DrawBossWinScreen(state, mouseState);
            else
                DrawCardRewardScreen(state, mouseState);
        }

        private void DrawBossWinScreen(KeyboardState state, MouseState mouseState)
        {
            if (!gameFinished)
            {
                GlyphManager.RemoveAllGlyphs();
                SteamManager.IncrementStat(SteamManager.SteamStats.RunsWon);
                gameFinished = true;
            }
            string fightWon = "You won the run";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, fightWon, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(fightWon).X / 2, 70), ThemeColors.Text);
            DrawRunStats(MainGame.screenWidth / 5, MainGame.screenWidth / 2 + MainGame.screenWidth / 10);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "Press enter to continue", new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString("Press enter to continue").X / 2, 450), ThemeColors.Text);

            if (runeUnlocks.TryGetValue(((Runes.Runes)selectedRune, difficulty), out UnlockType unlock))
                UnlockManager.UnlockUnlock(unlock);
            if (!mistake)
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Star);

            if (IsConfirmPressed(state, mouseState))
            {
                SaveManager.RemoveGameData();
                GlyphManager.RemoveAllGlyphs();
                gameSaveData = null;
                Reset();
                gameState = GameState.MENU;
            }
        }

        private void DrawCardRewardScreen(KeyboardState state, MouseState mouseState)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Color cardColor = (i == afterFightSelect) ? ThemeColors.Selected : ThemeColors.Foreground;
                Rectangle rewardRect = new Rectangle(MainGame.screenWidth / 5 * (i + 1) + SaveManager.size * 30, 250, 160, 120);
                if (mouseState.LeftButton == ButtonState.Released) mousePressed = false;
                if (rewardRect.Contains(mouseState.Position) && !keyboardUsed)
                {
                    if (!mousePressed && mouseState.LeftButton == ButtonState.Pressed && windowActive)
                    {
                        mousePressed = true;
                        FightToMap();
                    }
                    afterFightSelect = i;
                }
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, rewardRect, cardColor);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, cards[i].letter + (cards[i].mult ? "  *" : "  +") + cards[i].value, new Vector2(rewardRect.X + 25, 250 + 30), ThemeColors.Text);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, "Current: " + enhancements.GetLetterScore(cards[i].letter), new Vector2(rewardRect.X + 20, 250 + 90), ThemeColors.Text);
            }

            string fightWon = "Fight won";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, fightWon, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(fightWon).X / 2, 70), ThemeColors.Text);
            string chooseReward = "Choose your reward";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, chooseReward, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(chooseReward).X / 2, 130), ThemeColors.Text);
            if (state.IsKeyDown(Keys.Enter))
                FightToMap();
        }

        private void FightToMap()
        {
            if (cards[afterFightSelect].mult)
                enhancements.MultiplyLetterScore(cards[afterFightSelect].letter, cards[afterFightSelect].value);
            else
                enhancements.AddLetterScore(cards[afterFightSelect].letter, cards[afterFightSelect].value);

            SaveManager.SaveGame(seed, level, coins, lastSelectedNode, enhancements, difficulty, selectedRune, visitedNodes);
            roomSelected = false;
            canStartFight = false;

            var completedNodeType = selectedNode.type;

            if (completedNodeType == NodeType.BOSS)
            {
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Naudhiz0);
                if (Writer.diffIndexes.Count == 0)
                    UnlockManager.UnlockUnlock(UnlockManager.UnlockType.R);
                level++;
                visitedNodes = new List<int[]>();
                SetContext(-1, 0);
                map.GenerateNodes();
                selectedNode = map.GetFirstNode();
            }
            if (completedNodeType == NodeType.ELITE)
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.S);
            if (Writer.diffIndexes.Count >= 10)
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.EyeOfHorus);

            SteamManager.IncrementStat(SteamManager.SteamStats.Letters, Writer.writtenText.Count);
            SteamManager.IncrementStat(SteamManager.SteamStats.Words, Writer.writtenText.Count(c => c == ' '));
            SteamManager.IncrementStat(SteamManager.SteamStats.FightsWon);
            GlyphManager.SetUnlockedGlyphs();
        }
    }
}
