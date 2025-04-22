using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using typatro.GameFolder.UI;


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

        bool menuNav = true, optionNav = true;
        enum MenuSelect
        {
            play,
            options,
            exit,
            empty
        }
        private MenuSelect menuSelect = MenuSelect.play;
        string[] menuTexts;
        

        enum OptionSelect{
            theme,
            volume
        }
        private OptionSelect optionSelect = OptionSelect.theme;
        string[] optionTexts;

        public enum Themes{
            pink,
            red,
            black
        }
        public Themes selectedTheme = Themes.pink;
        string[] themeTexts;
        public int volume = 10;

        public Menu(SpriteBatch spriteBatch, SpriteFont font, Texture2D texture)
        {
            this.spriteBatch = spriteBatch;
            this.font = font;
            this.texture = texture;

            int menuSelectLength = Enum.GetValues(typeof(MenuSelect)).Length - 1;
            List<string> tempMenuTexts = new List<string>(menuSelectLength);
            for (int menuItems = 0; menuItems < menuSelectLength; menuItems++)
            {
                tempMenuTexts.Add(((MenuSelect)menuItems).ToString());
            }
            menuTexts = tempMenuTexts.ToArray();

            int menuOptionLength = Enum.GetValues(typeof(OptionSelect)).Length;
            List<string> tempOptionTexts = new List<string>(menuOptionLength);
            for (int menuItems = 0; menuItems < menuOptionLength; menuItems++)
            {
                tempOptionTexts.Add(((OptionSelect)menuItems).ToString());
            }
            optionTexts = tempOptionTexts.ToArray();

            int themesLength = Enum.GetValues(typeof(Themes)).Length;
            List<string> tempThemeTexts = new List<string>(themesLength);
            for (int menuItems = 0; menuItems < themesLength; menuItems++)
            {
                tempThemeTexts.Add(((Themes)menuItems).ToString());
            }
            themeTexts = tempThemeTexts.ToArray();

            if(Enum.TryParse(SettingsManager.theme, out Themes theme)){
                selectedTheme = theme;
            }
            volume = SettingsManager.volume;
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
                if ((int)menuSelect == menuIndex) menuColors[menuIndex] = ThemeColors.Selected;
                else menuColors[menuIndex] = ThemeColors.NotSelected;
            }
            if (state.IsKeyDown(Keys.Enter)) return (int)menuSelect;

            for (int line = 0; line < menuTexts.Length; line++)
            {
                spriteBatch.Draw(texture, new Rectangle((graphicsDevice.PreferredBackBufferWidth - menuRectWidth) / 2, topOffset + menuLineSpacing * line, menuRectWidth, menuRectHeight), menuColors[line]);
                Vector2 textSize = font.MeasureString(menuTexts[line]);
                Vector2 textPos = new Vector2((graphicsDevice.PreferredBackBufferWidth - textSize.X) / 2, topOffset + menuRectHeight / 2 - textSize.Y / 3 + menuLineSpacing * line);
                spriteBatch.DrawString(font, menuTexts[line], textPos, ThemeColors.Text);
            }
            return (int)MenuSelect.empty;
        }

        public void DrawOptionsMenu(GraphicsDeviceManager graphicsDevice)
        {
            Color[] menuColors = new Color[optionTexts.Length];
            KeyboardState state = Keyboard.GetState();
            if (menuNav)
            {
                if (state.IsKeyDown(Keys.Down) && optionSelect != (OptionSelect)optionTexts.Length-1){
                    optionSelect++;
                    menuNav = false;
                }
                else if (state.IsKeyDown(Keys.Up) && optionSelect != (OptionSelect)0){
                    optionSelect--;
                    menuNav = false;
                }
            }

            if (state.IsKeyUp(Keys.Up) && state.IsKeyUp(Keys.Down))
            {
                menuNav = true;
            }

            for (int menuIndex = 0; menuIndex < optionTexts.Length; menuIndex++)
            {
                if ((int)optionSelect == menuIndex) menuColors[menuIndex] = ThemeColors.Selected;
                else menuColors[menuIndex] = ThemeColors.NotSelected;
            }

            if(optionNav){
                if(state.IsKeyDown(Keys.Left)){
                    if(optionSelect == OptionSelect.theme && selectedTheme !=(Themes)0){
                        selectedTheme--;
                        optionNav = false;
                    }
                    if(optionSelect == OptionSelect.volume && volume > 0){
                        volume--;
                        optionNav = false;
                    }
                }
                if(state.IsKeyDown(Keys.Right)){
                    if(optionSelect == OptionSelect.theme && selectedTheme != (Themes)themeTexts.Length-1){
                        selectedTheme++;
                        optionNav = false;
                    }
                    if(optionSelect == OptionSelect.volume && volume < 10){
                        volume++;
                        optionNav = false;
                    }
                }
            }

            if (state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right))
            {
                optionNav = true;
            }

            for (int line = 0; line < optionTexts.Length; line++)
            {
                spriteBatch.Draw(texture, new Rectangle((graphicsDevice.PreferredBackBufferWidth - optionRectWidth) / 2 + leftOffset, topOffset + menuLineSpacing * line, optionRectWidth, optionRectHeight), menuColors[line]);
                Vector2 textSize = font.MeasureString(optionTexts[line]);
                Vector2 textPos = new Vector2((graphicsDevice.PreferredBackBufferWidth - textSize.X) / 2 - leftOffset, topOffset + optionRectHeight / 2 - textSize.Y / 3 + menuLineSpacing * line);
                spriteBatch.DrawString(font, optionTexts[line], textPos, ThemeColors.Text);
            }
            spriteBatch.DrawString(font, selectedTheme.ToString(), new Vector2((graphicsDevice.PreferredBackBufferWidth - optionRectWidth) / 2 + leftOffset+85, topOffset+20), ThemeColors.Text);
            spriteBatch.DrawString(font, "<             >", new Vector2((graphicsDevice.PreferredBackBufferWidth - optionRectWidth) / 2 + leftOffset+10, topOffset+20), ThemeColors.Text);
            spriteBatch.Draw(texture, new Rectangle((graphicsDevice.PreferredBackBufferWidth - optionRectWidth) / 2 + leftOffset+20, topOffset + menuLineSpacing+20, optionRectWidth-40, 40), ThemeColors.Text);
            spriteBatch.Draw(texture, new Rectangle((graphicsDevice.PreferredBackBufferWidth - optionRectWidth) / 2 + leftOffset+25, topOffset + menuLineSpacing+25, optionRectWidth-50, 30), ThemeColors.NotSelected);
            spriteBatch.Draw(texture, new Rectangle((graphicsDevice.PreferredBackBufferWidth - optionRectWidth) / 2 + leftOffset+25, topOffset + menuLineSpacing+25, (optionRectWidth-50)/10*volume, 30), ThemeColors.Selected);
        }
    }
}