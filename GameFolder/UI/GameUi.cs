using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using typatro.GameFolder.Models;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.Services;
using typatro.GameFolder.Upgrades;
using static typatro.GameFolder.GameLogic;

namespace typatro.GameFolder.UI
{
    public class GameUi
    {
        GameLogic gameLogic;
        public GameUi(GameLogic gameLogic) 
        {
            this.gameLogic = gameLogic;
        }

        public void Inventory(KeyboardState state = default)
        {
            MouseState mouseState = Mouse.GetState();
            gameLogic.inventoryUp = true;
            int columns = 4, rows = 8;
            int columnSpacing = (int)(MainGame.screenWidth / 4.5);
            int leftOffset = 40;

            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (column * rows + row >= 26) break;
                    SpriteFont font = gameLogic.enhancements.overHundred ? MainGame.Gfx.smallTextFont : MainGame.Gfx.gameFont;
                    MainGame.Gfx.spriteBatch.DrawString(font, (char)(column * rows + row + 'a') + ": " + gameLogic.enhancements.letters[column * rows + row], new Vector2(columnSpacing / 2 + column * columnSpacing - leftOffset, 70 + row * 40), ThemeColors.Text);
                    long change = gameLogic.enhancements.lettersChange[column * rows + row];
                    if (change != 0) MainGame.Gfx.spriteBatch.DrawString(font, (change < 0 ? "" : "+") + change, new Vector2(columnSpacing + column * columnSpacing + 25 - leftOffset, 70 + row * 40), change < 0 ? ThemeColors.Wrong : ThemeColors.Correct);
                }
            }

            int lineRow = -3, changeOffset = 150;
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"Shiny: {(int)(gameLogic.enhancements.shinyChance * 100)}%", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if (gameLogic.enhancements.shChange != 0) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"+{(int)(gameLogic.enhancements.shChange * 100)}%", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"Stone: {(int)(gameLogic.enhancements.stoneChance * 100)}%", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if (gameLogic.enhancements.stChange != 0) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"+{(int)(gameLogic.enhancements.stChange * 100)}%", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"Bloom: {(int)(gameLogic.enhancements.bloomChance * 100)}%", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if (gameLogic.enhancements.blChange != 0) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"+{(int)(gameLogic.enhancements.blChange * 100)}%", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);

            lineRow++;
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"Shiny mult: {gameLogic.enhancements.shinyScore.ToString("0.##")}x", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if (gameLogic.enhancements.shinyScoreChange != 0) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"+{gameLogic.enhancements.shinyScoreChange}x", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset + 26, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"Stone add: {gameLogic.enhancements.stoneScore}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if (gameLogic.enhancements.stoneScoreChange != 0) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"+{gameLogic.enhancements.stoneScoreChange}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);


            lineRow++;
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"Streak: {gameLogic.enhancements.streakMult.ToString("0.##")}x", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if (gameLogic.enhancements.wordChange != 0) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"+{gameLogic.enhancements.wordChange}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"Resist: {gameLogic.enhancements.damageResist}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if (gameLogic.enhancements.damageChange != 0) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"+{gameLogic.enhancements.damageChange}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"Block: {gameLogic.enhancements.mistakeBlock}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if (gameLogic.enhancements.mistakeChange != 0) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, $"+{gameLogic.enhancements.mistakeChange}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);

            Glyph[] glyphs = GlyphManager.GetGlyphs();
            if (glyphs.Length > 1)
            {
                if (state.IsKeyDown(Keys.Left) && gameLogic.inventoryMove && gameLogic.inventoryGlyphSelect > 1)
                {
                    gameLogic.inventoryGlyphSelect--;
                    gameLogic.inventoryMove = false;
                }
                if (state.IsKeyDown(Keys.Right) && gameLogic.inventoryMove && gameLogic.inventoryGlyphSelect < GlyphManager.GetGlyphCount() - 1)
                {
                    gameLogic.inventoryGlyphSelect++;
                    gameLogic.inventoryMove = false;
                }
                if (state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right)) gameLogic.inventoryMove = true;

                int borderOffset = 5, imageSize = 64, yOffset = 400, descOffset = 80, xColumnOffset = 80, xSideOffset = -30;
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(xColumnOffset * gameLogic.inventoryGlyphSelect - borderOffset + xSideOffset, yOffset - borderOffset, imageSize + borderOffset * 2, imageSize + borderOffset * 2), ThemeColors.Selected);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, GlyphManager.GetDescription(glyphs[gameLogic.inventoryGlyphSelect]), new Vector2(xColumnOffset + xSideOffset, yOffset + descOffset), ThemeColors.Text);
                columns = 0;
                foreach (Glyph glyph in glyphs)
                {
                    if (glyph != Glyph.NoGlyphsLeft)
                    {
                        Rectangle glyphRect = new Rectangle(xColumnOffset * columns + xSideOffset, yOffset, imageSize, imageSize);
                        if (glyphRect.Contains(mouseState.Position)) gameLogic.inventoryGlyphSelect = columns;
                        MainGame.Gfx.spriteBatch.Draw(GlyphManager.GetGlyphImage(glyph), glyphRect, ThemeColors.Foreground);
                    }
                    columns++;
                }
            }
        }

        public void TopBannerDisplay(bool onMap)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            gameLogic.inventoryUp = false;
            if (mouseState.LeftButton == ButtonState.Released)
            {
                gameLogic.inventoryMousePressed = false;
            }

            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(15, 15, MainGame.screenWidth - 30, 40), ThemeColors.Foreground);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(15, 15, MainGame.screenWidth - 30, 40), ThemeColors.Foreground);
            Vector2 textOffset = new Vector2(30, 20);

            //if (!tabPressed) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "tab -> inventory", new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString("tab -> inventory").X / 2, textOffset.Y), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, $"level:{gameLogic.level}/3", textOffset, ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, $"coins:{gameLogic.coins}", new Vector2(MainGame.screenWidth - MainGame.Gfx.gameFont.MeasureString($"coins:{gameLogic.coins}").X - textOffset.X, textOffset.Y), ThemeColors.Text);

            if (onMap && !keyboardState.IsKeyDown(Keys.Tab))
            {
                Rectangle inventoryRect = new Rectangle(MainGame.screenWidth - 70, 120, 45, 45);
                Color invIconColor = ThemeColors.NotSelected;
                if (inventoryRect.Contains(mouseState.Position) || (gameLogic.inventoryMousePressed && mouseState.LeftButton == ButtonState.Pressed))
                {
                    if (mouseState.LeftButton == ButtonState.Pressed && windowActive)
                    {
                        gameLogic.inventoryMousePressed = true;
                        Inventory(keyboardState);
                    }
                    invIconColor = ThemeColors.Selected;
                }
                if (!gameLogic.inventoryUp)
                {
                    MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, inventoryRect, invIconColor);
                    MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "i", new Vector2(inventoryRect.X + 20, inventoryRect.Y + 7), ThemeColors.Text);
                }

                Rectangle exitRect = new Rectangle(MainGame.screenWidth - 70, 65, 45, 45);
                Color exitIconColor = ThemeColors.NotSelected;
                if (exitRect.Contains(mouseState.Position))
                {
                    if (mouseState.LeftButton == ButtonState.Pressed && windowActive)
                    {
                        gameLogic.gameSaveData = SaveManager.LoadGame();

                        gameLogic.gameState = GameState.MENU;
                    }
                    exitIconColor = ThemeColors.Selected;
                }
                if (!gameLogic.inventoryUp)
                {
                    MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, exitRect, exitIconColor);
                    MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "<", new Vector2(exitRect.X + 15, exitRect.Y + 8), ThemeColors.Text);
                }
            }

        }

        public void HealthBar(ref Fight fight, long currentScore)
        {
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(40, 60, MainGame.screenWidth - 80, 35), ThemeColors.Background);
            int redBarLength = (int)((double)Math.Min(fight.scoreNeeded, fight.scoreNeeded - currentScore) / fight.scoreNeeded * (MainGame.screenWidth - 90));
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(45, 65, redBarLength, 25), ThemeColors.Selected);
            string score = $"{currentScore}/{fight.scoreNeeded}  -{fight.speed}/s";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, score, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.smallTextFont.MeasureString(score).X / 2, 68), ThemeColors.Text);
        }
    }
}
