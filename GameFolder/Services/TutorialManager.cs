using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using typatro.GameFolder.UI;

namespace typatro.GameFolder.Services
{
    public class TutorialStep
    {
        public string Text;
        public Color? OverrideColor;
        public Func<SpriteFont, Rectangle> GetBox;
    }

    public static class TutorialManager
    {
        private static List<TutorialStep> currentSteps;
        private static int currentStep = 0;
        private static bool tutorial = false;
        private static bool waitingForRelease = false;

        public static void Start(List<TutorialStep> steps)
        {
            currentSteps = steps;
            currentStep = 0;
            tutorial = false;
        }

        public static bool Draw(KeyboardState state, MouseState mouseState, SpriteFont font)
        {
            if (waitingForRelease)
            {
                if (state.IsKeyUp(Keys.Enter) && mouseState.LeftButton == ButtonState.Released)
                {
                    waitingForRelease = false;
                    return true;
                }
                return false;
            }

            if (currentSteps == null || currentStep >= currentSteps.Count) return false;

            MainGame.Gfx.spriteBatch.Draw(
                MainGame.Gfx.texture,
                new Rectangle(15, 15, MainGame.screenWidth - 30, MainGame.screenHeight - 30),
                ThemeColors.ShopReroll);

            TutorialStep step = currentSteps[currentStep];
            Rectangle box = step.GetBox(font);
            Color color = step.OverrideColor ?? ThemeColors.Text;

            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, box, ThemeColors.ExitShop);
            MainGame.Gfx.spriteBatch.DrawString(font, step.Text, new Vector2(box.X + 10, box.Y + 10), color);

            if (state.IsKeyUp(Keys.Enter) && mouseState.LeftButton == ButtonState.Released)
                tutorial = true;

            if (tutorial && (state.IsKeyDown(Keys.Enter) || mouseState.LeftButton == ButtonState.Pressed))
            {
                currentStep++;
                tutorial = false;

                if (currentStep >= currentSteps.Count)
                {
                    currentSteps = null;
                    waitingForRelease = true;
                }
            }

            return false;
        }

        public static List<TutorialStep> CharacterSteps()
        {
            int rectWidth = MainGame.screenWidth / 3, rectHeight = MainGame.screenHeight / 2;
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    Text = "Runes help you\nin your runs ->",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Runes help you\nin your runs ->");
                        return new Rectangle(
                            (int)(MainGame.screenWidth / 2 - rectWidth / 2 + (SaveManager.size * 20) - size.X - 10),
                            (int)(MainGame.screenHeight / 2.5f) - rectHeight / 2 + (SaveManager.size * 20) + 90,
                            (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "Use arrow keys\nor mouse to        ->\nchoose your rune",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Use arrow keys\nor mouse to        ->\nchoose your rune");
                        return new Rectangle(
                            (int)(MainGame.screenWidth / 2 + rectWidth / 2 + (SaveManager.size * 20) - size.X + 30),
                            (int)(MainGame.screenHeight / 2.5f) - rectHeight / 2 + (SaveManager.size * 20) + 90,
                            (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "Choose your\ndifficulty    ->",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Choose your\ndifficulty    ->");
                        return new Rectangle(
                            (int)(MainGame.screenWidth / 2 - rectWidth / 2 + (SaveManager.size * 20) - size.X - 10),
                            (int)(MainGame.screenHeight - 150 - font.MeasureString(" ").Y - 10),
                            (int)size.X + 20, (int)size.Y + 20);
                    }
                },
            };
        }

        public static List<TutorialStep> ShopSteps()
        {
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    Text = "<- Use arrow keys\n   or mouse\n   purchase upgrades",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("<- Use arrow keys\n   or mouse\n   purchase upgrades");
                        return new Rectangle(270, SaveManager.size == 0 ? 100 : MainGame.screenHeight / 3,
                            (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "<- When highlited\n   each card\n   will show its\n   description",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("<- When highlited\n   each card\n   will show its\n   description");
                        return new Rectangle(400, SaveManager.size == 0 ? 300 : (MainGame.screenHeight / 3) * 2 - 30,
                            (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "You can reroll    ->\ncards in the shop",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("You can reroll    ->\ncards in the shop");
                        return new Rectangle(460, MainGame.screenHeight / 3, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "After you're finished ->\nexit the shop",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("After you're finished ->\nexit the shop");
                        return new Rectangle(400, MainGame.screenHeight / 3 + 100, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
            };
        }
    }
}
