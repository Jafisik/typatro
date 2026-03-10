using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.Services;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder.Logic
{
    public class ScoreCalculator
    {
        int charCounter = 0, lastCharCount = 0, wordCounter = 0, lastWordCount = 1, lastCorrectWord = 0, extraScore = 0;
        long playerScore = 0, lastScore = 0;
        public long currentScore = 0;
        long shinyWritten, stoneWritten, bloomWritten;
        GameLogic gameLogic;
        public ScoreCalculator(GameLogic gameLogic)
        {
            this.gameLogic = gameLogic;
        }
        public void CalculateScore(Vector2 lastCharPos, ref Fight fight, ref Enhancements enhancements)
        {
            if (Writer.diffIndexes.Count > 0) gameLogic.mistake = true;
            long letterScore = 0;
            for (int i = 0; i < Writer.writtenText.Count; i++)
            {
                bool canAdd = true;
                for (int j = 0; j < Writer.diffIndexes.Count; j++)
                {
                    if (i == j) canAdd = false;
                }
                if (canAdd && Writer.writtenText[i] != ' ')
                {
                    letterScore += enhancements.letters[Writer.writtenText[i] - 'a'];
                }
            }

            if (Writer.writtenText.Count != lastCharCount)
            {
                if (GlyphManager.IsActive(Glyph.Thousand))
                {
                    charCounter++;
                    if (charCounter == 1000)
                    {
                        charCounter = 0;
                        extraScore += 100000;
                    }
                }
                lastScore = playerScore;
                gameLogic.letterTimer = gameLogic.timeInSeconds;
                lastCharCount = Writer.writtenText.Count;
            }

            if ((int)gameLogic.timeInSeconds == 60)
            {
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.M);
            }

            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, $"Mult:{gameLogic.wordStreak: 0.##}x", new Vector2(50, 100), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "Base:", new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString("Base:").X, 100), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, ((int)(currentScore / gameLogic.wordStreak)).ToString(), new Vector2(MainGame.screenWidth / 2, 100), ThemeColors.Text);

            string rewardText = "Reward: " + fight.cashGain;
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, rewardText, new Vector2(MainGame.screenWidth - 50 - MainGame.Gfx.gameFont.MeasureString(rewardText).X, 100), ThemeColors.Text);

            int mistakeCount = Math.Max(Writer.diffIndexes.Count - (GlyphManager.IsActive(Glyph.EyeOfHorus) ? 2 : 0), 0);
            if (GlyphManager.IsActive(Glyph.Star) && mistakeCount > 0) gameLogic.dead = true;

            string userWords = new string(Writer.writtenText.ToArray());
            int correctWords = 0;

            List<int> neededStarts = new List<int>();

            int start = 0;
            for (int i = 0; i <= gameLogic.neededText.Length; i++)
            {
                if (i == gameLogic.neededText.Length || gameLogic.neededText[i] == ' ')
                {
                    if (i > start) neededStarts.Add(start);
                    start = i;
                }
            }


            int word = -1;
            double shinyMultiplier = 1;
            int stoneScore = 0;
            for (int i = 0; i < neededStarts.Count - 1; i++)
            {
                int wordLength = neededStarts[i + 1] - neededStarts[i] - 1;
                if (userWords.Length < neededStarts[i] + wordLength + 1) break;

                word++;
                //these (i==0?-1:0) are to adjust for the lack of spaces in the first word
                string neededWord = gameLogic.neededText.Substring(neededStarts[i] + 1 + (i == 0 ? -1 : 0), wordLength + (i == 0 ? 1 : 0));
                string userWord = userWords.Substring(neededStarts[i] + 1 + (i == 0 ? -1 : 0), wordLength + (i == 0 ? 1 : 0));
                if (userWord == neededWord)
                {
                    correctWords++;
                    if (gameLogic.shinyWords.Contains(word))
                    {
                        //shinyMultiplier *= enhancements.shinyScore;
                    }
                    else if (gameLogic.stoneWords.Contains(word))
                    {
                        stoneScore += enhancements.stoneScore;
                    }
                }
            }

            if (userWords.Length != 0 && userWords.Length != lastWordCount && neededStarts.Contains(userWords.Length))
            {
                bool under3Sec = gameLogic.timeInSeconds - gameLogic.timeSinceLastWord < 3;
                if (!under3Sec && GlyphManager.IsActive(Glyph.N)) gameLogic.wordStreak = 0;
                gameLogic.timeSinceLastWord = gameLogic.timeInSeconds;
                lastWordCount = userWords.Length;
                wordCounter++;
                if (correctWords > lastCorrectWord)
                {
                    lastCorrectWord = correctWords;
                    gameLogic.wordStreak += GlyphManager.IsActive(Glyph.Scarab) ? 0.1 : 0.05;
                    if (gameLogic.wordStreak > gameLogic.highestStreak) gameLogic.highestStreak = (int)((gameLogic.wordStreak - 1) * 100);
                    extraScore += GlyphManager.IsActive(Glyph.N) && under3Sec ? 2 : 0;
                    if (gameLogic.shinyWords.Contains(word))
                    {
                        shinyWritten++;
                        gameLogic.wordStreak += enhancements.shinyScore;
                    }
                    else if (gameLogic.stoneWords.Contains(word))
                    {
                        stoneWritten++;
                    }
                    else if (gameLogic.bloomWords.Contains(word))
                    {
                        bloomWritten++;
                        string correctWord = userWords.Substring(neededStarts[word] + 1 + (word == 0 ? -1 : 0), neededStarts[word + 1] - neededStarts[word] - 1 + (word == 0 ? 1 : 0));
                        char[] correctWordChars = correctWord.ToCharArray();
                        foreach (char correctLetter in correctWordChars)
                        {
                            enhancements.AddLetterScore(correctLetter, 1);
                        }
                    }
                }
                else if (correctWords == lastCorrectWord)
                {
                    lastCorrectWord = correctWords;
                    gameLogic.wordStreak = 1;
                }
            }

            if (GlyphManager.IsActive(Glyph.Anubis) && wordCounter % 5 == 0)
            {
                if (!gameLogic.anubisActive && wordCounter > 0) gameLogic.coins++;
                gameLogic.anubisActive = true;
            }
            else gameLogic.anubisActive = false;

            if (gameLogic.startedTyping)
            {
                playerScore = (int)((extraScore + enhancements.mistakeBlock + letterScore + stoneScore) * shinyMultiplier);
                long enemyDamage = (long)((GlyphManager.IsActive(Glyph.House) ? 0.25 : 1) * (int)gameLogic.timeInSeconds) * fight.speed - enhancements.damageResist;
                long mistakeDamage = (GlyphManager.IsActive(Glyph.Snake) ? 0 : 1) * (GlyphManager.IsActive(Glyph.R) ? 5 : 1) * (gameLogic.difficulty >= 2 ? 5 : 1) * mistakeCount;
                currentScore = (int)((playerScore + correctWords * enhancements.streakMult - (enemyDamage < 0 ? 1 * (int)gameLogic.timeInSeconds : enemyDamage) - mistakeDamage) * gameLogic.wordStreak);
                if (Writer.writtenText.Count == 1) lastScore = 0;
                if (playerScore - lastScore != 0 && gameLogic.timeInSeconds - gameLogic.letterTimer < 0.25 && Writer.writtenText.Count > 0) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "+" + (playerScore - lastScore).ToString(), lastCharPos, ThemeColors.Correct);
            }
            else currentScore = enhancements.mistakeBlock;
            if (currentScore <= -100)
            {
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Snake);
            }
        }

        public void Reset()
        {
            lastCharCount = 0;
            lastWordCount = 1;
            lastCorrectWord = 0;
            extraScore = 0;
            lastScore = 0;
        }

        public long getShinyWritten()
        {
            return shinyWritten;
        }

        public long getStoneWritten()
        {
            return stoneWritten;
        }

        public long getBloomWritten()
        {
            return bloomWritten;
        }
    }
}
