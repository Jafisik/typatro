using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using typatro.GameFolder.Logic;
using typatro.GameFolder.Models;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.Services;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder
{
    public partial class GameLogic
    {
        bool debugMousePressed;
        int debugPage = 0;

        private void DrawDebugScreen()
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape))
            {
                gameState = GameState.MENU;
                return;
            }

            if (debugPage == 0) DrawDebugGlyphs(mouseState);
            else                DrawDebugEnemies(mouseState);

            if (mouseState.LeftButton == ButtonState.Released)
                debugMousePressed = false;
        }

        private void DrawDebugGlyphs(MouseState mouseState)
        {
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "DEBUG - glyphs", new Vector2(30, 18), ThemeColors.Text);

            var allGlyphs = (Glyph[])Enum.GetValues(typeof(Glyph));
            int cols = 6, cellSize = 72, gap = 8, startX = 30, startY = 65;

            for (int i = 0; i < allGlyphs.Length; i++)
            {
                Glyph glyph = allGlyphs[i];
                if (glyph == Glyph.NoGlyphsLeft) continue;

                int idx = i - 1; // skip NoGlyphsLeft
                int col = idx % cols;
                int row = idx / cols;
                Rectangle cell = new Rectangle(startX + col * (cellSize + gap), startY + row * (cellSize + gap), cellSize, cellSize);

                bool active = GlyphManager.IsActive(glyph);
                bool hovered = cell.Contains(mouseState.Position);
                Color bg = active ? ThemeColors.Selected : (hovered ? ThemeColors.Foreground : ThemeColors.NotSelected);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, cell, bg);

                Texture2D img = GlyphManager.GetGlyphImage(glyph);
                if (img != null)
                    MainGame.Gfx.spriteBatch.Draw(img, new Rectangle(cell.X + 4, cell.Y + 4, cellSize - 8, cellSize - 8), ThemeColors.Foreground);

                if (hovered && mouseState.LeftButton == ButtonState.Pressed && !debugMousePressed)
                {
                    debugMousePressed = true;
                    if (active) GlyphManager.Remove(glyph);
                    else        GlyphManager.Add(glyph);
                }
            }

            // Next button
            Rectangle nextBtn = new Rectangle(MainGame.screenWidth - 160, MainGame.screenHeight - 60, 130, 40);
            bool nextHovered = nextBtn.Contains(mouseState.Position);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, nextBtn, nextHovered ? ThemeColors.Selected : ThemeColors.Foreground);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "next >",
                new Vector2(nextBtn.X + 10, nextBtn.Y + 7), ThemeColors.Text);

            if (nextHovered && mouseState.LeftButton == ButtonState.Pressed && !debugMousePressed)
            {
                debugMousePressed = true;
                debugPage = 1;
            }
        }

        private void DrawDebugEnemies(MouseState mouseState)
        {
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "DEBUG - enemies", new Vector2(30, 18), ThemeColors.Text);

            var allEnemies = new List<(Enemy enemy, string category)>();
            foreach (var e in EnemyManager.Normal) allEnemies.Add((e, "normal"));
            foreach (var e in EnemyManager.Elite)  allEnemies.Add((e, "elite"));
            foreach (var e in EnemyManager.Boss)   allEnemies.Add((e, "boss"));

            int cols = 5, cellW = 145, cellH = 130, startX = 30, startY = 70;

            for (int i = 0; i < allEnemies.Count; i++)
            {
                var (enemy, category) = allEnemies[i];
                int col = i % cols;
                int row = i / cols;
                Rectangle cell = new Rectangle(startX + col * cellW, startY + row * cellH, cellW - 8, cellH - 8);

                bool hovered = cell.Contains(mouseState.Position);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, cell, hovered ? ThemeColors.Selected : ThemeColors.Foreground);

                if (enemy.Texture != null)
                {
                    int fw = enemy.Texture.Width / 4;
                    int fh = enemy.Texture.Height;
                    int spriteSize = 56;
                    Rectangle spriteRect = new Rectangle(cell.X + (cell.Width - spriteSize) / 2, cell.Y + 6, spriteSize, spriteSize);
                    MainGame.Gfx.spriteBatch.Draw(enemy.Texture, spriteRect, new Rectangle(0, 0, fw, fh), Color.White);
                }

                string name = enemy.Description.Split(':')[0];
                Vector2 nameSize = MainGame.Gfx.smallTextFont.MeasureString(name);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, name,
                    new Vector2(cell.X + cell.Width / 2 - nameSize.X / 2, cell.Y + 68), ThemeColors.Text);

                Color badgeColor = category == "boss" ? ThemeColors.Wrong : category == "elite" ? ThemeColors.Selected : ThemeColors.Correct;
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, category, new Vector2(cell.X + 4, cell.Y + 4), badgeColor);

                if (hovered && mouseState.LeftButton == ButtonState.Pressed && !debugMousePressed)
                {
                    debugMousePressed = true;
                    StartDebugFight(enemy);
                    return;
                }
            }

            // Back button
            Rectangle backBtn = new Rectangle(30, MainGame.screenHeight - 60, 110, 40);
            bool backHovered = backBtn.Contains(mouseState.Position);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, backBtn, backHovered ? ThemeColors.Selected : ThemeColors.Foreground);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "< back",
                new Vector2(backBtn.X + 10, backBtn.Y + 7), ThemeColors.Text);

            if (backHovered && mouseState.LeftButton == ButtonState.Pressed && !debugMousePressed)
            {
                debugMousePressed = true;
                debugPage = 0;
            }
        }

        private void StartDebugFight(Enemy enemy)
        {
            Reset();

            enhancements = new Enhancements();
            shop = new Shop(enhancements);
            treasure = new Treasure(enhancements);
            curseRoom = new CurseRoom(enhancements);
            coins = 30;
            level = 1;
            visitedNodes = new List<int[]>();
            mistake = false;

            fight = Fight.Create(1, 1, 1);
            currentEnemy = enemy;
            EnemyManager.SetActive(enemy.Type);
            selectedNode = new MapNode(null, NodeType.FIGHT, Vector2.Zero, 0, 0);
            lastSelectedNode = selectedNode;
            neededText = RandomTextGenerate(fight.words);
            Writer.writtenText.Clear();
            roomSelected = true;
            startedTyping = false;
            canStartFight = true;
            isDebugFight = true;

            gameState = GameState.LOADGAME;
        }
    }
}
