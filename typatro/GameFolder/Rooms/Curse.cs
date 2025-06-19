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
        int selectIndex;
        bool enterReleased;

        public CurseRoom(MainGame.Gfx gfx, Enhancements enhancements)
        {
            this.gfx = gfx;
            this.enhancements = enhancements;
        }

        public bool CurseRoomDisplay()
        {
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyUp(Keys.Enter))
            {
                enterReleased = true;
            }
            if (keyboard.IsKeyDown(Keys.Enter) && enterReleased)
            {
                enterReleased = false;
                return true;
            }

            gfx.spriteBatch.DrawString(gfx.smallTextFont, "Curse room", new Vector2(200, 200), ThemeColors.Text);
            gfx.spriteBatch.DrawString(gfx.smallTextFont, GetDescription((Curses)GameLogic.unseededRandom.Next(0, 2)), new Vector2(200, 300), ThemeColors.Text);


            return false;
        }


        enum Curses
        {
            [Description("Decrease 5 letters by -5 and gain 150 coins")]
            LettersForCoins,
            [Description("Multiply all letters by 1.5x, but loose special word chances")]
            NoWordsChance,

        }
        
        string GetDescription(Curses? curse)
        {
            var field = curse.GetType().GetField(curse.ToString());
            var attribute = (DescriptionAttribute)System.Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? "No description available" : attribute.Description;
        }
    }
}