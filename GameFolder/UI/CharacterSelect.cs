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
                if (!gameLogic.mousePressed && (runeSelect.Contains(mouseState.Position) || extraSpace.Contains(mouseState.Position)) && mouseState.LeftButton == ButtonState.Pressed && windowActive)
                {
                    if (!diffMove && gameLogic.selectedRune != 0)
                    {
                        gameLogic.selectedRune--;
                        gameLogic.difficulty = 0;
                    }
                    gameLogic.mousePressed = true;
                    diffMove = false;
                }
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, runeSelect, ThemeColors.NotSelected);
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

                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, runeSelect, diffMove ? ThemeColors.NotSelected : ThemeColors.Selected);
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
                if (!gameLogic.mousePressed && (runeSelect.Contains(mouseState.Position) || extraSpace.Contains(mouseState.Position)) &&
                    mouseState.LeftButton == ButtonState.Pressed && windowActive && UnlockManager.IsUnlockUnlocked(UnlockType.CharacterTutorial))
                {
                    if (gameLogic.selectedRune != maxRunes - 1)
                    {
                        gameLogic.selectedRune++;
                        gameLogic.difficulty = 0;
                    }
                    gameLogic.mousePressed = true;
                    diffMove = false;
                }
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, runeSelect, ThemeColors.NotSelected);
                string runeName = ((Runes.Runes)gameLogic.selectedRune + 1).ToString().Substring(0, 1);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, runeName, new Vector2(MainGame.screenWidth - MainGame.screenWidth / 4 - MainGame.Gfx.menuFont.MeasureString(runeName).X / 2, (int)(MainGame.screenHeight / 2.5f) - MainGame.Gfx.menuFont.MeasureString(runeName).Y / 2), ThemeColors.Text);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, ">", new Vector2(runeSelect.X + runeSelect.Width + MainGame.Gfx.menuFont.MeasureString(">").X, (int)(MainGame.screenHeight / 2.5f) - 20), ThemeColors.Text);
            }

            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "Difficulty: ", new Vector2(MainGame.screenWidth / 2f - MainGame.Gfx.menuFont.MeasureString("Difficulty: ").X, MainGame.screenHeight - 150), ThemeColors.Text);

            int padding = 10;

            string diffString = $"<{gameLogic.difficulty}: {DifficultyText(gameLogic.difficulty)}>";
            if (gameLogic.difficulty != 0
                && runeUnlocks.TryGetValue(((Runes.Runes)gameLogic.selectedRune, gameLogic.difficulty), out UnlockType diffUnlock)
                && !UnlockManager.IsUnlockUnlocked(diffUnlock))
            {
                diffString = "<?>";
            }
            Vector2 diffStringSize = MainGame.Gfx.menuFont.MeasureString(diffString);
            Rectangle diffRectLeft = new Rectangle(MainGame.screenWidth / 2 - padding, MainGame.screenHeight - 150 - padding, ((int)diffStringSize.X + padding * 2) / 2, (int)diffStringSize.Y + padding);
            Rectangle diffRectRight = new Rectangle(MainGame.screenWidth / 2 - padding + ((int)diffStringSize.X + padding * 2) / 2, MainGame.screenHeight - 150 - padding, ((int)diffStringSize.X + padding * 2) / 2, (int)diffStringSize.Y + padding);
            if (diffRectLeft.Contains(mouseState.Position) || diffRectRight.Contains(mouseState.Position))
            {
                if (!gameLogic.mousePressed && mouseState.LeftButton == ButtonState.Pressed && windowActive
                    && UnlockManager.IsUnlockUnlocked(UnlockType.CharacterTutorial))
                {
                    if (diffRectLeft.Contains(mouseState.Position))
                    {
                        if (gameLogic.difficulty != 0)
                        {
                            gameLogic.difficulty--;
                        }
                    }
                    else if (diffRectRight.Contains(mouseState.Position))
                    {
                        if (gameLogic.difficulty != 5)
                        {
                            gameLogic.difficulty++;
                        }
                    }
                    
                }

                gameLogic.canStartFight = false;
                diffMove = true;
            }
            if (diffMove)
            {
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, diffRectLeft, (diffString == "<?>") ? ThemeColors.Wrong : ThemeColors.Selected);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, diffRectRight, (diffString == "<?>") ? ThemeColors.Wrong : ThemeColors.Selected);
            }
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, diffString, new Vector2(MainGame.screenWidth / 2f, MainGame.screenHeight - 150), ThemeColors.Text);
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
