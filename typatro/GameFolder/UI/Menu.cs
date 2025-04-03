using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


namespace typatro.GameFolder
{

    class Menu
    {
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D texture;
        int menuLineSpacing = 100, topOffset = 140;
        int menuRectWidth = 400, menuRectHeight = 80;
        int leftOffset = 140, optionRectWidth = 300, optionRectHeight = 80;

        bool menuNav = true;
        enum MenuSelect
        {
            PLAY,
            OPTIONS,
            EXIT,
            EMPTY
        }
        private MenuSelect menuSelect = MenuSelect.PLAY;
        string[] menuTexts;
        

        enum OptionSelect{
            THEME,
            VOLUME
        }
        private OptionSelect optionSelect = OptionSelect.THEME;
        string[] optionTexts;

        public Menu(SpriteBatch spriteBatch, SpriteFont font, Texture2D texture)
        {
            this.spriteBatch = spriteBatch;
            this.font = font;
            this.texture = texture;

            int menuSelectLength = Enum.GetValues(typeof(MenuSelect)).Length - 1;
            List<string> tempMenuTexts = new List<string>(menuSelectLength);
            for (int menuItems = 0; menuItems < menuSelectLength; menuItems++)
            {
                tempMenuTexts.Add(((MenuSelect)menuItems).ToString().ToLower());
            }
            menuTexts = tempMenuTexts.ToArray();

            int menuOptionLength = Enum.GetValues(typeof(OptionSelect)).Length;
            List<string> tempOptionTexts = new List<string>(menuOptionLength);
            for (int menuItems = 0; menuItems < menuOptionLength; menuItems++)
            {
                tempOptionTexts.Add(((OptionSelect)menuItems).ToString().ToLower());
            }
            optionTexts = tempOptionTexts.ToArray();
        }

        //To add new new menu item just add an item into enum MenuSelect and in the class MainGame add the same thing to enum GameState
        public int DrawMainMenu(GraphicsDeviceManager graphicsDevice)
        {
            Color[] menuColors = new Color[menuTexts.Length];
            KeyboardState state = Keyboard.GetState();
            if (menuNav)
            {
                if (state.IsKeyDown(Keys.Down) && menuSelect != (MenuSelect)menuTexts.Length-1) menuSelect++;
                else if (state.IsKeyDown(Keys.Up) && menuSelect != (MenuSelect)0) menuSelect--;
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
                spriteBatch.Draw(texture, new Rectangle((graphicsDevice.PreferredBackBufferWidth - menuRectWidth) / 2, topOffset + menuLineSpacing * line, menuRectWidth, menuRectHeight), menuColors[line]);
                Vector2 textSize = font.MeasureString(menuTexts[line]);
                Vector2 textPos = new Vector2((graphicsDevice.PreferredBackBufferWidth - textSize.X) / 2, topOffset + menuRectHeight / 2 - textSize.Y / 3 + menuLineSpacing * line);
                spriteBatch.DrawString(font, menuTexts[line], textPos, Color.Black);
            }
            return (int)MenuSelect.EMPTY;
        }

        public void DrawOptionsMenu(GraphicsDeviceManager graphicsDevice)
        {
            Color[] menuColors = new Color[optionTexts.Length];
            KeyboardState state = Keyboard.GetState();
            if (menuNav)
            {
                if (state.IsKeyDown(Keys.Down) && optionSelect != (OptionSelect)optionTexts.Length-1) optionSelect++;
                else if (state.IsKeyDown(Keys.Up) && optionSelect != (OptionSelect)0) optionSelect--;
                menuNav = false;
            }

            if (state.IsKeyUp(Keys.Up) && state.IsKeyUp(Keys.Down))
            {
                menuNav = true;
            }

            for (int menuIndex = 0; menuIndex < optionTexts.Length; menuIndex++)
            {
                if ((int)optionSelect == menuIndex) menuColors[menuIndex] = Color.Gray;
                else menuColors[menuIndex] = Color.LightGray;
            }

            for (int line = 0; line < optionTexts.Length; line++)
            {
                spriteBatch.Draw(texture, new Rectangle((graphicsDevice.PreferredBackBufferWidth - optionRectWidth) / 2 + leftOffset, topOffset + menuLineSpacing * line, optionRectWidth, optionRectHeight), menuColors[line]);
                Vector2 textSize = font.MeasureString(optionTexts[line]);
                Vector2 textPos = new Vector2((graphicsDevice.PreferredBackBufferWidth - textSize.X) / 2 - leftOffset, topOffset + optionRectHeight / 2 - textSize.Y / 3 + menuLineSpacing * line);
                spriteBatch.DrawString(font, optionTexts[line], textPos, Color.Black);
            }
        }
    }
}