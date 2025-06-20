using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder.Rooms
{
    class CurseRoom
    {
        MainGame.Gfx gfx;
        Enhancements enhancements;
        Curses curse;
        bool pickUp = true, keyDown, enterReleased;
        readonly int leftOffset = 50, descOffset = 150, rectTopOffset = 200, rectWidth = 100, rectHeight = 50;

        public CurseRoom(MainGame.Gfx gfx, Enhancements enhancements)
        {
            this.gfx = gfx;
            this.enhancements = enhancements;
        }

        public bool CurseRoomDisplay(ref long coins)
        {
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyUp(Keys.Enter))
            {
                enterReleased = true;
            }

            if(!keyDown && (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.Right))){
                pickUp = !pickUp;
                keyDown = true;
            }
            if(keyboard.IsKeyUp(Keys.Left) && keyboard.IsKeyUp(Keys.Right)){
                keyDown = false;
            }
            if(keyboard.IsKeyDown(Keys.Enter) && enterReleased){
                if(pickUp){
                    CurseResolve(ref coins);
                }
                enterReleased = false;
                return true;
            }

            gfx.spriteBatch.DrawString(gfx.smallTextFont, "Curse room", new Vector2(200, 200), ThemeColors.Text);
            gfx.spriteBatch.DrawString(gfx.smallTextFont, GetDescription(curse), new Vector2(200, 300), ThemeColors.Text);

            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(leftOffset, rectTopOffset+descOffset*2, rectWidth, rectHeight), pickUp ? ThemeColors.Selected : ThemeColors.NotSelected);
            gfx.spriteBatch.DrawString(gfx.gameFont, "yes", new Vector2(leftOffset+10, rectTopOffset+descOffset*2+5), ThemeColors.Text);
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(leftOffset*2+rectHeight, rectTopOffset+descOffset*2, rectWidth, rectHeight), pickUp ? ThemeColors.NotSelected : ThemeColors.Selected);
            gfx.spriteBatch.DrawString(gfx.gameFont, "no", new Vector2(leftOffset*2+rectHeight+10, rectTopOffset+descOffset*2+5), ThemeColors.Text);


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
            }
        }


        enum Curses
        {
            [Description("Decrease 5 random letters by -5, but gain 150 coins")]
            LettersForCoins,
            [Description("Decrease all word chances by 2%, but gain 150 coins")]
            ChanceForCoins,
            [Description("Multiply all letters by 1.2x, but loose special word chances")]
            NoWordsChance,

            [Description("Decrease a random letter by -20, but then multiply it by -1")]
            AroundTheWorld
        }
        
        string GetDescription(Curses? curse)
        {
            var field = curse.GetType().GetField(curse.ToString());
            var attribute = (DescriptionAttribute)System.Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? "No description available" : attribute.Description;
        }
    }
}