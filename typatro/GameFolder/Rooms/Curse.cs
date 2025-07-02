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
        readonly int leftOffset = 100, descOffset = 150, rectTopOffset = 100, rectWidth = 100, rectHeight = 50;

        public CurseRoom(Enhancements enhancements)
        {
            this.enhancements = enhancements;
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

            Rectangle yesRect = new Rectangle(leftOffset, rectTopOffset + descOffset * 2, rectWidth, rectHeight);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, yesRect, pickUp ? ThemeColors.Selected : ThemeColors.NotSelected);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "yes", new Vector2(leftOffset + 10, rectTopOffset + descOffset * 2 + 5), ThemeColors.Text);
            
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
            

            Rectangle noRect = new Rectangle(leftOffset + rectWidth, rectTopOffset + descOffset * 2, rectWidth, rectHeight);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, noRect, pickUp ? ThemeColors.NotSelected : ThemeColors.Selected);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "no", new Vector2(leftOffset * 2 + rectHeight + 10, rectTopOffset + descOffset * 2 + 5), ThemeColors.Text);
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
            //curse = Curses.ChanceForCoins; //specific curse tester
        }


        public void CurseResolve(ref long coins)
        {
            switch (curse)
            {
                case Curses.LettersForCoins:
                    coins += 150;
                    for (int i = 0; i < 5; i++)
                    {
                        GameLogic.actions.Add(new UserAction("randomLetter", ""));
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
                    GameLogic.actions.Add(new UserAction("randomLetter", ""));
                    char randLetter = (char)(GameLogic.seededRandom.Next(0, 26) + 'a');
                    enhancements.AddLetterScore(randLetter, -20);
                    enhancements.MultiplyLetterScore(randLetter, -1);
                    break;
                case Curses.Rocks:
                    enhancements.stoneScore = 10;
                    enhancements.stoneChance += 0.25;
                    break;
            }
        }


        enum Curses
        {
            [Description("Decrease 5 random letters by -5, \n\nbut gain 150 coins")]
            LettersForCoins,
            [Description("Decrease all word chances by 2%, \n\nbut gain 150 coins")]
            ChanceForCoins,
            [Description("Multiply all letters by 1.2x, \n\nbut loose special word chances")]
            NoWordsChance,
            [Description("Decrease a random letter by -20, \n\nbut then multiply it by -1")]
            AroundTheWorld,
            [Description("Decrease stone word score to 10, \n\nbut gain 25% stone chance")]
            Rocks,
            [Description("Change shine mult to 1x, \n\nbut gain 10% shine chance")]
            Glimmer,
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
                _ => "",
            };
        }
    }
}