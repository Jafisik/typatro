using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


namespace typatro.GameFolder
{

    class Menu
    {
        SpriteBatch _spriteBatch;
        SpriteFont font;
        Texture2D texture;
        int menuLineSpacing = 100, topOffset = 80, leftOffset = 160, rectWidth = 400, rectHeight = 80;
        enum MenuSelect
        {
            PLAY,
            OPTIONS,
            EXIT,
            MENU
        }
        private MenuSelect menuSelect = MenuSelect.PLAY;
        string[] menuTexts;
        bool menuNav = true;

        public Menu(SpriteBatch spriteBatch, SpriteFont font, Texture2D texture)
        {
            this._spriteBatch = spriteBatch;
            this.font = font;
            this.texture = texture;
            int menuSelectLength = Enum.GetValues(typeof(MenuSelect)).Length - 1;
            List<string> tempMenuTexts = new List<string>(menuSelectLength);
            for (int menuItems = 0; menuItems < menuSelectLength; menuItems++)
            {
                tempMenuTexts.Add(((MenuSelect)menuItems).ToString().ToLower());
            }
            menuTexts = tempMenuTexts.ToArray();
        }

        //To add new new menu item just add an item into enum MenuSelect and in the class MainGame add the same thing to enum GameState
        public int DrawMainMenu(GraphicsDeviceManager graphicsDevice)
        {
            Color[] menuColors = new Color[menuTexts.Length];
            KeyboardState state = Keyboard.GetState();
            if (menuNav)
            {
                if (state.IsKeyDown(Keys.Down) && menuSelect != MenuSelect.EXIT) menuSelect++;
                else if (state.IsKeyDown(Keys.Up) && menuSelect != MenuSelect.PLAY) menuSelect--;
                menuNav = false;
            }

            if (state.IsKeyUp(Keys.Up) && state.IsKeyUp(Keys.Down))
            {
                menuNav = true;
            }

            for (int menuIndex = 0; menuIndex < menuTexts.Length; menuIndex++)
            {
                if ((int)menuSelect == menuIndex) menuColors[menuIndex] = Color.Gray;
                else menuColors[menuIndex] = Color.LightGray;
            }
            if (state.IsKeyDown(Keys.Enter)) return (int)menuSelect;

            for (int line = 0; line < menuTexts.Length; line++)
            {
                _spriteBatch.Draw(texture, new Rectangle((graphicsDevice.PreferredBackBufferWidth - rectWidth) / 2, topOffset + menuLineSpacing * line, rectWidth, rectHeight), menuColors[line]);
                Vector2 textSize = font.MeasureString(menuTexts[line]);
                Vector2 textPos = new Vector2((graphicsDevice.PreferredBackBufferWidth - textSize.X) / 2, 80 + rectHeight / 2 - textSize.Y / 3 + menuLineSpacing * line);
                _spriteBatch.DrawString(font, menuTexts[line], textPos, Color.Black);
            }
            return (int)MenuSelect.MENU;
        }

        public void DrawOptionsMenu(GraphicsDeviceManager graphicsDevice)
        {
            Vector2 textPosition = new Vector2((graphicsDevice.PreferredBackBufferWidth - font.MeasureString("options").X) / 2, 80);
            _spriteBatch.DrawString(font, "options", textPosition, Color.AliceBlue);

            _spriteBatch.DrawString(font, "theme", new Vector2(leftOffset, topOffset*2),Color.Black);
            _spriteBatch.DrawString(font, "theme", new Vector2(leftOffset, topOffset*3),Color.Black);
        }
    }
}