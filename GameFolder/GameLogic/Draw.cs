using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using typatro.GameFolder.Services;
using typatro.GameFolder.UI;
using static typatro.GameFolder.Services.UnlockManager;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder
{
    public partial class GameLogic
    {
        float bgRotation = 0f, scaleDelta;
        bool scaleSwap;

        public void Draw(GraphicsDeviceManager graphicsDevice)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            bgRotation -= 0.002f;
            if (!scaleSwap)
            {
                scaleDelta += 0.0002f;
                if (scaleDelta >= 0.2f) scaleSwap = true;
            }
            else
            {
                scaleDelta -= 0.0002f;
                if (scaleDelta <= 0f) scaleSwap = false;
            }

            Vector2 centerScreen = new Vector2(MainGame.screenWidth / 2f, MainGame.screenHeight / 2f);
            Vector2 bgOrigin = new Vector2(MainGame.Gfx.bg.Width / 2f, MainGame.Gfx.bg.Height / 2f);

            MainGame.Gfx.spriteBatch.GraphicsDevice.Clear(ThemeColors.Background);
            Color bgImageColor = ThemeColors.Background;
            bgImageColor.A = 150;
            float scale = 1f;
            if (SaveManager.size == 1) scale = 1.4f;
            if (SaveManager.size == 2) scale = 1.6f;

            MainGame.Gfx.spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: Microsoft.Xna.Framework.Graphics.SamplerState.LinearClamp);
            int blurOffset = 4;
            Color blurColor = bgImageColor;
            blurColor.A = 60;
            foreach (var offset in new[] { new Vector2(-blurOffset, 0), new Vector2(blurOffset, 0), new Vector2(0, -blurOffset), new Vector2(0, blurOffset) })
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.bg, centerScreen + offset, null, blurColor, bgRotation, bgOrigin, scale + scaleDelta, SpriteEffects.None, 0f);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.bg, centerScreen, null, bgImageColor, bgRotation, bgOrigin, scale + scaleDelta, SpriteEffects.None, 0f);
            MainGame.Gfx.spriteBatch.End();

            MainGame.Gfx.spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp);

            int lineWidth = 15;
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(0, 0, MainGame.screenWidth, lineWidth), ThemeColors.Foreground);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(0, 0, lineWidth, MainGame.screenHeight), ThemeColors.Foreground);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(0, MainGame.screenHeight - lineWidth, MainGame.screenWidth, lineWidth), ThemeColors.Foreground);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(MainGame.screenWidth - lineWidth, 0, lineWidth, MainGame.screenHeight), ThemeColors.Foreground);

            graphicsDevice.IsFullScreen = SaveManager.fullscreen == 1;

            if (gameState == GameState.MENU)
            {
                gameState = (GameState)menu.DrawMainMenu(gameSaveData != null, gameFinished);
                if (keyboardState.IsKeyUp(Keys.Enter))
                    gameFinished = false;
                if (gameState == GameState.LOADGAME) LoadGame();
                if (gameState == GameState.NEWGAME) NewGame();
                if (gameState == GameState.DEBUG)
                {
                    GlyphManager.RemoveAllGlyphs();
                    GlyphManager.Add(Glyph.NoGlyphsLeft);
                    debugPage = 0;
                }
                firstEnter = true;
                roomSelected = false;
                canStartFight = false;
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Uruz0);
            }
            else if (gameState == GameState.RUNES)
            {
                KeyboardState state = Keyboard.GetState();
                characterSelect.CharacterChoose(ref enhancements);
                if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.CharacterTutorial))
                {
                    if (TutorialManager.Draw(state, mouseState))
                        UnlockManager.UnlockUnlock(UnlockType.CharacterTutorial);
                }
            }
            else if (gameState == GameState.LOADGAME)
            {
                Play();
            }
            else if (gameState == GameState.OPTIONS)
            {
                if (keyboardState.IsKeyDown(Keys.Escape))
                {
                    gameState = GameState.MENU;
                    SaveManager.SaveSettings(SaveManager.theme, SaveManager.volume, SaveManager.size, SaveManager.fullscreen);
                }
                ThemeColors.Apply(SaveManager.theme);
                if (menu.DrawOptionsMenu()) gameState = GameState.MENU;
            }
            else if (gameState == GameState.DEBUG)
            {
                DrawDebugScreen();
            }
            else if (gameState == GameState.EXIT)
            {
                SteamManager.Shutdown();
                Environment.Exit(0);
            }

            if (keyboardState.GetPressedKeyCount() > 0 && !(keyboardState.GetPressedKeyCount() == 1 && keyboardState.IsKeyDown(Keys.Tab)))
            {
                keyboardUsed = true;
            }

            Texture2D mouseTexture = MainGame.Gfx.mouse1;
            if (mouseState.LeftButton == ButtonState.Pressed) mouseTexture = MainGame.Gfx.mouse2;
            if (!keyboardUsed)
            {
                MainGame.Gfx.spriteBatch.Draw(mouseTexture, new Vector2(mouseState.X, mouseState.Y), null, ThemeColors.Mouse, 0f, new Vector2(16, 16), 1.6f, SpriteEffects.None, 0f);
            }
            else
            {
                if (mousePosition != mouseState.Position)
                {
                    keyboardUsed = false;
                }
            }

            mousePosition = mouseState.Position;
            MainGame.Gfx.spriteBatch.End();
        }

        private void DrawRunStats(int col1X, int col2X)
        {
            var letters = enhancements.HighestLetter();
            int accuracy = lettersWritten > 0 ? (int)((1.0 - (double)mistakesWritten / lettersWritten) * 100) : 100;
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont,
                $"Words written: {wordsWritten}\nLetters written:{lettersWritten}\nMistakes: {mistakesWritten}\nAccuracy: {accuracy}%",
                new Vector2(col1X, 150), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont,
                $"Most upgraded letter: {letters.bestLetter}:  {letters.bestLetterNum}\nTotal score: {totalScore}\nMax score: {maxScore}",
                new Vector2(col2X, 150), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont,
                $"Shiny words: {scoreCalculator.GetShinyWritten()}\nStone words: {scoreCalculator.GetStoneWritten()}\nBloom words: {scoreCalculator.GetBloomWritten()}",
                new Vector2(col1X, 300), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont,
                $"Highest streak: {highestStreak}\nCoins gained: {coinsGained}\nMax coins: {maxCoins}",
                new Vector2(col2X, 300), ThemeColors.Text);
        }
    }
}