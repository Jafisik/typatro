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
using static typatro.GameFolder.Services.EnemyManager;

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
                gameState = isDebugFight ? GameState.DEBUG : GameState.MENU;
                isDebugFight = false;
            }
        }

        Keys[] prevKeys = new Keys[0];
        float pitch = 0f;
        int prevMistakes = 0;
        private void RoomHandler(KeyboardState state, MouseState mouseState)
        {
            if (state.IsKeyDown(Keys.Tab) && (!IsFight(selectedNode.type) || timeInSeconds == 0 || afterFightScreen))
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
                    if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.TreasureTutorial))
                        if (TutorialManager.Draw(state, mouseState))
                            UnlockManager.UnlockUnlock(UnlockManager.UnlockType.TreasureTutorial);
                    break;
                case NodeType.SHOP:
                    if (!inventoryUp) isFightFinished = shop.DisplayShop(ref coins, ref mousePressed);
                    gameUi.TopBannerDisplay(true, showCoins: false);
                    if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.ShopTutorial))
                        if (TutorialManager.Draw(state, mouseState))
                            UnlockManager.UnlockUnlock(UnlockManager.UnlockType.ShopTutorial);
                    break;
                case NodeType.CURSE:
                    gameUi.TopBannerDisplay(true);
                    if (!inventoryUp) isFightFinished = curseRoom.CurseRoomDisplay(ref coins, ref mousePressed);
                    if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.CurseTutorial))
                        if (TutorialManager.Draw(state, mouseState))
                            UnlockManager.UnlockUnlock(UnlockManager.UnlockType.CurseTutorial);
                    break;
            }
        }

        private void HandleFightRoom(KeyboardState state, MouseState mouseState)
        {
            // Nothing else about the fight shows during this window - just the reassurance
            // message on a blank screen - so the retry doesn't read as the enemy/text
            // flickering back in mid-transition.
            if (MainGame.time.TotalGameTime.TotalSeconds < tutorialRetryMsgUntil)
            {
                string retryText = "Don't worry, let's try that again!";
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, retryText,
                    new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(retryText).X / 2, MainGame.screenHeight / 2 - 20),
                    ThemeColors.Text);
                return;
            }

            const double introHold = 3.0, introSlide = 1.0;
            if (state.IsKeyUp(Keys.Enter)) introEnterReady = true;
            // An eager player pressing a typing key (or a fresh Enter) during the hold jumps
            // straight to the slide - checked against the raw key (letters/space only, mirroring
            // Writer.ConvertKeyToChar) rather than Writer.writtenText, since actual typing is
            // blocked entirely until the intro is over (see TypingSystem in Update.cs). Enter
            // only counts once introEnterReady has seen it released at least once since the
            // intro started, so the same held-down Enter used to confirm the fight's map node
            // can't instantly skip it before the player lets go.
            if (enemyIntroActive && enemyIntroTimer < introHold &&
                (state.GetPressedKeys().Any(k => (k >= Keys.A && k <= Keys.Z) || k == Keys.Space) ||
                 (introEnterReady && state.IsKeyDown(Keys.Enter))))
                enemyIntroTimer = introHold;
            bool introPlaying = enemyIntroActive;
            if (enemyIntroActive)
            {
                enemyIntroTimer += MainGame.time.ElapsedGameTime.TotalSeconds;
                if (enemyIntroTimer >= introHold + introSlide) enemyIntroActive = false;
            }

            // Calculated before the text is drawn below, so the current word's live base is
            // ready in time to be drawn glued to the caret.
            scoreCalculator.CalculateScore(ref fight, ref enhancements, showHud: !introPlaying);

            // While the enemy intro is playing, only the enemy + its description show - no
            // hint/input text, banner, or health bar yet, until it's settled in the corner.
            if (!introPlaying)
            {
                string hintText = Is(EnemyType.D) ? MaskUpcomingWords(neededText, Writer.writtenText.Count) : neededText;
                float specialFlashAlpha = (float)Math.Max(0, 1 - (timeInSeconds - scoreCalculator.specialFlashTime) / Logic.ScoreCalculator.specialFlashDuration);
                // Fires exactly once, the same frame CalculateScore just set specialFlashTime
                // to the current timeInSeconds - timeInSeconds only advances again next frame.
                if (scoreCalculator.specialFlashTime == timeInSeconds)
                    (scoreCalculator.specialFlashHit ? sfx.specialWordHit : sfx.specialWordMiss).Play((float)SaveManager.volume / 10, 0f, 0f);
                writer.WriteText(hintText, ThemeColors.Selected, shinyWords, stoneWords, bloomWords, isHintText: true, rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset,
                    flashWordIndex: scoreCalculator.specialFlashWordIndex, flashColor: scoreCalculator.specialFlashColor, flashAlpha: specialFlashAlpha);
                if (UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.FightTutorial))
                    writer.UserInputText(Writer.writtenText.ToArray(), enhancements.mistakeBlock, wordBase: scoreCalculator.currentWordBase, rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset);
            }
            if (!introPlaying)
            {
                gameUi.TopBannerDisplay(false);
                if (!EnemyManager.Is(EnemyType.C))
                    gameUi.HealthBar(ref fight, scoreCalculator.currentScore, scoreCalculator.displayedScore);
            }

            if (kHeperShieldActive)
            {
                string shieldText = "SHIELD";
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, shieldText,
                    new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(shieldText).X / 2, 160),
                    ThemeColors.Selected);
            }


            PlayTypeSound(state);

            if (currentEnemy?.Texture != null)
            {
                int frameWidth = currentEnemy.Texture.Width / 4;
                int frameHeight = currentEnemy.Texture.Height;
                int frame = (int)(MainGame.time.TotalGameTime.TotalSeconds) % 4;
                int scale = 6;
                int border = 6;
                Rectangle sourceRect = new Rectangle(frame * frameWidth, 0, frameWidth, frameHeight);
                Rectangle destRect = new Rectangle(MainGame.screenWidth - frameWidth * scale - frameWidth, MainGame.screenHeight - frameHeight * scale - frameHeight, frameWidth * scale, frameHeight * scale);
                int cornerBorderWidth = destRect.Width + border * 2;

                // Past the halfway point of the slide, the description's wrap width snaps
                // back to the normal corner width instead of continuing to track the
                // (by-then much narrower) box - matches how it reads once settled, rather
                // than squeezing text into an ever-shrinking line.
                bool useWideText = false;
                float slideT = 1f;

                if (introPlaying)
                {
                    // Big, centered and pushed further down the screen for the hold, then
                    // eases up into its usual corner spot over introSlide seconds.
                    float aspect = (float)frameWidth / frameHeight;
                    int bigHeight = (int)(MainGame.screenHeight * 0.55f);
                    int bigWidth = (int)(bigHeight * aspect);
                    int bigYOffset = 70;
                    Rectangle bigRect = new Rectangle(MainGame.screenWidth / 2 - bigWidth / 2, MainGame.screenHeight / 2 - bigHeight / 2 + bigYOffset, bigWidth, bigHeight);

                    slideT = (float)Math.Clamp((enemyIntroTimer - introHold) / introSlide, 0.0, 1.0);
                    useWideText = slideT < 0.5f;
                    slideT = slideT * slideT * (3f - 2f * slideT); // smoothstep - less mechanical than a linear slide
                    destRect = new Rectangle(
                        (int)MathHelper.Lerp(bigRect.X, destRect.X, slideT),
                        (int)MathHelper.Lerp(bigRect.Y, destRect.Y, slideT),
                        (int)MathHelper.Lerp(bigRect.Width, destRect.Width, slideT),
                        (int)MathHelper.Lerp(bigRect.Height, destRect.Height, slideT));
                }

                Rectangle borderRect = new Rectangle(destRect.X - border, destRect.Y - border, destRect.Width + border * 2, destRect.Height + border * 2);

                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(borderRect.X, borderRect.Y, borderRect.Width, border), ThemeColors.Foreground);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(borderRect.X, borderRect.Bottom - border, borderRect.Width, border), ThemeColors.Foreground);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(borderRect.X, borderRect.Y, border, borderRect.Height), ThemeColors.Foreground);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(borderRect.Right - border, borderRect.Y, border, borderRect.Height), ThemeColors.Foreground);

                Color enemyBg = ThemeColors.Background;
                enemyBg.A = 160;
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, destRect, enemyBg);
                MainGame.Gfx.spriteBatch.Draw(currentEnemy.Texture, destRect, sourceRect, Color.White);

                // Snaps from gameFont to smallTextFont partway through the slide, same
                // threshold as the wrap width - a continuous shrink read as jittery.
                SpriteFont descFont = (introPlaying && useWideText) ? MainGame.Gfx.gameFont : MainGame.Gfx.smallTextFont;
                float descScale = useWideText ? 1.3f : 1f;
                float wrapWidth = useWideText ? MainGame.screenWidth * 0.75f : cornerBorderWidth;
                string wrapped = WrapText(descFont, currentEnemy.Description, wrapWidth / descScale);
                Vector2 descSize = descFont.MeasureString(wrapped) * descScale;
                MainGame.Gfx.spriteBatch.DrawString(descFont, wrapped,
                    new Vector2(borderRect.X + borderRect.Width / 2 - descSize.X / 2, borderRect.Y - descSize.Y - 5),
                    ThemeColors.Text, 0f, Vector2.Zero, descScale, SpriteEffects.None, 0f);
            }

            if (jumpscareActive && MainGame.Gfx.foxy != null)
            {
                float t = (float)((timeInSeconds - (jumpscareEndTime - 0.4)) / 0.4);
                t = Math.Clamp(t, 0f, 1f);
                int w = (int)(MainGame.screenWidth * t);
                int h = (int)(MainGame.screenHeight * t);
                int x = (MainGame.screenWidth - w) / 2;
                int y = (MainGame.screenHeight - h) / 2;
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.foxy, new Rectangle(x, y, w, h), Color.White);
            }

            if (eyeOfHorusActive || molochActive)
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(0, 0, MainGame.screenWidth, MainGame.screenHeight), Color.Black);
            if (!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.Cat))
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.catPic, new Rectangle((int)catPos.X, (int)catPos.Y, 120, 80), Color.White);
            foreach (var (pos, _) in wendigoBugs)
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle((int)pos.X, (int)pos.Y, 4, 4), Color.Green);

            if (ictusFlashActive > 0)
            {
                MainGame.Gfx.spriteBatch.End();
                MainGame.Gfx.spriteBatch.Begin(SpriteSortMode.Deferred, blendState: MainGame.Gfx.invertBlend, samplerState: SamplerState.PointClamp);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(0, 0, MainGame.screenWidth, MainGame.screenHeight), Color.White);
                MainGame.Gfx.spriteBatch.End();
                MainGame.Gfx.spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
            }

            if (Is(EnemyType.Q) && !isFightFinished)
            {
                const double closeTime = 60.0, killDelay = 10.0;
                float t = (float)Math.Min(timeInSeconds / closeTime, 1.0);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(0, 0, MainGame.screenWidth, MainGame.screenHeight), Color.Black * t);

                if (timeInSeconds >= closeTime + killDelay)
                    dead = true;
            }

            bool textFinished = Writer.writtenText.Count == neededText.Length;
            bool enemyDefeated = scoreCalculator.currentScore >= fight.scoreNeeded;
            const double fightWinHold = 1.1;
            if (enemyDefeated && Is(EnemyType.P) && !polemanRespawned && !textFinished)
            {
                polemanRespawned = true;
                fight.scoreNeeded += fight.scoreNeeded / 2;
            }
            // A clear "defeated" beat instead of cutting straight to the reward screen the
            // instant the score threshold is crossed - running out of text without enough
            // score is a loss instead, which stays immediate.
            else if (enemyDefeated && !fightWinPending)
            {
                fightWinPending = true;
                fightWinPauseUntil = MainGame.time.TotalGameTime.TotalSeconds + fightWinHold;
                CreditUnreachedBloomWords();
                sfx.enemyDefeated.Play((float)SaveManager.volume / 10, 0f, 0f);
            }
            else if (textFinished && !enemyDefeated)
                isFightFinished = true;

            if (fightWinPending)
            {
                double elapsed = fightWinHold - (fightWinPauseUntil - MainGame.time.TotalGameTime.TotalSeconds);
                float t = (float)Math.Clamp(elapsed / fightWinHold, 0.0, 1.0);
                float alpha = t < 0.15f ? t / 0.15f : (t > 0.8f ? (1f - t) / 0.2f : 1f);
                float scale = 1f + 0.3f * (1f - MathF.Pow(1f - Math.Min(t / 0.3f, 1f), 3));

                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(0, 0, MainGame.screenWidth, MainGame.screenHeight), Color.Black * (0.55f * alpha));

                string winText = "Enemy defeated!";
                Vector2 winSize = MainGame.Gfx.menuFont.MeasureString(winText) * scale;
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, winText,
                    new Vector2(MainGame.screenWidth / 2 - winSize.X / 2, MainGame.screenHeight / 2 - winSize.Y / 2),
                    Color.White * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                if (MainGame.time.TotalGameTime.TotalSeconds >= fightWinPauseUntil)
                {
                    fightWinPending = false;
                    isFightFinished = true;
                }
            }

            if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.FightTutorial))
            {
                if (TutorialManager.Draw(state, mouseState))
                    UnlockManager.UnlockUnlock(UnlockManager.UnlockType.FightTutorial);
            }
            else if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.SpecialWordsTutorial))
            {
                if (TutorialManager.Draw(state, mouseState))
                    UnlockManager.UnlockUnlock(UnlockManager.UnlockType.SpecialWordsTutorial);
            }
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
            else if (seed == 10)
            {
                RetryTutorialFight();
                return;
            }
            else
            {
                dead = true;
                playerSurvived = false;
            }

            if (isDebugFight && playerSurvived)
            {
                isDebugFight = false;
                roomSelected = false;
                canStartFight = false;
                Reset();
                gameState = GameState.DEBUG;
                return;
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

                if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.RewardTutorial))
                    TutorialManager.Start(TutorialManager.RewardSteps());
            }
        }

        // A fight normally ends the instant enough score is banked, well before the player
        // reaches the end of the generated text (that extra length is just a safety buffer for
        // weaker typing) - any bloom word sitting past wherever the player stopped would
        // otherwise never trigger, wasted purely because of where it happened to land rather
        // than anything the player did. Auto-credit those as if typed correctly instead, so
        // playing well is never quietly punished for outrunning the text.
        private void CreditUnreachedBloomWords()
        {
            if (bloomWords.Count == 0) return;
            int typedChars = Writer.writtenText.Count;
            int bloomBoost = enhancements.bloomScore + (GlyphManager.IsActive(Glyph.S) ? 5 : 0);
            string[] words = neededText.Split(' ');
            int offset = 0;
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0) continue;
                if (bloomWords.Contains(i) && typedChars <= offset)
                {
                    foreach (char c in words[i])
                        enhancements.AddLetterScore(c, bloomBoost);
                    scoreCalculator.CreditUnreachedBloom();
                }
                offset += words[i].Length + 1;
            }
        }

        // Resets the same tutorial fight for another go instead of letting the player die -
        // mirrors the setup done on first entering the fight (see MapHandler's NodeSelect
        // branch in Map.cs), just without re-rolling the enemy or difficulty.
        private void RetryTutorialFight()
        {
            Reset();
            EnemyManager.SetActive(currentEnemy.Type);
            bool allowSpecialWords = UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.FightTutorial);
            neededText = RandomTextGenerate(fight.words + (GlyphManager.IsActive(Glyph.Papyrus) ? 20 : 0) - (difficulty >= 5 ? 5 : 0), allowSpecialWords);
            Writer.writtenText.Clear();
            Writer.diffIndexes.Clear();
            startedTyping = false;
            canStartFight = true;
            timeInSeconds = 0;
            tutorialRetryMsgUntil = MainGame.time.TotalGameTime.TotalSeconds + 2.0;
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

            if (selectedNode.type == NodeType.BOSS && seed == 10)
                DrawTutorialCompleteScreen(state, mouseState);
            else if (selectedNode.type == NodeType.BOSS && level == 3)
                DrawBossWinScreen(state, mouseState);
            else
                DrawCardRewardScreen(state, mouseState);
        }

        // Shown instead of the usual letter-reward screen when the tutorial map's boss is
        // beaten - skips straight to a real character select instead of handing out a letter
        // reward for a run that's about to be discarded.
        private void DrawTutorialCompleteScreen(KeyboardState state, MouseState mouseState)
        {
            string title = "Tutorial complete!";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, title, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(title).X / 2, 200), ThemeColors.Text);
            string subtitle = "Time to pick your rune and start your first real run";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, subtitle, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.smallTextFont.MeasureString(subtitle).X / 2, 260), ThemeColors.Text);
            string confirm = "Press enter to continue";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, confirm, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(confirm).X / 2, 450), ThemeColors.Text);

            if (IsConfirmPressed(state, mouseState))
            {
                // This boss kill is real (same unlocks/stats FightToMap() would give a normal
                // boss win) even though the tutorial run itself gets discarded below.
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Naudhiz0);
                if (Writer.diffIndexes.Count == 0)
                    UnlockManager.UnlockUnlock(UnlockManager.UnlockType.R);
                SteamManager.IncrementStat(SteamManager.SteamStats.Letters, Writer.writtenText.Count);
                SteamManager.IncrementStat(SteamManager.SteamStats.Words, Writer.writtenText.Count(c => c == ' '));
                SteamManager.IncrementStat(SteamManager.SteamStats.FightsWon);

                // MapTutorial is already unlocked at this point, so NewGame() takes its
                // non-firstRun branch: a real random map and gameState = RUNES.
                NewGame();
                roomSelected = false;
                canStartFight = false;
                firstEnter = true;
            }
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
            bool rewardTutorialDone = UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.RewardTutorial);

            const int cardWidth = 180, cardHeight = 150, gap = 30, cardY = 330;
            int rowWidth = cards.Count * cardWidth + (cards.Count - 1) * gap;
            int startX = MainGame.screenWidth / 2 - rowWidth / 2;

            Rectangle panelRect = new Rectangle(startX - 30, cardY - 30, rowWidth + 60, cardHeight + 60);
            Color panelColor = Color.Lerp(ThemeColors.Background, ThemeColors.Text, 0.15f);
            panelColor.A = 235;
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, panelRect, panelColor);

            for (int i = 0; i < cards.Count; i++)
            {
                Rectangle rewardRect = new Rectangle(startX + i * (cardWidth + gap), cardY, cardWidth, cardHeight);
                bool selected = i == afterFightSelect;
                Color cardColor = selected ? ThemeColors.Selected : ThemeColors.Foreground;

                if (mouseState.LeftButton == ButtonState.Released) mousePressed = false;
                if (rewardRect.Contains(mouseState.Position) && !keyboardUsed)
                {
                    if (rewardTutorialDone && !mousePressed && mouseState.LeftButton == ButtonState.Pressed && windowActive)
                    {
                        mousePressed = true;
                        FightToMap();
                    }
                    afterFightSelect = i;
                }
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, rewardRect, cardColor);
                if (selected) ThemeColors.DrawGlowCorners(rewardRect, ThemeColors.Text);

                // Stacked instead of side-by-side: letter takes the top half, current value
                // and what it adds each take a quarter underneath.
                int quarterHeight = rewardRect.Height / 4;
                Rectangle letterZone = new Rectangle(rewardRect.X, rewardRect.Y, rewardRect.Width, rewardRect.Height - quarterHeight * 2);
                Rectangle currentZone = new Rectangle(rewardRect.X, letterZone.Bottom, rewardRect.Width, quarterHeight);
                Rectangle addZone = new Rectangle(rewardRect.X, currentZone.Bottom, rewardRect.Width, rewardRect.Bottom - currentZone.Bottom);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, currentZone, ThemeColors.NotSelected);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, addZone, ThemeColors.ShopReroll);

                string letterText = cards[i].letter.ToString();
                float letterScale = 1.3f;
                Vector2 letterSize = MainGame.Gfx.menuFont.MeasureString(letterText) * letterScale;
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, letterText,
                    new Vector2((int)(letterZone.X + letterZone.Width / 2f - letterSize.X / 2f), (int)(letterZone.Y + letterZone.Height / 2f - letterSize.Y / 2f) + 10),
                    ThemeColors.Text, 0f, Vector2.Zero, letterScale, SpriteEffects.None, 0f);

                string currentText = "Now: " + enhancements.GetLetterScore(cards[i].letter);
                string addText = (cards[i].mult ? "*" : "+") + cards[i].value.ToString();
                float numScale = 0.6f;
                Vector2 currentSize = MainGame.Gfx.gameFont.MeasureString(currentText) * numScale;
                Vector2 addSize = MainGame.Gfx.gameFont.MeasureString(addText) * numScale;
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, currentText,
                    new Vector2((int)(currentZone.X + currentZone.Width / 2f - currentSize.X / 2f), (int)(currentZone.Y + currentZone.Height / 2f - currentSize.Y / 2f)),
                    ThemeColors.Text, 0f, Vector2.Zero, numScale, SpriteEffects.None, 0f);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, addText,
                    new Vector2((int)(addZone.X + addZone.Width / 2f - addSize.X / 2f), (int)(addZone.Y + addZone.Height / 2f - addSize.Y / 2f)),
                    ThemeColors.Text, 0f, Vector2.Zero, numScale, SpriteEffects.None, 0f);
            }

            string fightWon = "Fight won";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, fightWon, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(fightWon).X / 2, 140), ThemeColors.Text);
            string chooseReward = "Choose your reward";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, chooseReward, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(chooseReward).X / 2, 200), ThemeColors.Text);

            if (!rewardTutorialDone)
            {
                if (TutorialManager.Draw(state, mouseState))
                    UnlockManager.UnlockUnlock(UnlockManager.UnlockType.RewardTutorial);
            }
            else if (state.IsKeyDown(Keys.Enter))
            {
                FightToMap();
            }
        }

        private void FightToMap()
        {
            if (cards[afterFightSelect].mult)
                enhancements.MultiplyLetterScore(cards[afterFightSelect].letter, cards[afterFightSelect].value);
            else
                enhancements.AddLetterScore(cards[afterFightSelect].letter, cards[afterFightSelect].value);

            if (!isDebugFight)
                SaveManager.SaveGame(seed, level, coins, lastSelectedNode, enhancements, difficulty, selectedRune, visitedNodes);
            roomSelected = false;
            canStartFight = false;

            if (isDebugFight)
            {
                isDebugFight = false;
                gameState = GameState.DEBUG;
                return;
            }

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
