using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.Services;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder.Rooms
{
    class CurseRoom
    {
        Enhancements enhancements;
        Curses curse;
        bool pickUp = true, keyDown, enterReleased;
        readonly Rectangle panelRect;

        // Same checkmark/cross position as Treasure - Backgrounds/yes.png and no.png are the
        // shared overlay art for both rooms, on the same 128x64 canvas.
        static readonly Rectangle yesHitboxNative = new Rectangle(48, 43, 14, 13);
        static readonly Rectangle noHitboxNative = new Rectangle(64, 42, 15, 14);

        public CurseRoom(Enhancements enhancements)
        {
            this.enhancements = enhancements;
            int panelWidth = 850;
            panelRect = new Rectangle((MainGame.screenWidth - panelWidth) / 2, 50, panelWidth, 480);
        }

        public bool CurseRoomDisplay(ref long coins, ref bool mousePressed)
        {
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            if (keyboard.IsKeyUp(Keys.Enter))
            {
                enterReleased = true;
            }

            if (!keyDown && keyboard.IsKeyDown(Keys.Left))
            {
                pickUp = true;
                keyDown = true;
            }
            else if (!keyDown && keyboard.IsKeyDown(Keys.Right))
            {
                pickUp = false;
                keyDown = true;
            }
            if (keyboard.IsKeyUp(Keys.Left) && keyboard.IsKeyUp(Keys.Right))
            {
                keyDown = false;
            }
            if (UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.CurseTutorial) && keyboard.IsKeyDown(Keys.Enter) && enterReleased)
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

            // Confined to the play field - below the top banner, with a matching gap at the
            // bottom - instead of stretching the art edge to edge over the whole window.
            Rectangle screenRect = new Rectangle(0, 55, MainGame.screenWidth, MainGame.screenHeight - 55 - 15);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.curseBg, screenRect, Color.White);

            // The curse's name breathes between two shades and jitters a couple pixels, so
            // the room reads as "unstable" the instant you walk in rather than just being
            // another menu.
            int leftOffset = panelRect.X + 170;
            double t = MainGame.time.TotalGameTime.TotalSeconds;
            float pulse = (float)(0.5 + 0.5 * Math.Sin(t * 2.5));
            Color nameColor = Color.Lerp(ThemeColors.Text, ThemeColors.Wrong, pulse * 0.6f);
            Vector2 nameJitter = new Vector2((float)(Math.Sin(t * 13.0) * 1.5), (float)(Math.Cos(t * 9.0) * 1.5));
            float nameScale = 1.2f, descScale = 1.15f;
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, GetName(curse), new Vector2(leftOffset, panelRect.Y + 105) + nameJitter,
                nameColor, 0f, Vector2.Zero, nameScale, SpriteEffects.None, 0f);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, GetDescription(curse), new Vector2(leftOffset, panelRect.Y + 175),
                ThemeColors.Text, 0f, Vector2.Zero, descScale, SpriteEffects.None, 0f);

            float scaleX = screenRect.Width / 128f, scaleY = screenRect.Height / 64f;
            Rectangle yesRect = new Rectangle(screenRect.X + (int)(yesHitboxNative.X * scaleX), screenRect.Y + (int)(yesHitboxNative.Y * scaleY), (int)(yesHitboxNative.Width * scaleX), (int)(yesHitboxNative.Height * scaleY));
            Rectangle noRect = new Rectangle(screenRect.X + (int)(noHitboxNative.X * scaleX), screenRect.Y + (int)(noHitboxNative.Y * scaleY), (int)(noHitboxNative.Width * scaleX), (int)(noHitboxNative.Height * scaleY));

            if (!TutorialManager.IsShowing() && yesRect.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
            {
                if (!mousePressed && mouseState.LeftButton == ButtonState.Pressed)
                {
                    CurseResolve(ref coins);
                    mousePressed = true;
                    return true;
                }
                pickUp = true;
            }
            if (!TutorialManager.IsShowing() && noRect.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
            {
                if (!mousePressed && mouseState.LeftButton == ButtonState.Pressed)
                {
                    mousePressed = true;
                    return true;
                }
                pickUp = false;
            }

            MainGame.Gfx.spriteBatch.Draw(pickUp ? MainGame.Gfx.yesOverlay : MainGame.Gfx.noOverlay, screenRect, Color.White);

            return false;
        }

        public void NewCurse(bool forceTradeGlyph = false)
        {
            curse = forceTradeGlyph ? Curses.TradeGlyph : (Curses)GameLogic.unseededRandom.Next(0, Enum.GetValues(typeof(Curses)).Length);
        }


        public void CurseResolve(ref long coins)
        {
            switch (curse)
            {
                case Curses.LettersForCoins:
                    coins += 150;
                    for (int i = 0; i < 5; i++)
                    {
                        enhancements.AddLetterScore((char)(GameLogic.contextRandom.Next(0, 26) + 'a'), -5);
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
                    char randLetter = (char)(GameLogic.contextRandom.Next(0, 26) + 'a');
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
                    foreach (Glyph oldGlyph in GlyphManager.GetGlyphs())
                    {
                        enhancements.RemoveGlyphEnhancementsUpdate(oldGlyph);
                    }
                    // NoGlyphsLeft is a placeholder for "you have none" that still lives in
                    // activeGlyphs, so it must be excluded here - otherwise it inflates the
                    // count and the curse hands out a free glyph even with nothing to gamble.
                    int glyphCount = GlyphManager.GetGlyphs().Count(g => g != Glyph.NoGlyphsLeft);
                    GlyphManager.RemoveAllGlyphs();
                    for (int i = 0; i < glyphCount; i++)
                    {
                        Glyph newGlyph = GlyphManager.GetRandomUnusedGlyph();
                        GlyphManager.Add(newGlyph);
                        enhancements.AddGlyphEnhancementsUpdate(newGlyph);
                    }
                    break;
                case Curses.TradeGlyph:
                    Glyph removedGlyph = GlyphManager.RemoveRandom();
                    // Nothing to trade with no glyphs to begin with - the curse just does
                    // nothing instead of still handing out a free one.
                    if (removedGlyph != Glyph.NoGlyphsLeft)
                    {
                        enhancements.RemoveGlyphEnhancementsUpdate(removedGlyph);
                        Glyph tradedGlyph = GlyphManager.GetRandomUnusedGlyph();
                        GlyphManager.Add(tradedGlyph);
                        enhancements.AddGlyphEnhancementsUpdate(tradedGlyph);
                    }
                    break;
                case Curses.DoYouBelieve:
                    for (int i = 0; i < 26; i++)
                    {
                        enhancements.AddLetterScore((char)('a' + i), GameLogic.contextRandom.Next(0, 2) == 1 ? 20 : -20);
                    }
                    break;
                case Curses.AllForGlyphs:
                    coins = -100;
                    Glyph freeGlyph = GlyphManager.GetRandomUnusedGlyph();
                    GlyphManager.Add(freeGlyph);
                    enhancements.AddGlyphEnhancementsUpdate(freeGlyph);
                    break;
                case Curses.HangingQueen:
                    enhancements.MultiplyLetterScore(enhancements.HighestLetter().bestLetter,0);
                    enhancements.AllLettersMultiplyScore(1.5);
                    break;
                case Curses.HangingRook:
                    enhancements.AllLettersAddScore(5);
                    enhancements.MultiplyLetterScore(enhancements.HighestLetter().bestLetter,0);
                    break;
                case Curses.SilenceOfSound:
                    enhancements.AllLettersMultiplyScore(1.5);
                    enhancements.MultiplyLetterScore('a', 0);
                    enhancements.MultiplyLetterScore('e', 0);
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
            [Description("- Your highest letter score is set to 0\n\n+ Add +5 to all other letters")]
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