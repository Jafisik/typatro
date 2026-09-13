using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.Services;
using static typatro.GameFolder.Services.EnemyManager;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder.Logic
{
    public class ScoreCalculator
    {
        int charCounter = 0, lastCharCount = 0, wordCounter = 0, lastWordCount = 1, lastCorrectWord = 0, extraScore = 0, lastMistakeCount = 0;
        long playerScore = 0;
        public long currentScore = 0;
        long shinyWritten, stoneWritten, bloomWritten;

        // Score is locked in per word instead of the streak mult re-scaling the whole fight's
        // score every frame - otherwise a mistake resetting the mult instantly shrank
        // everything you'd already earned, which read as the enemy "healing". bankedScore only
        // ever grows (once a word is committed, later mult changes can't touch it);
        // positiveScoreAtLastBoundary is the raw (pre-mult) running total as of the last word
        // boundary, so the delta since then is the word currently being typed.
        long bankedScore = 0, positiveScoreAtLastBoundary = 0;
        bool wordJustBoundaried = false;
        public double displayedScore = 0;
        double bankFlashTime = -10;
        long lastBankAmount = 0, lastBankBase = 0;
        double lastBankMult = 1;
        // Base of the word currently being typed (0 between words) - read by Writer to draw it
        // glued to the caret, following the same rotation/wobble as the text itself.
        public long currentWordBase = 0;
        // Brief highlight drawn behind a special word in the hint text the moment it lands
        // (correctly or not) - read by Writer, faded out over specialFlashDuration seconds.
        public const double specialFlashDuration = 0.6;
        public int specialFlashWordIndex = -1;
        public double specialFlashTime = -10;
        public Color specialFlashColor;
        // Lets Play.cs tell hit and miss apart to play the right sound, without knowing
        // anything about colors itself.
        public bool specialFlashHit;
        static readonly Color failedFlashColor = new Color(210, 45, 45);
        HashSet<int> kHeperBlockedIndexes = new();
        // Each character's letter-score contribution, locked in the moment it's typed (using
        // enhancements.letters as it stood right then) instead of being re-summed from the
        // live enhancements.letters every frame. Enemy T permanently lowers a letter's score
        // on a mistake - re-summing live would retroactively re-price every earlier correct
        // occurrence of that letter in the same fight all at once, banking as a huge,
        // seemingly random loss at the next word boundary.
        List<long> letterScoreSnapshot = new();
        GameLogic gameLogic;
        public ScoreCalculator(GameLogic gameLogic)
        {
            this.gameLogic = gameLogic;
        }
        public void CalculateScore(ref Fight fight, ref Enhancements enhancements, bool showHud = true)
        {
            if (Writer.diffIndexes.Count > 0) gameLogic.mistake = true;

            if (Writer.writtenText.Count != lastCharCount)
            {
                if (GlyphManager.IsActive(Glyph.Thousand))
                {
                    charCounter++;
                    if (charCounter == 1000)
                    {
                        charCounter = 0;
                        // Straight into bankedScore, not extraScore - it's a one-off milestone
                        // bonus, not part of any word, so it shouldn't leak into the live
                        // word-base preview.
                        bankedScore += 100000;
                    }
                }
                if (Is(EnemyType.H) && gameLogic.kHeperShieldActive && Writer.writtenText.Count > lastCharCount)
                    kHeperBlockedIndexes.Add(Writer.writtenText.Count - 1);
                if (Is(EnemyType.T) && Writer.writtenText.Count > lastCharCount)
                {
                    char newChar = Writer.writtenText[Writer.writtenText.Count - 1];
                    if (newChar != ' ')
                    {
                        bool isMistake = Writer.diffIndexes.Contains(Writer.writtenText.Count - 1);
                        enhancements.AddLetterScore(newChar, isMistake ? -10 : 1);
                    }
                }
                lastCharCount = Writer.writtenText.Count;
            }

            while (letterScoreSnapshot.Count > Writer.writtenText.Count)
                letterScoreSnapshot.RemoveAt(letterScoreSnapshot.Count - 1);
            while (letterScoreSnapshot.Count < Writer.writtenText.Count)
            {
                int i = letterScoreSnapshot.Count;
                char c = Writer.writtenText[i];
                long score = 0;
                if (!Writer.diffIndexes.Contains(i) && c != ' ')
                {
                    bool blockedByEnemy = (c == 'e' && EnemyManager.Is(EnemyType.E))
                                      || (c == 'o' && EnemyManager.Is(EnemyType.O))
                                      || kHeperBlockedIndexes.Contains(i);
                    if (!blockedByEnemy)
                        score = enhancements.letters[c - 'a'];
                }
                letterScoreSnapshot.Add(score);
            }
            long letterScore = 0;
            foreach (long s in letterScoreSnapshot) letterScore += s;

            if ((int)gameLogic.timeInSeconds == 60)
            {
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.M);
            }

            if (Is(EnemyType.S) && gameLogic.timeInSeconds >= 60 && gameLogic.startedTyping && !gameLogic.isFightFinished)
                gameLogic.dead = true;

            if (!Is(EnemyType.C) && showHud)
            {
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, $"Mult:{gameLogic.wordStreak: 0.##}x", new Vector2(50, 100), ThemeColors.Text);
                string rewardText = "Reward: " + fight.cashGain;
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, rewardText, new Vector2(MainGame.screenWidth - 50 - MainGame.Gfx.gameFont.MeasureString(rewardText).X, 100), ThemeColors.Text);
            }

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
                        shinyMultiplier *= enhancements.shinyScore;
                    }
                    else if (gameLogic.stoneWords.Contains(word))
                    {
                        stoneScore += enhancements.stoneScore;
                    }
                }
            }

            if (userWords.Length != 0 && userWords.Length != lastWordCount && neededStarts.Contains(userWords.Length))
            {
                wordJustBoundaried = true;
                bool under3Sec = gameLogic.timeInSeconds - gameLogic.timeSinceLastWord < 3;
                if (!under3Sec && GlyphManager.IsActive(Glyph.N)) gameLogic.wordStreak = 0;
                bool under2Sec = gameLogic.timeInSeconds - gameLogic.timeSinceLastWord < 2;
                if (!under2Sec && Is(EnemyType.J)) gameLogic.wordStreak = 1;
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
                        specialFlashWordIndex = word;
                        specialFlashTime = gameLogic.timeInSeconds;
                        specialFlashColor = Color.Gold;
                        specialFlashHit = true;
                    }
                    else if (gameLogic.stoneWords.Contains(word))
                    {
                        stoneWritten++;
                        specialFlashWordIndex = word;
                        specialFlashTime = gameLogic.timeInSeconds;
                        specialFlashColor = Color.Gray;
                        specialFlashHit = true;
                    }
                    else if (gameLogic.bloomWords.Contains(word))
                    {
                        bloomWritten++;
                        string correctWord = userWords.Substring(neededStarts[word] + 1 + (word == 0 ? -1 : 0), neededStarts[word + 1] - neededStarts[word] - 1 + (word == 0 ? 1 : 0));
                        char[] correctWordChars = correctWord.ToCharArray();
                        int bloomBoost = enhancements.bloomScore + (GlyphManager.IsActive(Glyph.S) ? 5 : 0);
                        foreach (char correctLetter in correctWordChars)
                        {
                            enhancements.AddLetterScore(correctLetter, bloomBoost);
                        }
                        specialFlashWordIndex = word;
                        specialFlashTime = gameLogic.timeInSeconds;
                        specialFlashColor = Color.DarkGreen;
                        specialFlashHit = true;
                    }
                }
                else if (correctWords == lastCorrectWord)
                {
                    lastCorrectWord = correctWords;
                    // A special word typed wrong doesn't just silently skip its bonus - the same
                    // brief flash as a hit, just in the "missed" color, fading at the same rate.
                    if (gameLogic.shinyWords.Contains(word) || gameLogic.stoneWords.Contains(word) || gameLogic.bloomWords.Contains(word))
                    {
                        specialFlashWordIndex = word;
                        specialFlashTime = gameLogic.timeInSeconds;
                        specialFlashColor = failedFlashColor;
                        specialFlashHit = false;
                    }
                    if (!GlyphManager.IsActive(Glyph.Snake))
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
                double houseReduction = GlyphManager.IsActive(Glyph.House) ? 0.25 : 1;
                long enemyDamage = (long)(houseReduction * (int)gameLogic.timeInSeconds) * fight.speed - enhancements.damageResist;
                long clampedEnemyDamage = enemyDamage < 0 ? (int)gameLogic.timeInSeconds : enemyDamage;

                if (GlyphManager.IsActive(Glyph.R) && mistakeCount > lastMistakeCount)
                    gameLogic.wordStreak = Math.Min(gameLogic.wordStreak, 0.8);
                if (Is(EnemyType.B) && mistakeCount > lastMistakeCount)
                    gameLogic.coins = Math.Max(0, gameLogic.coins - (mistakeCount - lastMistakeCount) * 10);
                if (Is(EnemyType.N) && mistakeCount > lastMistakeCount)
                {
                    gameLogic.coins = Math.Max(0, gameLogic.coins - (mistakeCount - lastMistakeCount));
                    if (gameLogic.coins <= 0) gameLogic.dead = true;
                }
                // Mistake penalties hit bankedScore (HP) directly and immediately, not
                // extraScore - extraScore feeds the current word's live base, and a mistake
                // mid-word was making that preview swing randomly negative instead of only
                // affecting HP once the word actually banks.
                if (Is(EnemyType.X) && mistakeCount > lastMistakeCount)
                    bankedScore -= 5 * (mistakeCount - lastMistakeCount);
                // A mistake costs more the deeper into the run you are - level 1 is a light
                // -1, level 2 -5, level 3 -20. Xiphos already has its own flat penalty above,
                // so it's excluded here.
                else if (mistakeCount > lastMistakeCount)
                {
                    int mistakePenalty = gameLogic.level switch { 2 => 5, 3 => 20, _ => 1 };
                    bankedScore -= mistakePenalty * (mistakeCount - lastMistakeCount);
                }
                lastMistakeCount = mistakeCount;

                long positiveScore = Math.Max(0, playerScore + (long)(correctWords * enhancements.streakMult));

                // The word that just finished gets locked in at whatever mult applies right
                // now (already bumped up if it was correct, already reset to 1 if it wasn't) -
                // once it's in bankedScore, nothing later can shrink it. Only the word
                // currently being typed (the delta since the last boundary) still floats with
                // the live mult, as a preview.
                long deltaSinceBoundary = positiveScore - positiveScoreAtLastBoundary;
                if (wordJustBoundaried)
                {
                    lastBankBase = deltaSinceBoundary;
                    lastBankMult = gameLogic.wordStreak;
                    // A word's own banked amount can't go negative - a bad base times the mult
                    // floors at 0 for that word, it just doesn't add anything (separate mistake
                    // penalties above still hit bankedScore directly regardless).
                    long rawBankAmount = (long)(deltaSinceBoundary * gameLogic.wordStreak);
                    lastBankAmount = Math.Max(0, rawBankAmount);
                    if (rawBankAmount < 0)
                    {
                        string wordText = word >= 0 && word < neededStarts.Count - 1
                            ? gameLogic.neededText.Substring(neededStarts[word] + 1 + (word == 0 ? -1 : 0), neededStarts[word + 1] - neededStarts[word] - 1 + (word == 0 ? 1 : 0))
                            : "?";
                        Console.WriteLine($"[ScoreCalculator] word #{word} '{wordText}' would've banked negative, clamped to 0 - " +
                            $"base={deltaSinceBoundary} mult={gameLogic.wordStreak:0.##} raw={rawBankAmount} " +
                            $"letterScore={letterScore} extraScore={extraScore} stoneScore={stoneScore} shinyMult={shinyMultiplier:0.##} " +
                            $"mistakeBlock={enhancements.mistakeBlock} playerScore={playerScore} positiveScore={positiveScore} " +
                            $"positiveScoreAtLastBoundary={positiveScoreAtLastBoundary} mistakeCount={mistakeCount} " +
                            $"enemy={EnemyManager.ActiveEnemy} level={gameLogic.level}");
                    }
                    bankedScore += lastBankAmount;
                    positiveScoreAtLastBoundary = positiveScore;
                    deltaSinceBoundary = 0;
                    wordJustBoundaried = false;
                    bankFlashTime = gameLogic.timeInSeconds;
                }
                // HP only moves at the word-completion trigger above - not per letter - so it
                // stays flat while a word is in progress (the ticking time damage aside).
                currentScore = bankedScore - clampedEnemyDamage;
                currentWordBase = deltaSinceBoundary;
            }
            else
            {
                currentScore = enhancements.mistakeBlock;
                currentWordBase = 0;
            }

            // Bar/number ease toward the true score instead of snapping, so a banked word
            // visibly chips the health bar down rather than jumping instantly.
            displayedScore += (currentScore - displayedScore) * 0.15;
            if (Math.Abs(currentScore - displayedScore) < 1) displayedScore = currentScore;

            // At the moment a word banks: "base x mult =" small, then the actual damage dealt
            // big underneath - shows exactly how that number was reached instead of just a
            // bare +X.
            if (showHud && gameLogic.timeInSeconds - bankFlashTime < 0.6 && lastBankAmount != 0)
            {
                float t = (float)((gameLogic.timeInSeconds - bankFlashTime) / 0.6);
                float alpha = 1 - t;
                float rise = t * 20;

                string formulaText = $"{lastBankBase} x {lastBankMult:0.##} =";
                Vector2 formulaSize = MainGame.Gfx.smallTextFont.MeasureString(formulaText);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, formulaText,
                    new Vector2(MainGame.screenWidth / 2 - formulaSize.X / 2, 108 - rise), ThemeColors.Text * alpha);

                string damageText = lastBankAmount.ToString();
                float damageScale = 1.6f;
                Vector2 damageSize = MainGame.Gfx.gameFont.MeasureString(damageText) * damageScale;
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, damageText,
                    new Vector2(MainGame.screenWidth / 2 - damageSize.X / 2, 128 - rise), ThemeColors.Correct * alpha, 0f, Vector2.Zero, damageScale, SpriteEffects.None, 0f);
            }

            if (currentScore <= -100)
            {
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Snake);
            }
        }

        // Counts a bloom word that got auto-credited because the fight ended before the
        // player ever reached it, so the "Bloom words" stat still reflects it.
        public void CreditUnreachedBloom() => bloomWritten++;

        public void Reset()
        {
            kHeperBlockedIndexes.Clear();
            letterScoreSnapshot.Clear();
            lastCharCount = 0;
            lastWordCount = 1;
            lastCorrectWord = 0;
            extraScore = 0;
            lastMistakeCount = 0;
            bankedScore = 0;
            positiveScoreAtLastBoundary = 0;
            wordJustBoundaried = false;
            displayedScore = 0;
            bankFlashTime = -10;
            lastBankAmount = 0;
            specialFlashTime = -10;
            specialFlashWordIndex = -1;
        }

        public long GetShinyWritten() => shinyWritten;
        public long GetStoneWritten() => stoneWritten;
        public long GetBloomWritten() => bloomWritten;
    }
}
