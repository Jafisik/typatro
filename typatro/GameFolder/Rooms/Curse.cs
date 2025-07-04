using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;
using static Microsoft.Xna.Framework.Graphics.SpriteFont;

namespace typatro.GameFolder.Rooms
{
    class CurseRoom
    {
        Enhancements enhancements;
        Curses curse;
        bool pickUp = true, keyDown, enterReleased;
        readonly int topOffset = 80, rectWidth = 170, rectHeight = 60, rectOffset;

        public CurseRoom(Enhancements enhancements)
        {
            this.enhancements = enhancements;
            rectOffset = MainGame.screenWidth / 4;
        }

        public bool CurseRoomDisplay(ref long coins, ref bool mousePressed)
        {
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            if (keyboard.IsKeyUp(Keys.Enter))
            {
                enterReleased = true;
            }

            if (!keyDown && (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.Right)))
            {
                pickUp = !pickUp;
                keyDown = true;
            }
            if (keyboard.IsKeyUp(Keys.Left) && keyboard.IsKeyUp(Keys.Right))
            {
                keyDown = false;
            }
            if (keyboard.IsKeyDown(Keys.Enter) && enterReleased)
            {
                if (pickUp)
                {
                    CurseResolve(ref coins);
                }
                enterReleased = false;
                return true;
            }

            if(mouseState.LeftButton == ButtonState.Released)
            {
                mousePressed = false;
            }

            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, GetName(curse), new Vector2(100, 100), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, GetDescription(curse), new Vector2(100, 200), ThemeColors.Text);

            Rectangle yesRect = new Rectangle(rectOffset, topOffset * 5, rectWidth, rectHeight);

            Vector2 yesSize = MainGame.Gfx.gameFont.MeasureString("accept");
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, yesRect, pickUp ? ThemeColors.Selected : ThemeColors.NotSelected);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "accept", new Vector2(yesRect.X + yesRect.Width / 2 - yesSize.X / 2, yesRect.Y + yesRect.Height / 2 - yesSize.Y / 2 + 5), ThemeColors.Text);

            if (yesRect.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
            {
                if (!mousePressed && mouseState.LeftButton == ButtonState.Pressed)
                {
                    CurseResolve(ref coins);
                    mousePressed = true;
                    return true;
                }
                pickUp = true;
            }


            Rectangle noRect = new Rectangle(rectOffset * 2, topOffset * 5, rectWidth, rectHeight);
            Vector2 noSize = MainGame.Gfx.gameFont.MeasureString("decline");
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, noRect, pickUp ? ThemeColors.NotSelected : ThemeColors.Selected);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "decline", new Vector2(noRect.X + noRect.Width / 2 - noSize.X / 2, noRect.Y + noRect.Height / 2 - noSize.Y / 2 + 5), ThemeColors.Text);
            if (noRect.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
            {
                if (!mousePressed && mouseState.LeftButton == ButtonState.Pressed)
                {
                    mousePressed = true;
                    return true;
                }
                pickUp = false;
            }
            
            return false;
        }

        public void NewCurse()
        {
            curse = (Curses)GameLogic.unseededRandom.Next(0, Enum.GetValues(typeof(Curses)).Length);
            curse = Curses.GambleVision; //specific curse tester
        }


        public void CurseResolve(ref long coins)
        {
            switch (curse)
            {
                case Curses.LettersForCoins:
                    coins += 150;
                    for (int i = 0; i < 5; i++)
                    {
                        if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("randomLetter", ""));
                        enhancements.AddLetterScore((char)(GameLogic.seededRandom.Next(0, 26) + 'a'), -5);
                    }
                    break;
                case Curses.ChanceForCoins:
                    coins += 150;
                    enhancements.AddShinyChance(-0.02);
                    enhancements.AddStoneChance(-0.02);
                    enhancements.AddBloomChance(-0.02);
                    break;
                case Curses.NoWordsChance:
                    enhancements.AllLettersMultiplyScore(1.5);
                    enhancements.bloomChance = 0;
                    enhancements.shinyChance = 0;
                    enhancements.stoneChance = 0;
                    break;
                case Curses.AroundTheWorld:
                    if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("randomLetter", ""));
                    char randLetter = (char)(GameLogic.seededRandom.Next(0, 26) + 'a');
                    enhancements.AddLetterScore(randLetter, -20);
                    enhancements.MultiplyLetterScore(randLetter, -1);
                    break;
                case Curses.Rocks:
                    enhancements.stoneScore = 10;
                    enhancements.stoneChance += 0.25;
                    break;
                case Curses.Glimmer:
                    enhancements.shinyScore = 1;
                    enhancements.shinyChance = 0.1;
                    break;
                case Curses.GambleVision:
                    int glyphCount = GlyphManager.GetGlyphCount();
                    GlyphManager.RemoveAllGlyphs();
                    for (int i = 0; i < glyphCount; i++)
                    {
                        GlyphManager.Add(GlyphManager.GetRandomUnusedGlyph());
                    }
                    break;
                case Curses.TradeGlyph:
                    GlyphManager.RemoveRandom();
                    GlyphManager.Add(GlyphManager.GetRandomUnusedGlyph());
                    break;
                case Curses.DoYouBelieve:
                    for (int i = 0; i < 26; i++)
                    {
                        enhancements.AddLetterScore((char)('a' + i), GameLogic.seededRandom.Next(0, 2) == 1 ? 20 : -20);
                        if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("randomLetter", ""));
                    }
                    break;
                case Curses.AllForGlyphs:
                    coins = -100;
                    GlyphManager.Add(GlyphManager.GetRandomUnusedGlyph());
                    break;
                case Curses.HangingQueen:
                    enhancements.MultiplyLetterScore(enhancements.HighestLetter().bestLetter,0);
                    enhancements.AllLettersMultiplyScore(1.5);
                    break;
                case Curses.HangingRook:
                    enhancements.AllLettersAddScore(25);
                    enhancements.MultiplyLetterScore(enhancements.HighestLetter().bestLetter,0);
                    break;
                case Curses.SilenceOfSound:
                    enhancements.AllLettersMultiplyScore(1.5);
                    enhancements.MultiplyLetterScore('a', 0);
                    enhancements.MultiplyLetterScore('a', 0);
                    enhancements.MultiplyLetterScore('i', 0);
                    enhancements.MultiplyLetterScore('o', 0);
                    enhancements.MultiplyLetterScore('u', 0);
                    break;
                default:
                    break;
            }
        }


        enum Curses
        {
            [Description("- Decrease 5 random letters by -5\n\n+ Gain 150 coins")]
            LettersForCoins,
            [Description("- Decrease all word chances by 2%\n\n+ Gain 150 coins")]
            ChanceForCoins,
            [Description("- Lose special word chances\n\n+ Multiply all letters by 1.2x")]
            NoWordsChance,
            [Description("- Decrease a random letter by -20\n\n+ Multiply the same letter by -1")]
            AroundTheWorld,
            [Description("- Decrease stone word score to 10\n\n+ Gain 25% stone chance")]
            Rocks,
            [Description("- Change shine mult to 1x\n\n+ Gain 10% shine chance")]
            Glimmer,
            [Description("- Remove 1 random glyph\n\n+ Gain 1 new glyph")]
            TradeGlyph,
            [Description("- Remove all glyphs\n\n+ Replace them with new ones")]
            GambleVision,
            [Description("- All vowels score 0\n\n+ Multiply consonants by 1.5x")]
            SilenceOfSound,
            [Description("Randomly add +20 or -20 to your letter scores")]
            DoYouBelieve,
            [Description("- Your highest letter score is set to 0\n\n+ Multiply all letters by 2x")]
            HangingQueen,
            [Description("- Your highest letter score is set to 0\n\n+ Add +25 to all other letters")]
            HangingRook,
            [Description("- Set coins to -100\n\n+ Gain a random glyph")]
            AllForGlyphs,
        }

        string GetDescription(Curses? curse)
        {
            var field = curse.GetType().GetField(curse.ToString());
            var attribute = (DescriptionAttribute)System.Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? "No description available" : attribute.Description;
        }

        string GetName(Curses? curse)
        {
            return curse switch
            {
                Curses.LettersForCoins => "Letters for coins",
                Curses.ChanceForCoins => "Chance for coins",
                Curses.AroundTheWorld => "Around the world",
                Curses.Glimmer => "Glimmer",
                Curses.NoWordsChance => "No word chance",
                Curses.Rocks => "Rocks",
                Curses.AllForGlyphs => "All for glyphs",
                Curses.DoYouBelieve => "Do you believe?",
                Curses.GambleVision => "Gamble vision",
                Curses.HangingQueen => "Hanging queen",
                Curses.HangingRook => "Hanging rook",
                Curses.TradeGlyph => "Trade glyph",
                Curses.SilenceOfSound => "Silence of sound",
                
                _ => "Not defined",
            };
        }
    }
}