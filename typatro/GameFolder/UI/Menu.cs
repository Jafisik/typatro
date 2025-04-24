using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using typatro.GameFolder.UI;
using System.Linq;
using Microsoft.Xna.Framework.Media;
using Microsoft.VisualBasic;


namespace typatro.GameFolder
{

    class Menu
    {
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D texture;
        int menuLineSpacing = 100, topOffset = 210;
        int menuRectWidth = 400, menuRectHeight = 80;
        int leftOffset = 140, optionRectWidth = 300, optionRectHeight = 80;

        bool menuNav = true, optionNav = true;
        enum MenuSelect
        {
            load,
            start,
            options,
            exit,
            empty
        }
        private MenuSelect menuSelect = MenuSelect.start;
        string[] menuTexts = new string[] { "continue", "new game", "options", "exit"};
        

        enum OptionSelect{
            theme,
            volume
        }
        private OptionSelect optionSelect = OptionSelect.theme;
        string[] optionTexts = new string[] { "theme", "volume"};

        public string[] themes = new string[]{ "black", "pink", "blue", "red"  };

        public Menu(SpriteBatch spriteBatch, SpriteFont font, Texture2D texture)
        {
            this.spriteBatch = spriteBatch;
            this.font = font;
            this.texture = texture;
            
            int[] settings = SaveManager.LoadSettings();
            SaveManager.theme = settings[0];
            SaveManager.volume = settings[1];
        }

        //To add new new menu item just add an item into enum MenuSelect and in the class MainGame add the same thing to enum GameState
        public int DrawMainMenu(GraphicsDeviceManager graphicsDevice, bool gameSaved)
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

            string title = "GLYPHORA";
            Vector2 screenCenter = new Vector2(275, 100);

            spriteBatch.DrawString(font, title, screenCenter, ThemeColors.Text, 0f, new Vector2(), 2f, SpriteEffects.None, 0.6f);
            for (int line = gameSaved?0:1; line < menuTexts.Length; line++)
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
                    if(optionSelect == OptionSelect.theme && SaveManager.theme != 0){
                        SaveManager.theme--;
                        optionNav = false;
                    }
                    if(optionSelect == OptionSelect.volume && SaveManager.volume > 0){
                        SaveManager.volume--;
                        MediaPlayer.Volume = SaveManager.volume / 10f;
                        optionNav = false;
                    }
                }
                if(state.IsKeyDown(Keys.Right)){
                    if(optionSelect == OptionSelect.theme && SaveManager.theme != themes.Length-1){
                        SaveManager.theme++;
                        optionNav = false;
                    }
                    if(optionSelect == OptionSelect.volume && SaveManager.volume < 10){
                        SaveManager.volume++;
                        MediaPlayer.Volume = SaveManager.volume / 10f;
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
            spriteBatch.DrawString(font, themes[SaveManager.theme], new Vector2((graphicsDevice.PreferredBackBufferWidth - optionRectWidth) / 2 + leftOffset+85, topOffset+20), ThemeColors.Text);
            spriteBatch.DrawString(font, "<             >", new Vector2((graphicsDevice.PreferredBackBufferWidth - optionRectWidth) / 2 + leftOffset+10, topOffset+20), ThemeColors.Text);
            spriteBatch.Draw(texture, new Rectangle((graphicsDevice.PreferredBackBufferWidth - optionRectWidth) / 2 + leftOffset+20, topOffset + menuLineSpacing+20, optionRectWidth-40, 40), ThemeColors.Background);
            spriteBatch.Draw(texture, new Rectangle((graphicsDevice.PreferredBackBufferWidth - optionRectWidth) / 2 + leftOffset+25, topOffset + menuLineSpacing+25, optionRectWidth-50, 30), ThemeColors.Background);
            spriteBatch.Draw(texture, new Rectangle((graphicsDevice.PreferredBackBufferWidth - optionRectWidth) / 2 + leftOffset+25, topOffset + menuLineSpacing+25, (optionRectWidth-50)/10*SaveManager.volume, 30), ThemeColors.Selected);
        }
    }
}