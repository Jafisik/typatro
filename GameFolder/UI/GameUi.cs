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

        void DrawBoxBg(Rectangle rect)
        {
            Color bg = ThemeColors.Background;
            bg.A = 160;
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, rect, bg);
        }

        void DrawBorder(Rectangle rect, Color color, int thickness = 2)
        {
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        // bg drawn first so text can be painted on top of it; border drawn separately
        // afterwards wherever the caller needs it to sit above already-drawn text.
        void DrawOutline(Rectangle rect, Color color, int thickness = 2)
        {
            DrawBoxBg(rect);
            DrawBorder(rect, color, thickness);
        }

        public void Inventory(KeyboardState state = default)
        {
            MouseState mouseState = Mouse.GetState();
            gameLogic.inventoryUp = true;
            int columns = 3, rows = 9;

            // Whole screen split top/bottom - 3/4 for letters+enhancements, 1/4 for glyphs -
            // with a real gap between every panel instead of them nearly touching.
            int margin = 35, gap = 30, rowGap = 15;
            int contentTop = 65, contentBottom = MainGame.screenHeight - 30;
            int contentLeft = margin, contentRight = MainGame.screenWidth - margin;
            int totalHeight = contentBottom - contentTop;
            int bottomHeight = (int)(totalHeight * 0.36f);
            int topHeight = totalHeight - bottomHeight - rowGap;

            int sidePanelWidth = 280;
            Rectangle lettersPanel = new Rectangle(contentLeft, contentTop, contentRight - contentLeft - gap - sidePanelWidth, topHeight);
            Rectangle sidePanel = new Rectangle(lettersPanel.Right + gap, contentTop, sidePanelWidth, topHeight);
            Rectangle glyphPanel = new Rectangle(contentLeft, lettersPanel.Bottom + rowGap, contentRight - contentLeft, bottomHeight);

            Color panelColor = Color.Lerp(ThemeColors.Background, ThemeColors.Text, 0.15f);
            panelColor.A = 235;
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, lettersPanel, panelColor);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, sidePanel, panelColor);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, glyphPanel, panelColor);

            // Section labels eat a bit of vertical space at the top of each panel - the
            // content below (letter grid rows, enhancement text scale) is shrunk slightly to
            // still fit under them.
            int labelH = 42;
            float labelScale = 1.8f;
            void DrawPanelLabel(string text, Rectangle panel, int xOffset = 22, int yOffset = 11)
            {
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, text, new Vector2(panel.X + xOffset, panel.Y + yOffset),
                    ThemeColors.Text, 0f, Vector2.Zero, labelScale, SpriteEffects.None, 0f);
            }
            DrawPanelLabel("Letter scores", lettersPanel);
            DrawPanelLabel("Enhancements", sidePanel, xOffset: 15);
            DrawPanelLabel("Glyphs", glyphPanel, yOffset: 4);

            int colWidth = lettersPanel.Width / columns;
            int rowSpacing = 36;
            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (column * rows + row >= 26) break;
                    SpriteFont font = gameLogic.enhancements.overHundred ? MainGame.Gfx.smallTextFont : MainGame.Gfx.gameFont;
                    int colX = lettersPanel.X + 30 + column * colWidth;
                    int rowY = lettersPanel.Y + labelH + 14 + row * rowSpacing;
                    DrawBoxBg(new Rectangle(lettersPanel.X + 18 + column * colWidth, lettersPanel.Y + labelH + 12 + row * rowSpacing, colWidth - 30, 34));
                    MainGame.Gfx.spriteBatch.DrawString(font, (char)(column * rows + row + 'a') + ": " + gameLogic.enhancements.letters[column * rows + row], new Vector2(colX, rowY), ThemeColors.Text, 0f, Vector2.Zero, 0.95f, SpriteEffects.None, 0f);
                    long change = gameLogic.enhancements.lettersChange[column * rows + row];
                    if (change != 0) MainGame.Gfx.spriteBatch.DrawString(font, (change < 0 ? "" : "+") + change, new Vector2(colX + 150, rowY), change < 0 ? ThemeColors.Wrong : ThemeColors.Correct, 0f, Vector2.Zero, 0.95f, SpriteEffects.None, 0f);
                }
            }

            // Aligned to the same top edge the letter columns start from, and bigger than
            // before so it doesn't read as an afterthought crammed into the leftover space.
            int enhX = sidePanel.X + 20, changeOffset = 155;
            float enhScale = 1.05f;
            float enhLineH = MainGame.Gfx.smallTextFont.LineSpacing * enhScale + 6;
            float enhY = sidePanel.Y + labelH + 20;

            void DrawEnh(string label, string changeText = null)
            {
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, label, new Vector2(enhX, enhY),
                    ThemeColors.Text, 0f, Vector2.Zero, enhScale, SpriteEffects.None, 0f);
                if (changeText != null)
                    MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, changeText, new Vector2(enhX + changeOffset, enhY),
                        ThemeColors.Correct, 0f, Vector2.Zero, enhScale, SpriteEffects.None, 0f);
                enhY += enhLineH;
            }

            // Each special word type (shiny/stone/bloom) gets its own box around its chance +
            // add lines, instead of one box for all three chances and another for all three
            // adds - so each word type reads as its own self-contained group.
            int enhBoxWidth = sidePanel.Right - 15 - (enhX - 10);
            void DrawEnhGroup(int lineCount, Action drawLines)
            {
                // Line count is known up front, so the box size is too - the bg can be painted
                // before the text (and the border after), instead of the bg landing on top of
                // already-drawn text and darkening it.
                Rectangle box = new Rectangle(enhX - 10, (int)enhY - 5, enhBoxWidth, (int)(enhLineH * lineCount) + 5);
                DrawBoxBg(box);
                drawLines();
                DrawBorder(box, ThemeColors.NotSelected);
                enhY += enhLineH * 0.35f;
            }

            DrawEnhGroup(2, () =>
            {
                DrawEnh($"Shiny: {(int)(gameLogic.enhancements.shinyChance * 100)}%", gameLogic.enhancements.shChange != 0 ? $"+{(int)(gameLogic.enhancements.shChange * 100)}%" : null);
                DrawEnh($"Shiny mult: {gameLogic.enhancements.shinyScore.ToString("0.##")}x", gameLogic.enhancements.shinyScoreChange != 0 ? $"+{gameLogic.enhancements.shinyScoreChange}x" : null);
            });
            DrawEnhGroup(2, () =>
            {
                DrawEnh($"Stone: {(int)(gameLogic.enhancements.stoneChance * 100)}%", gameLogic.enhancements.stChange != 0 ? $"+{(int)(gameLogic.enhancements.stChange * 100)}%" : null);
                DrawEnh($"Stone add: {gameLogic.enhancements.stoneScore}", gameLogic.enhancements.stoneScoreChange != 0 ? $"+{gameLogic.enhancements.stoneScoreChange}" : null);
            });
            DrawEnhGroup(2, () =>
            {
                DrawEnh($"Bloom: {(int)(gameLogic.enhancements.bloomChance * 100)}%", gameLogic.enhancements.blChange != 0 ? $"+{(int)(gameLogic.enhancements.blChange * 100)}%" : null);
                DrawEnh($"Bloom add: {gameLogic.enhancements.bloomScore}", gameLogic.enhancements.bloomScoreChange != 0 ? $"+{gameLogic.enhancements.bloomScoreChange}" : null);
            });
            enhY += enhLineH * 0.25f;
            DrawEnhGroup(3, () =>
            {
                DrawEnh($"Streak: {gameLogic.enhancements.streakMult.ToString("0.##")}x", gameLogic.enhancements.wordChange != 0 ? $"+{gameLogic.enhancements.wordChange}" : null);
                DrawEnh($"Resist: {gameLogic.enhancements.damageResist}", gameLogic.enhancements.damageChange != 0 ? $"+{gameLogic.enhancements.damageChange}" : null);
                DrawEnh($"Block: {gameLogic.enhancements.mistakeBlock}", gameLogic.enhancements.mistakeChange != 0 ? $"+{gameLogic.enhancements.mistakeChange}" : null);
            });

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

                int borderOffset = 5, imageSize = 60, xColumnOffset = 76;
                int descX = glyphPanel.X + 20;
                int iconX = glyphPanel.X - 50;
                int yOffset = glyphPanel.Y + labelH + 18;
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(xColumnOffset * gameLogic.inventoryGlyphSelect - borderOffset + iconX, yOffset - borderOffset, imageSize + borderOffset * 2, imageSize + borderOffset * 2), ThemeColors.Selected);

                // Icons are small and sit in their own row up top, so the description - the
                // part that was actually running off the bottom of the screen - gets most of
                // the panel's remaining height, wrapped and scaled down to fit inside it.
                float descScale = 0.95f;
                string description = GameLogic.WrapText(MainGame.Gfx.smallTextFont, GlyphManager.GetDescription(glyphs[gameLogic.inventoryGlyphSelect]), glyphPanel.Width - 60);
                float descLineH = MainGame.Gfx.smallTextFont.LineSpacing * descScale;
                float descY = yOffset + imageSize + 12;
                foreach (string line in description.Split('\n'))
                {
                    MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, line, new Vector2(descX, descY),
                        ThemeColors.Text, 0f, Vector2.Zero, descScale, SpriteEffects.None, 0f);
                    descY += descLineH;
                }
                columns = 0;
                foreach (Glyph glyph in glyphs)
                {
                    if (glyph != Glyph.NoGlyphsLeft)
                    {
                        Rectangle glyphRect = new Rectangle(xColumnOffset * columns + iconX, yOffset, imageSize, imageSize);
                        if (glyphRect.Contains(mouseState.Position)) gameLogic.inventoryGlyphSelect = columns;
                        MainGame.Gfx.spriteBatch.Draw(GlyphManager.GetGlyphImage(glyph), glyphRect, ThemeColors.Foreground);
                    }
                    columns++;
                }
            }
        }

        public void TopBannerDisplay(bool onMap, bool showCoins = true)
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
            if (showCoins)
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

        public void HealthBar(ref Fight fight, long currentScore, double displayedScore)
        {
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(40, 60, MainGame.screenWidth - 80, 35), ThemeColors.Background);
            int redBarLength = (int)(Math.Min(fight.scoreNeeded, fight.scoreNeeded - displayedScore) / fight.scoreNeeded * (MainGame.screenWidth - 90));
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(45, 65, redBarLength, 25), ThemeColors.Selected);
            string score = $"{currentScore}/{fight.scoreNeeded}  -{fight.speed}/s";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, score, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.smallTextFont.MeasureString(score).X / 2, 68), ThemeColors.Text);
        }
    }
}
