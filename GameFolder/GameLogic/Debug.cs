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

            DrawDebugTutorialToggles(mouseState);
            DrawDebugLevelButtons(mouseState);

            // Menu button
            Rectangle menuBtn = new Rectangle(30, MainGame.screenHeight - 60, 110, 40);
            bool menuHovered = menuBtn.Contains(mouseState.Position);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, menuBtn, menuHovered ? ThemeColors.Selected : ThemeColors.Foreground);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "< menu",
                new Vector2(menuBtn.X + 10, menuBtn.Y + 7), ThemeColors.Text);

            if (menuHovered && mouseState.LeftButton == ButtonState.Pressed && !debugMousePressed)
            {
                debugMousePressed = true;
                gameState = GameState.MENU;
                return;
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

        // Stacked vertically on the right side of the glyphs page - toggle to re-lock/unlock
        // each tutorial so they can be replayed without editing unlocks.json by hand.
        private void DrawDebugTutorialToggles(MouseState mouseState)
        {
            var tutorials = UnlockManager.TutorialTypes;

            int cols = 2, cellW = 150, cellH = 55, gap = 8;
            int startX = MainGame.screenWidth - cols * cellW - gap - 30, startY = 65;

            for (int i = 0; i < tutorials.Length; i++)
            {
                UnlockManager.UnlockType tutorial = tutorials[i];
                int col = i % cols, row = i / cols;
                Rectangle cell = new Rectangle(startX + col * (cellW + gap), startY + row * (cellH + gap), cellW, cellH);

                bool unlocked = UnlockManager.IsUnlockUnlocked(tutorial);
                bool hovered = cell.Contains(mouseState.Position);
                Color bg = unlocked ? ThemeColors.Selected : (hovered ? ThemeColors.Foreground : ThemeColors.NotSelected);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, cell, bg);

                string name = tutorial.ToString();
                Vector2 nameSize = MainGame.Gfx.smallTextFont.MeasureString(name);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, name,
                    new Vector2(cell.X + cell.Width / 2 - nameSize.X / 2, cell.Y + 10), ThemeColors.Text);

                string status = unlocked ? "seen" : "not seen";
                Vector2 statusSize = MainGame.Gfx.smallTextFont.MeasureString(status);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, status,
                    new Vector2(cell.X + cell.Width / 2 - statusSize.X / 2, cell.Y + 34), ThemeColors.Text);

                if (hovered && mouseState.LeftButton == ButtonState.Pressed && !debugMousePressed)
                {
                    debugMousePressed = true;
                    if (unlocked) UnlockManager.LockUnlock(tutorial);
                    else UnlockManager.UnlockUnlock(tutorial);
                }
            }

            int rows = (tutorials.Length + cols - 1) / cols;
            Rectangle resetAllBtn = new Rectangle(startX, startY + rows * (cellH + gap), cols * cellW + gap, 36);
            bool resetAllHovered = resetAllBtn.Contains(mouseState.Position);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, resetAllBtn, resetAllHovered ? ThemeColors.Selected : ThemeColors.Foreground);
            string resetAllText = "reset ALL tutorials";
            Vector2 resetAllSize = MainGame.Gfx.smallTextFont.MeasureString(resetAllText);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, resetAllText,
                new Vector2(resetAllBtn.X + resetAllBtn.Width / 2 - resetAllSize.X / 2, resetAllBtn.Y + resetAllBtn.Height / 2 - resetAllSize.Y / 2), ThemeColors.Text);

            if (resetAllHovered && mouseState.LeftButton == ButtonState.Pressed && !debugMousePressed)
            {
                debugMousePressed = true;
                UnlockManager.ResetAllTutorials();
            }
        }

        // Jumps straight into a fresh run's map at the given level, so level-specific visuals
        // (tint) and difficulty scaling can be tested without playing through levels 1..n-1.
        private void DrawDebugLevelButtons(MouseState mouseState)
        {
            int count = 3, btnW = 140, btnH = 40, gap = 20;
            int totalWidth = count * btnW + (count - 1) * gap;
            int startX = (MainGame.screenWidth - totalWidth) / 2;
            int y = MainGame.screenHeight - 60;

            for (int i = 0; i < count; i++)
            {
                int startLevel = i + 1;
                Rectangle btn = new Rectangle(startX + i * (btnW + gap), y, btnW, btnH);
                bool hovered = btn.Contains(mouseState.Position);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, btn, hovered ? ThemeColors.Selected : ThemeColors.Foreground);
                string text = $"start lvl {startLevel}";
                Vector2 textSize = MainGame.Gfx.smallTextFont.MeasureString(text);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, text,
                    new Vector2(btn.X + btn.Width / 2 - textSize.X / 2, btn.Y + btn.Height / 2 - textSize.Y / 2), ThemeColors.Text);

                if (hovered && mouseState.LeftButton == ButtonState.Pressed && !debugMousePressed)
                {
                    debugMousePressed = true;
                    StartDebugRun(startLevel);
                }
            }
        }

        private void StartDebugRun(int startLevel)
        {
            Reset();
            seed = unseededRandom.Next();
            level = startLevel;
            difficulty = 0;
            selectedRune = 0;
            SetContext(-1, 0);
            map = new Map();
            map.GenerateNodes();
            selectedNode = map.GetFirstNode();
            lastSelectedNode = selectedNode;
            enhancements = new Enhancements();
            shop = new Shop(enhancements);
            treasure = new Treasure(enhancements);
            curseRoom = new CurseRoom(enhancements);
            coins = startCoins;
            GlyphManager.RemoveAllGlyphs();
            GlyphManager.Add(Glyph.NoGlyphsLeft);
            visitedNodes = new List<int[]>();
            mistake = false;
            deadCounted = false;
            mousePressed = true;
            tutorial = false;
            isDebugFight = false;
            gameState = GameState.LOADGAME;
        }

        private void DrawDebugEnemies(MouseState mouseState)
        {
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "DEBUG - enemies", new Vector2(30, 18), ThemeColors.Text);

            // Handle the back button's click first so it always wins clicks in its area,
            // even where the enemy grid below happens to overlap it.
            Rectangle backBtn = new Rectangle(30, MainGame.screenHeight - 60, 110, 40);
            bool backHovered = backBtn.Contains(mouseState.Position);
            bool backClicked = backHovered && mouseState.LeftButton == ButtonState.Pressed && !debugMousePressed;
            if (backClicked)
            {
                debugMousePressed = true;
                debugPage = 0;
            }

            var allEnemies = new List<(Enemy enemy, string category)>();
            foreach (var e in EnemyManager.Normal) allEnemies.Add((e, "normal"));
            foreach (var e in EnemyManager.Elite)  allEnemies.Add((e, "elite"));
            foreach (var e in EnemyManager.Boss)   allEnemies.Add((e, "boss"));

            int cols = 7, cellW = 105, cellH = 95, startX = 30, startY = 70;

            for (int i = 0; i < allEnemies.Count; i++)
            {
                var (enemy, category) = allEnemies[i];
                int col = i % cols;
                int row = i / cols;
                Rectangle cell = new Rectangle(startX + col * cellW, startY + row * cellH, cellW - 8, cellH - 8);

                bool hovered = !backHovered && cell.Contains(mouseState.Position);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, cell, hovered ? ThemeColors.Selected : ThemeColors.Foreground);

                if (enemy.Texture != null)
                {
                    int fw = enemy.Texture.Width / 4;
                    int fh = enemy.Texture.Height;
                    int spriteSize = 38;
                    Rectangle spriteRect = new Rectangle(cell.X + (cell.Width - spriteSize) / 2, cell.Y + 4, spriteSize, spriteSize);
                    MainGame.Gfx.spriteBatch.Draw(enemy.Texture, spriteRect, new Rectangle(0, 0, fw, fh), Color.White);
                }

                string name = enemy.Description.Split(':')[0];
                Vector2 nameSize = MainGame.Gfx.smallTextFont.MeasureString(name);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, name,
                    new Vector2(cell.X + cell.Width / 2 - nameSize.X / 2, cell.Y + 46), ThemeColors.Text);

                Color badgeColor = category == "boss" ? ThemeColors.Wrong : category == "elite" ? ThemeColors.Selected : ThemeColors.Correct;
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, category, new Vector2(cell.X + 3, cell.Y + 2), badgeColor);

                if (hovered && mouseState.LeftButton == ButtonState.Pressed && !debugMousePressed)
                {
                    debugMousePressed = true;
                    StartDebugFight(enemy);
                    return;
                }
            }

            // Back button (drawn on top, after the grid, so it's never visually hidden)
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, backBtn, backHovered ? ThemeColors.Selected : ThemeColors.Foreground);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "< back",
                new Vector2(backBtn.X + 10, backBtn.Y + 7), ThemeColors.Text);
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

            fight = Fight.Create(1, 1, 1, 0);
            currentEnemy = enemy;
            EnemyManager.SetActive(enemy.Type);
            selectedNode = new MapNode(null, NodeType.FIGHT, Vector2.Zero, 0, 0);
            lastSelectedNode = selectedNode;
            neededText = RandomTextGenerate(fight.words);
            Writer.writtenText.Clear();
            Writer.diffIndexes.Clear();
            roomSelected = true;
            startedTyping = false;
            canStartFight = true;
            isDebugFight = true;
            enemyIntroActive = false;

            gameState = GameState.LOADGAME;
        }
    }
}
