using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel.DataAnnotations;
using typatro.GameFolder.Services;
using typatro.GameFolder.Upgrades;
using static typatro.GameFolder.GameLogic;
using static typatro.GameFolder.Services.UnlockManager;
namespace typatro.GameFolder.UI
{
    public class CharacterSelect
    {
        private GameLogic gameLogic;
        bool runeMove, diffMove;

        public CharacterSelect(GameLogic gameLogic) 
        {
            this.gameLogic = gameLogic;
            TutorialManager.Start(TutorialManager.CharacterSteps());
        }
        public void CharacterChoose(ref Enhancements enhancements)
        {
            KeyboardState state = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            int maxRunes = Enum.GetValues(typeof(Runes.Runes)).Length;
            Rectangle backRect = new Rectangle(50, 50, 50, 50);
            Color backSelected = ThemeColors.NotSelected;
            if (backRect.Contains(mouseState.Position)) backSelected = ThemeColors.Selected;
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, backRect, backSelected);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "<", new Vector2(67, 57), ThemeColors.Text);
            if (state.IsKeyDown(Keys.Escape) || !gameLogic.mousePressed && backRect.Contains(mouseState.Position) &&
                mouseState.LeftButton == ButtonState.Pressed && windowActive && UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.CharacterTutorial))
            {
                gameLogic.gameState = GameState.MENU;
                gameLogic.gameSaveData = SaveManager.LoadGame();
            }
            if (gameLogic.canStartFight && state.IsKeyDown(Keys.Enter) && UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.CharacterTutorial) && gameLogic.tutorial)
            {
                if (runeUnlocks.TryGetValue(((Runes.Runes)gameLogic.selectedRune, gameLogic.difficulty), out UnlockType unlock))
                {
                    if (UnlockManager.IsUnlockUnlocked(unlock)){
                        NewGameChoiceUpdate(ref enhancements);
                        gameLogic.tutorial = false;
                        gameLogic.gameState = GameState.LOADGAME;
                    }
                }
            }
            else if (state.IsKeyUp(Keys.Enter) && mouseState.LeftButton == ButtonState.Released)
            {
                gameLogic.canStartFight = true;
                gameLogic.tutorial = true;
            }

            if (mouseState.LeftButton == ButtonState.Released) gameLogic.mousePressed = false;

            int rectWidth = MainGame.screenWidth / 3, rectHeight = MainGame.screenHeight / 2;

            // Precompute hover for center darkening (includes arrow areas)
            bool leftHover = false, rightHover = false;
            if (gameLogic.selectedRune != 0)
            {
                Rectangle lr = new Rectangle(MainGame.screenWidth / 5 - rectWidth / 4 + SaveManager.size * 30, (int)(MainGame.screenHeight / 2.5f) - rectHeight / 4, rectWidth / 2, rectHeight / 2);
                Rectangle lArrow = new Rectangle(lr.X - 60, lr.Y, 60, lr.Height);
                Rectangle lExtra = new Rectangle(lr.X + lr.Width, lr.Y, 50, lr.Height);
                leftHover = !GameLogic.keyboardUsed && (lr.Contains(mouseState.Position) || lExtra.Contains(mouseState.Position) || lArrow.Contains(mouseState.Position));
            }
            if (gameLogic.selectedRune != maxRunes - 1)
            {
                Rectangle rr = new Rectangle(MainGame.screenWidth - MainGame.screenWidth / 5 - rectWidth / 4 - SaveManager.size * 30, (int)(MainGame.screenHeight / 2.5f) - rectHeight / 4, rectWidth / 2, rectHeight / 2);
                Rectangle rArrow = new Rectangle(rr.X + rr.Width, rr.Y, 60, rr.Height);
                Rectangle rExtra = new Rectangle(rr.X - 50, rr.Y, 50, rr.Height);
                rightHover = !GameLogic.keyboardUsed && (rr.Contains(mouseState.Position) || rExtra.Contains(mouseState.Position) || rArrow.Contains(mouseState.Position));
            }

            if (runeMove)
            {
                if (state.IsKeyDown(Keys.Left))
                {
                    if (!diffMove && gameLogic.selectedRune != 0)
                    {
                        gameLogic.selectedRune--;
                        gameLogic.difficulty = 0;
                    }
                    else if (diffMove && gameLogic.difficulty != 0)
                    {
                        gameLogic.difficulty--;
                    }

                    runeMove = false;
                }
                if (state.IsKeyDown(Keys.Right))
                {
                    if (!diffMove && gameLogic.selectedRune != maxRunes - 1)
                    {
                        gameLogic.selectedRune++;
                        gameLogic.difficulty = 0;
                    }
                    else if (diffMove && gameLogic.difficulty != 5)
                    {
                        gameLogic.difficulty++;
                    }
                    runeMove = false;
                }
            }
            if (state.IsKeyDown(Keys.Down)) diffMove = true;
            else if (state.IsKeyDown(Keys.Up)) diffMove = false;

            if (!runeMove)
            {
                if (state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right)) runeMove = true;
            }

            if (gameLogic.selectedRune != 0)
            {
                Rectangle runeSelect = new Rectangle(MainGame.screenWidth / 5 - rectWidth / 4 + SaveManager.size * 30, (int)(MainGame.screenHeight / 2.5f) - rectHeight / 4, rectWidth / 2, rectHeight / 2);
                Rectangle extraSpace = new Rectangle(runeSelect.X + runeSelect.Width, runeSelect.Y, 50, runeSelect.Height);
                Rectangle arrowSpace = new Rectangle(runeSelect.X - 60, runeSelect.Y, 60, runeSelect.Height);
                if (!gameLogic.mousePressed && (runeSelect.Contains(mouseState.Position) || extraSpace.Contains(mouseState.Position) || arrowSpace.Contains(mouseState.Position)) && mouseState.LeftButton == ButtonState.Pressed && windowActive)
                {
                    if (!diffMove && gameLogic.selectedRune != 0)
                    {
                        gameLogic.selectedRune--;
                        gameLogic.difficulty = 0;
                    }
                    gameLogic.mousePressed = true;
                    diffMove = false;
                }
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, runeSelect, leftHover ? ThemeColors.Selected : ThemeColors.NotSelected);
                string runeName = ((Runes.Runes)gameLogic.selectedRune - 1).ToString().Substring(0, 1);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, runeName, new Vector2(MainGame.screenWidth / 4 - MainGame.Gfx.menuFont.MeasureString(runeName).X / 2, (int)(MainGame.screenHeight / 2.5f) - MainGame.Gfx.menuFont.MeasureString(runeName).Y / 2), ThemeColors.Text);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "<", new Vector2(runeSelect.X - MainGame.Gfx.menuFont.MeasureString("<").X * 2, (int)(MainGame.screenHeight / 2.5f) - 20), ThemeColors.Text);
            }
            {
                Rectangle runeSelect = new Rectangle(MainGame.screenWidth / 2 - rectWidth / 2 + (SaveManager.size * 20), (int)(MainGame.screenHeight / 2.5f) - rectHeight / 2 + (SaveManager.size * 20),
                    rectWidth - (SaveManager.size * 40), rectHeight - (SaveManager.size * 40));
                if (runeSelect.Contains(mouseState.Position))
                {
                    if (!gameLogic.mousePressed && mouseState.LeftButton == ButtonState.Pressed && windowActive
                        && UnlockManager.IsUnlockUnlocked(UnlockType.CharacterTutorial))
                    {
                        if (runeUnlocks.TryGetValue(((Runes.Runes)gameLogic.selectedRune, gameLogic.difficulty), out UnlockType unlockStart))
                        {
                            if (UnlockManager.IsUnlockUnlocked(unlockStart))
                            {
                                NewGameChoiceUpdate(ref enhancements);
                                gameLogic.gameState = GameState.LOADGAME;
                                gameLogic.tutorial = false;
                            }
                        }
                        gameLogic.mousePressed = true;
                        diffMove = false;
                    }
                }

                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, runeSelect, (diffMove || leftHover || rightHover) ? ThemeColors.NotSelected : ThemeColors.Selected);
                ThemeColors.DrawGlowCorners(runeSelect, ThemeColors.Text);
                string runeName = ((Runes.Runes)gameLogic.selectedRune).ToString();
                int topOffset = 10;
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, runeName, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.menuFont.MeasureString(runeName).X / 2, (int)(MainGame.screenHeight / 2.5f) - MainGame.Gfx.menuFont.MeasureString(runeName).Y * 3 + topOffset * 2), ThemeColors.Text);
                if (runeUnlocks.TryGetValue(((Runes.Runes)gameLogic.selectedRune, 0), out UnlockType unlock))
                {
                    if (UnlockManager.IsUnlockUnlocked(unlock))
                    {
                        var field = ((Runes.Runes)gameLogic.selectedRune).GetType().GetField(((Runes.Runes)gameLogic.selectedRune).ToString());
                        var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));

                        string[] descStrings = attribute.GetDescription().Split('\n');
                        int line = 0;
                        foreach (string desc in descStrings)
                        {
                            if (line == 0) MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, desc, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(desc).X / 4 + 7, (int)(MainGame.screenHeight / 2.5f) - MainGame.Gfx.gameFont.MeasureString(runeName).Y * (2 - line++) + topOffset), ThemeColors.Text);
                            else MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, desc, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(desc).X / 2, (int)(MainGame.screenHeight / 2.5f) - MainGame.Gfx.gameFont.MeasureString(runeName).Y * (2 - line++) + topOffset - 20), ThemeColors.Text);
                        }
                    }
                    else
                    {
                        var field = ((Runes.Runes)gameLogic.selectedRune).GetType().GetField(((Runes.Runes)gameLogic.selectedRune).ToString());
                        var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));

                        string[] descStrings = attribute.GetPrompt().Split('\n');
                        int line = 0;
                        foreach (string desc in descStrings)
                        {
                            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, desc, new Vector2(MainGame.screenWidth / 2 - MainGame.Gfx.gameFont.MeasureString(desc).X / 2, (int)(MainGame.screenHeight / 2.5f) - MainGame.Gfx.gameFont.MeasureString(runeName).Y * (2 - line++) + topOffset), ThemeColors.Wrong);
                        }
                    }
                }
            }
            if (gameLogic.selectedRune != maxRunes - 1)
            {
                Rectangle runeSelect = new Rectangle(MainGame.screenWidth - MainGame.screenWidth / 5 - rectWidth / 4 - SaveManager.size * 30, (int)(MainGame.screenHeight / 2.5f) - rectHeight / 4, rectWidth / 2, rectHeight / 2);
                Rectangle extraSpace = new Rectangle(runeSelect.X - 50, runeSelect.Y, 50, runeSelect.Height);
                Rectangle arrowSpace = new Rectangle(runeSelect.X + runeSelect.Width, runeSelect.Y, 60, runeSelect.Height);
                if (!gameLogic.mousePressed && (runeSelect.Contains(mouseState.Position) || extraSpace.Contains(mouseState.Position) || arrowSpace.Contains(mouseState.Position)) &&
                    mouseState.LeftButton == ButtonState.Pressed && windowActive && UnlockManager.IsUnlockUnlocked(UnlockType.CharacterTutorial))
                {
                    gameLogic.selectedRune++;
                    gameLogic.difficulty = 0;
                    gameLogic.mousePressed = true;
                    diffMove = false;
                }
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, runeSelect, rightHover ? ThemeColors.Selected : ThemeColors.NotSelected);
                string runeName = ((Runes.Runes)gameLogic.selectedRune + 1).ToString().Substring(0, 1);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, runeName, new Vector2(MainGame.screenWidth - MainGame.screenWidth / 4 - MainGame.Gfx.menuFont.MeasureString(runeName).X / 2, (int)(MainGame.screenHeight / 2.5f) - MainGame.Gfx.menuFont.MeasureString(runeName).Y / 2), ThemeColors.Text);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, ">", new Vector2(runeSelect.X + runeSelect.Width + MainGame.Gfx.menuFont.MeasureString(">").X, (int)(MainGame.screenHeight / 2.5f) - 20), ThemeColors.Text);
            }

            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "Difficulty:", new Vector2(MainGame.screenWidth / 2f - MainGame.Gfx.menuFont.MeasureString("Difficulty:   ").X, MainGame.screenHeight - 150), ThemeColors.Text);

            bool locked = gameLogic.difficulty != 0
                && runeUnlocks.TryGetValue(((Runes.Runes)gameLogic.selectedRune, gameLogic.difficulty), out UnlockType diffUnlock)
                && !UnlockManager.IsUnlockUnlocked(diffUnlock);

            string diffNum = gameLogic.difficulty.ToString();
            string diffName = locked ? " ???" : DifficultyText(gameLogic.difficulty);
            Color diffColor = locked ? ThemeColors.Wrong : ThemeColors.Text;

            int padding = 6;
            float diffY = MainGame.screenHeight - 150;
            Vector2 leftArrowSize = MainGame.Gfx.menuFont.MeasureString("<");
            Vector2 numSize = MainGame.Gfx.menuFont.MeasureString(diffNum);
            float fixedNumWidth = MainGame.Gfx.menuFont.MeasureString("0").X;
            Vector2 rightArrowSize = MainGame.Gfx.menuFont.MeasureString(">");

            float numX = MainGame.screenWidth / 2f;
            float leftArrowX = numX - padding - fixedNumWidth / 2 - leftArrowSize.X;
            float rightArrowX = numX + padding + fixedNumWidth / 2;

            Rectangle leftArrowRect = new Rectangle((int)leftArrowX - padding, (int)diffY - padding, (int)leftArrowSize.X + padding * 2, (int)leftArrowSize.Y + padding * 2);
            Rectangle rightArrowRect = new Rectangle((int)rightArrowX - padding, (int)diffY - padding, (int)rightArrowSize.X + padding * 2, (int)rightArrowSize.Y + padding * 2);

            if (leftArrowRect.Contains(mouseState.Position) || rightArrowRect.Contains(mouseState.Position))
            {
                diffMove = true;
                gameLogic.canStartFight = false;
                if (!gameLogic.mousePressed && mouseState.LeftButton == ButtonState.Pressed && windowActive
                    && UnlockManager.IsUnlockUnlocked(UnlockType.CharacterTutorial))
                {
                    if (leftArrowRect.Contains(mouseState.Position) && gameLogic.difficulty != 0)
                        gameLogic.difficulty--;
                    else if (rightArrowRect.Contains(mouseState.Position) && gameLogic.difficulty != 5)
                        gameLogic.difficulty++;
                    gameLogic.mousePressed = true;
                }
            }
            else if (!GameLogic.keyboardUsed)
            {
                diffMove = false;
            }

            Color leftArrowColor = diffMove && leftArrowRect.Contains(mouseState.Position) ? ThemeColors.Selected : diffColor;
            Color rightArrowColor = diffMove && rightArrowRect.Contains(mouseState.Position) ? ThemeColors.Selected : diffColor;
            if (diffMove)
            {
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, leftArrowRect, locked ? ThemeColors.Wrong : ThemeColors.NotSelected);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, rightArrowRect, locked ? ThemeColors.Wrong : ThemeColors.NotSelected);
            }
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "<", new Vector2(leftArrowX, diffY), leftArrowColor);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, diffNum, new Vector2(numX - numSize.X / 2f, diffY), diffColor);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, ">", new Vector2(rightArrowX, diffY), rightArrowColor);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, diffName, new Vector2(rightArrowX + rightArrowSize.X + padding * 2, diffY), diffColor);
        }


        private void NewGameChoiceUpdate(ref Enhancements enhancements)
        {
            switch (gameLogic.selectedRune)
            {
                case (int)Runes.Runes.Uruz:
                    enhancements.AllLettersAddScore(1);
                    break;
                case (int)Runes.Runes.Halagaz:
                    enhancements.AllLettersAddScore(-6);
                    for (int i = 0; i < 13; i++)
                    {
                        int index = GameLogic.unseededRandom.Next(0, 26);
                        while (enhancements.letters[index] != -5)
                        {
                            index = GameLogic.unseededRandom.Next(0, 26);
                        }
                        enhancements.AddLetterScore((char)('a' + index), 15);
                    }
                    break;
                case (int)Runes.Runes.Jera:
                    gameLogic.coins = 80;
                    break;
                case (int)Runes.Runes.Naudhiz:
                    enhancements.bloomChance = 0.1;
                    break;
            }
        }

        private string DifficultyText(int difficulty)
        {
            return difficulty switch
            {
                0 => " Normal",
                1 => " -15 coins",
                2 => " Mistakes -5",
                3 => " Word score -1",
                4 => " Double damage",
                5 => " -5 words",
                _ => "",
            };
        }
    }
}
