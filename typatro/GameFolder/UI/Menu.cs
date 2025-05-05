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
        int menuLineSpacing = 100;
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
            volume,
            size,
            fullScreen
        }
        private OptionSelect optionSelect = OptionSelect.theme;
        string[] optionTexts = new string[] { "theme", "volume", "size", "fullscreen"};

        public string[] themes = new string[]{ "black", "pink", "blue", "red"  };

        public string[] sizes = new string[]{ "800/600", "1152/648", "1280/720"};
        public bool fullscreen = false;

        public Menu(SpriteBatch spriteBatch, SpriteFont font, Texture2D texture)
        {
            this.spriteBatch = spriteBatch;
            this.font = font;
            this.texture = texture;
            
            int[] settings = SaveManager.LoadSettings();
            try{
                SaveManager.theme = settings[0];
                SaveManager.volume = settings[1];
                SaveManager.size = settings[2];
                SaveManager.fullscreen = settings[3];
                ChangeScreenSize(SaveManager.size);
                if (SaveManager.fullscreen == 1) MainGame.graphics.IsFullScreen = true;
                else MainGame.graphics.IsFullScreen = false;
                MainGame.graphics.ApplyChanges();
            } catch(Exception e){
                Console.WriteLine("Couldn't load settings " + e.Message);
            }
            
        }

        //To add new new menu item just add an item into enum MenuSelect and in the class MainGame add the same thing to enum GameState
        public int DrawMainMenu(bool gameSaved)
        {
            Color[] menuColors = new Color[menuTexts.Length];
            KeyboardState state = Keyboard.GetState();
            if (menuNav)
            {
                if (state.IsKeyDown(Keys.Down) && menuSelect != (MenuSelect)menuTexts.Length-1) menuSelect++;
                else if (state.IsKeyDown(Keys.Up) && menuSelect != (MenuSelect)(gameSaved?0:1)) menuSelect--;
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
            Vector2 screenCenter = new Vector2(MainGame.screenWidth/2, MainGame.screenHeight/5);

            spriteBatch.DrawString(font, title, screenCenter-font.MeasureString(title), ThemeColors.Text, 0f, new Vector2(), 2f, SpriteEffects.None, 0.6f);
            for (int line = 0; line < menuTexts.Length; line++)
            {
                spriteBatch.Draw(texture, new Rectangle((MainGame.screenWidth - menuRectWidth) / 2, (int)(MainGame.screenHeight/5*1.5) + (int)(MainGame.screenHeight/6.5) * line, menuRectWidth, menuRectHeight), line == 0 && !gameSaved?ThemeColors.Background:menuColors[line]);
                Vector2 textSize = font.MeasureString(menuTexts[line]);
                Vector2 textPos = new Vector2((MainGame.screenWidth - textSize.X) / 2, (int)(MainGame.screenHeight/5*1.5) + (int)(MainGame.screenHeight/6.5) * line + menuRectHeight/2-textSize.Y/3);
                spriteBatch.DrawString(font, menuTexts[line], textPos, ThemeColors.Text);
            }
            return (int)MenuSelect.empty;
        }

        public void DrawOptionsMenu()
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
                    if(optionSelect == OptionSelect.size && SaveManager.size != 0){
                        SaveManager.size--;
                        ChangeScreenSize(SaveManager.size);
                        optionNav = false;
                    }
                    if(optionSelect == OptionSelect.fullScreen){
                        if(SaveManager.fullscreen == 0){
                            SaveManager.fullscreen = 1;
                            MainGame.graphics.IsFullScreen = true;
                        }
                        else{
                            SaveManager.fullscreen = 0;
                            MainGame.graphics.IsFullScreen = false;
                        }
                        MainGame.graphics.ApplyChanges();
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
                    if(optionSelect == OptionSelect.size && SaveManager.size != sizes.Length-1){
                        SaveManager.size++;
                        ChangeScreenSize(SaveManager.size);
                        optionNav = false;
                    }
                    if(optionSelect == OptionSelect.fullScreen){
                        if(SaveManager.fullscreen == 0){
                            SaveManager.fullscreen = 1;
                            MainGame.graphics.IsFullScreen = true;
                        }
                        else{
                            SaveManager.fullscreen = 0;
                            MainGame.graphics.IsFullScreen = false;
                        }
                        MainGame.graphics.ApplyChanges();
                        optionNav = false;
                    }
                }
            }

            if (state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right))
            {
                optionNav = true;
            }
            int optionTopOffset = 100;
            for (int line = 0; line < optionTexts.Length; line++)
            {
                spriteBatch.Draw(texture, new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset, optionTopOffset + menuLineSpacing * line, optionRectWidth, optionRectHeight), menuColors[line]);
                Vector2 textSize = font.MeasureString(optionTexts[line]);
                Vector2 textPos = new Vector2(MainGame.screenWidth/2 - textSize.X - 40, optionTopOffset + optionRectHeight / 2 - textSize.Y / 3 + menuLineSpacing * line);
                spriteBatch.DrawString(font, optionTexts[line], textPos, ThemeColors.Text);
            }
            spriteBatch.DrawString(font, themes[SaveManager.theme], new Vector2(MainGame.screenWidth/2 + optionRectWidth/2- font.MeasureString(themes[SaveManager.theme]).X/2-10, optionTopOffset+20), ThemeColors.Text);
            spriteBatch.DrawString(font, "<             >", new Vector2((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+10, optionTopOffset+20), ThemeColors.Text);
            spriteBatch.Draw(texture, new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+20, optionTopOffset + menuLineSpacing+20, optionRectWidth-40, 40), ThemeColors.Background);
            spriteBatch.Draw(texture, new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+25, optionTopOffset + menuLineSpacing+25, optionRectWidth-50, 30), ThemeColors.Background);
            spriteBatch.Draw(texture, new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+25, optionTopOffset + menuLineSpacing+25, (optionRectWidth-50)/10*SaveManager.volume, 30), ThemeColors.Selected);
            spriteBatch.DrawString(font, sizes[SaveManager.size], new Vector2(MainGame.screenWidth/2 + optionRectWidth/2- font.MeasureString(sizes[SaveManager.size]).X/2-10, optionTopOffset+20 + menuLineSpacing*2), ThemeColors.Text);
            spriteBatch.DrawString(font, "<             >", new Vector2((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+10, optionTopOffset+20 + menuLineSpacing*2), ThemeColors.Text);
            string fullScr = SaveManager.fullscreen == 0?"off":"on";
            spriteBatch.DrawString(font, fullScr, new Vector2(MainGame.screenWidth/2 + optionRectWidth/2- font.MeasureString(fullScr).X/2-10, optionTopOffset+20 + menuLineSpacing*3),ThemeColors.Text);
        }
        private void ChangeScreenSize(int size){
            switch(size){
                case 0:
                    MainGame.screenWidth = 800;
                    MainGame.screenHeight = 600;
                    MainGame.graphics.PreferredBackBufferWidth = MainGame.screenWidth;
                    MainGame.graphics.PreferredBackBufferHeight = MainGame.screenHeight;
                    MainGame.graphics.ApplyChanges();
                    break;
                case 1:
                    MainGame.screenWidth = 1152;
                    MainGame.screenHeight = 648;
                    MainGame.graphics.PreferredBackBufferWidth = MainGame.screenWidth;
                    MainGame.graphics.PreferredBackBufferHeight = MainGame.screenHeight;
                    MainGame.graphics.ApplyChanges();
                    break;
                case 2:
                    MainGame.screenWidth = 1280;
                    MainGame.screenHeight = 720;
                    MainGame.graphics.PreferredBackBufferWidth = MainGame.screenWidth;
                    MainGame.graphics.PreferredBackBufferHeight = MainGame.screenHeight;
                    MainGame.graphics.ApplyChanges();
                    break;
            }
        }
    }
}