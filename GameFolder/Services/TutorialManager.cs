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
        public string SecondaryText;
        public Color? SecondaryColor;
        // "left", "right", "up" or "down" - draws a small separate box with just the
        // arrow glyph, touching that side of the main box, instead of baking "<-"/"->"
        // into the text itself.
        public string Arrow;
        // Extra (x, y) nudge applied on top of the arrow's default auto-computed position,
        // for cases that need the arrow off its default centered spot without moving Text.
        public Point ArrowOffset = Point.Zero;
    }

    public static class TutorialManager
    {
        private static List<TutorialStep> currentSteps;
        private static int currentStep = 0;
        private static bool tutorial = false;
        private static bool waitingForRelease = false;
        private static bool startWaiting = false;

        // True while a tutorial popup sequence is actively being shown OR has just finished
        // and is waiting for the key/click release before Draw() reports completion - used
        // instead of a per-caller "already started" bool, since those can get stuck true
        // forever if the sequence gets interrupted before it unlocks. Must also cover the
        // waitingForRelease tail, otherwise a caller sees "nothing showing" the instant the
        // last step completes and immediately restarts the whole sequence from step 0.
        public static bool IsShowing() => currentSteps != null || waitingForRelease;

        public static void Start(List<TutorialStep> steps, bool waitForRelease = false)
        {
            currentSteps = steps;
            currentStep = 0;
            tutorial = false;
            waitingForRelease = false;
            startWaiting = waitForRelease;
        }

        public static bool Draw(KeyboardState state, MouseState mouseState)
        {
            SpriteFont font = TutorialFont();
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

            // Dims the screen so the bubble stands out, but stays translucent so whatever
            // it's pointing at (a shop card, a curse choice, ...) is still visible through it.
            Color dim = ThemeColors.ShopReroll;
            dim.A = 160;
            MainGame.Gfx.spriteBatch.Draw(
                MainGame.Gfx.texture,
                new Rectangle(15, 15, MainGame.screenWidth - 30, MainGame.screenHeight - 30),
                dim);

            TutorialStep step = currentSteps[currentStep];
            Rectangle box = step.GetBox(font);

            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, box, ThemeColors.ExitShop);
            MainGame.Gfx.spriteBatch.DrawString(font, step.Text, new Vector2(box.X + 10, box.Y + 10), step.OverrideColor ?? ThemeColors.Text);

            if (step.SecondaryText != null)
                MainGame.Gfx.spriteBatch.DrawString(font, step.SecondaryText, new Vector2(box.X + 10, box.Y + 10), step.SecondaryColor ?? ThemeColors.Text);

            if (step.Arrow != null)
            {
                string glyph = step.Arrow switch { "left" => "<-", "right" => "->", "up" => "^", "down" => "v", _ => "" };
                Vector2 glyphSize = font.MeasureString(glyph);
                int arrowGap = 6;
                Rectangle arrowBox = step.Arrow switch
                {
                    "left" => new Rectangle(box.X - (int)glyphSize.X - 20 - arrowGap, box.Y + box.Height / 2 - (int)glyphSize.Y / 2 - 8, (int)glyphSize.X + 20, (int)glyphSize.Y + 16),
                    "right" => new Rectangle(box.Right + arrowGap, box.Y + box.Height / 2 - (int)glyphSize.Y / 2 - 8, (int)glyphSize.X + 20, (int)glyphSize.Y + 16),
                    "up" => new Rectangle(box.X + box.Width / 2 - (int)glyphSize.X / 2 - 10, box.Y - (int)glyphSize.Y - 16 - arrowGap, (int)glyphSize.X + 20, (int)glyphSize.Y + 16),
                    _ => new Rectangle(box.X + box.Width / 2 - (int)glyphSize.X / 2 - 10, box.Bottom + arrowGap, (int)glyphSize.X + 20, (int)glyphSize.Y + 16),
                };
                arrowBox.X += step.ArrowOffset.X;
                arrowBox.Y += step.ArrowOffset.Y;
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, arrowBox, ThemeColors.ExitShop);
                MainGame.Gfx.spriteBatch.DrawString(font, glyph,
                    new Vector2(arrowBox.X + arrowBox.Width / 2 - glyphSize.X / 2, arrowBox.Y + arrowBox.Height / 2 - glyphSize.Y / 2),
                    step.OverrideColor ?? ThemeColors.Text);
            }

            if (startWaiting)
            {
                if (state.IsKeyUp(Keys.Enter) && mouseState.LeftButton == ButtonState.Released)
                    startWaiting = false;
                return false;
            }

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

        public static SpriteFont TutorialFont() => MainGame.Gfx.menuFont;


        // Shown at the tutorial map's very first fight - just the core loop: type to deal
        // damage, drain the health bar to win, but it heals back if you're slow.
        public static List<TutorialStep> FightSteps()
        {
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    Text = "Type the letters shown\nhere. Each correct one\ndeals damage to the enemy",
                    Arrow = "up",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Type the letters shown\nhere. Each correct one\ndeals damage to the enemy");
                        return new Rectangle(200, 350, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "This is the enemy's health.\nDrain it to 0 before you\nrun out of words to win",
                    Arrow = "up",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("This is the enemy's health.\nDrain it to 0 before you\nrun out of words to win");
                        return new Rectangle(290, 170, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "But it heals back over time -\nbe quick, or your progress\nslips away",
                    Arrow = "up",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("But it heals back over time -\nbe quick, or your progress\nslips away");
                        return new Rectangle(330, 170, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "Finishing words back-to-back\nbuilds a multiplier that boosts\nevery hit after that",
                    Arrow = "left",
                    ArrowOffset = new Point(0, -40),
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Finishing words back-to-back\nbuilds a multiplier that boosts\nevery hit after that");
                        return new Rectangle(300, 90, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "Each enemy has a unique\ntwist - check its description\nhere before you fight it",
                    Arrow = "right",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Each enemy has a unique\ntwist - check its description\nhere before you fight it");
                        return new Rectangle(MainGame.screenWidth - (int)size.X - 330, MainGame.screenHeight - 220, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
            };
        }

        // Shown at the tutorial map's second fight (after the shop), once the basics from
        // FightSteps are already understood - covers the colored special words.
        public static List<TutorialStep> SpecialWordsSteps()
        {
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    Text = "Special words\ngive you effects\nbased on the color\nif written correctly",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Special words\ngive you effects\nbased on the color\nif written correctly");
                        return new Rectangle(180, 200, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "Stone words give you a flat + score",
                    SecondaryText = "Stone words",
                    SecondaryColor = Color.Gray,
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Stone words give you a flat + score");
                        return new Rectangle(180, 200, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "Shiny words give you a * score multiplier",
                    SecondaryText = "Shiny words",
                    SecondaryColor = ThemeColors.Selected,
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Shiny words give you a * score multiplier");
                        return new Rectangle(180, 200, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "Bloom words upgrade the letters in the word\n\nStill counts even if you win the fight before reaching it",
                    SecondaryText = "Bloom words",
                    SecondaryColor = Color.DarkGreen,
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Bloom words upgrade the letters in the word\n\nStill counts even if you win the fight before reaching it");
                        return new Rectangle(180, 200, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
            };
        }

        // Shown the first time the player reaches the post-fight letter reward screen.
        public static List<TutorialStep> RewardSteps()
        {
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    Text = "Pick a letter to permanently\nboost - it'll score more\nevery time you type it",
                    Arrow = "up",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Pick a letter to permanently\nboost - it'll score more\nevery time you type it");
                        return new Rectangle(MainGame.screenWidth / 2 - (int)size.X / 2, 530, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
            };
        }

        // Shown the first time the player enters a Treasure room.
        public static List<TutorialStep> TreasureSteps()
        {
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    Text = "This is a Treasure room.\nYou're offered a glyph -\na permanent bonus for this run",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("This is a Treasure room.\nYou're offered a glyph -\na permanent bonus for this run");
                        return new Rectangle(200, 100, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "Accept keeps it for the rest\nof the run, decline skips it.\nRead the tradeoff first!",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Accept keeps it for the rest\nof the run, decline skips it.\nRead the tradeoff first!");
                        return new Rectangle(200, 100, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
            };
        }

        // Shown the first time the player enters a Curse room.
        public static List<TutorialStep> CurseSteps()
        {
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    Text = "This is a Curse room - it\nalways offers a trade:\na downside for an upside",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("This is a Curse room - it\nalways offers a trade:\na downside for an upside");
                        return new Rectangle(360, 100, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
            };
        }

        // Shown the first time the player is standing on a node with more than one forward
        // option - explains that a branch means a choice, not just one path forward.
        public static List<TutorialStep> MapChoiceSteps(Vector2 optionA, Vector2 optionB)
        {
            Vector2 mid = (optionA + optionB) / 2f;
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    Text = "This branches into multiple\nrooms - use arrow keys or\nmouse to pick which one",
                    Arrow = "down",
                    ArrowOffset = new Point(-90, 0),
                    GetBox = font => {
                        Vector2 size = font.MeasureString("This branches into multiple\nrooms - use arrow keys or\nmouse to pick which one");
                        return new Rectangle(280, 50, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
            };
        }

        // Just a welcome and a pointer at the first fight node - everything else (inventory,
        // escape to menu, etc.) is taught later, closer to where it actually matters.
        public static List<TutorialStep> MapSteps(Vector2 firstFightPoint)
        {
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    Text = "Welcome to Glyphora!\nLet's walk through your first run.",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Welcome to Glyphora!\nLet's walk through your first run.");
                        return new Rectangle(MainGame.screenWidth / 2 - (int)size.X / 2 - 10, 130, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "Click here, or press Enter,\nto start your first fight",
                    Arrow = "down",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Click here, or press Enter,\nto start your first fight");
                        return new Rectangle((int)firstFightPoint.X - (int)size.X / 2, (int)firstFightPoint.Y - (int)size.Y - 95, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
            };
        }

        public static List<TutorialStep> CharacterSteps()
        {
            int rectWidth = MainGame.screenWidth / 3, rectHeight = MainGame.screenHeight / 2;
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    Text = "Runes help you\nin your runs",
                    Arrow = "right",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Runes help you\nin your runs");
                        return new Rectangle(
                            (int)(MainGame.screenWidth / 2 - rectWidth / 2 + (40) - size.X - 30),
                            (int)(MainGame.screenHeight / 2.5f) - rectHeight / 2 + (40) + 90,
                            (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "Use arrow keys\nor mouse to\nchoose your rune",
                    Arrow = "right",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Use arrow keys\nor mouse to\nchoose your rune");
                        return new Rectangle(
                            (int)(MainGame.screenWidth / 2 + rectWidth / 2 + (40) - size.X + 10),
                            (int)(MainGame.screenHeight / 2.5f) - rectHeight / 2 + (40) + 90,
                            (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "Choose your\ndifficulty",
                    Arrow = "right",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Choose your\ndifficulty");
                        return new Rectangle(
                            (int)(MainGame.screenWidth / 2 - rectWidth / 2 + (40) - size.X - 30),
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
                    Text = "Spend coins here to buy\npermanent upgrades\nwith arrow keys or mouse",
                    Arrow = "left",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Spend coins here to buy\npermanent upgrades\nwith arrow keys or mouse");
                        return new Rectangle(340, MainGame.screenHeight / 3 - 40,
                            (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "When highlighted\neach card\nwill show its\ndescription",
                    Arrow = "left",
                    ArrowOffset = new Point(0,-70),
                    GetBox = font => {
                        Vector2 size = font.MeasureString("When highlighted\neach card\nwill show its\ndescription");
                        return new Rectangle(700, (MainGame.screenHeight / 3) * 2 - 30,
                            (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "You can reroll\ncards in the shop",
                    Arrow = "right",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("You can reroll\ncards in the shop");
                        return new Rectangle(480, MainGame.screenHeight / 3-30, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
                new TutorialStep
                {
                    Text = "After you're finished\nexit the shop",
                    Arrow = "right",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("After you're finished\nexit the shop");
                        return new Rectangle(380, MainGame.screenHeight / 3 + 80, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
            };
        }

        // Shown once, back on the map right after the player's first Treasure pickup -
        // by then they actually have something (a glyph) worth checking in there.
        public static List<TutorialStep> InventorySteps()
        {
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    Text = "Press Tab, or click the i\nup here, to see your letter\nupgrades and glyphs",
                    Arrow = "right",
                    GetBox = font => {
                        Vector2 size = font.MeasureString("Press Tab, or click the i\nup here, to see your letter\nupgrades and glyphs");
                        return new Rectangle(MainGame.screenWidth - (int)size.X - 180, 80, (int)size.X + 20, (int)size.Y + 20);
                    }
                },
            };
        }
    }
}
