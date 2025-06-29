using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using typatro.GameFolder.UI;
using System.Linq;
using Microsoft.Xna.Framework.Media;
using Microsoft.VisualBasic;
using System.Diagnostics;


namespace typatro.GameFolder
{

    class Menu
    {
        MainGame.Gfx gfx;
        int menuLineSpacing = 100;
        int menuRectWidth = 400, menuRectHeight = 80;
        int leftOffset = 170, optionRectWidth = 300, optionRectHeight = 80;

        bool menuNav = true, optionNav = true, introFinished = false, introSkip = false, mousePressed;
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
            fullScreen,
            back
        }
        private OptionSelect optionSelect = OptionSelect.theme;
        string[] optionTexts = new string[] { "theme", "volume", "size", "fullscreen", "back"};

        public string[] themes = new string[]{ "green", "pink", "blue", "red"  };

        public string[] sizes = new string[]{ "800/600", "1152/648", "1280/720"};
        public bool fullscreen = false;

        public Menu(MainGame.Gfx gfx)
        {
            this.gfx = gfx;
            
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
        public int DrawMainMenu(bool gameSaved, bool gameWon)
        {
            Color[] menuColors = new Color[menuTexts.Length];
            KeyboardState state = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
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
            
            double totalSeconds = MainGame.time.TotalGameTime.TotalSeconds;
            if (state.IsKeyDown(Keys.Enter) && ((introFinished && introSkip) || totalSeconds >= 6) && !gameWon) return (int)menuSelect;
            if(!introSkip && (state.IsKeyDown(Keys.Enter) || mouseState.LeftButton == ButtonState.Pressed)){
                introFinished = true;
                mousePressed = true;
            }
            if(introFinished && state.IsKeyUp(Keys.Enter)){
                introSkip = true;
            }
            if(mouseState.LeftButton == ButtonState.Released){
                mousePressed = false;
            }

                string title = "GLYPHORA";
            Vector2 start = new Vector2(MainGame.screenWidth / 2f, MainGame.screenHeight / 2f);
            Vector2 end = new Vector2(MainGame.screenWidth / 2f, MainGame.screenHeight / 5f);
            Vector2 current = start;

            if (totalSeconds >= 2 && totalSeconds <= 4){
                float t = (float)((totalSeconds - 2) / 2);
                current = Vector2.Lerp(start, end, t);
            }
            else if (totalSeconds > 4){
                current = end;
            }

            Vector2 textSize = gfx.menuFont.MeasureString(title);
            Vector2 drawPos = current - textSize;

            if(!introFinished) gfx.spriteBatch.DrawString(gfx.menuFont, title, drawPos, ThemeColors.Text, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.6f);
            else gfx.spriteBatch.DrawString(gfx.menuFont, title, end-textSize, ThemeColors.Text, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.6f);

            if (totalSeconds >= 4 || introFinished){
                float alpha = MathHelper.Clamp((float)((totalSeconds - 4) / 2), 0f, 1f);

                for (int line = 0; line < menuTexts.Length; line++){
                    Color rectColor = (line == 0 && !gameSaved ? ThemeColors.Background : menuColors[line]) * (introFinished?1:alpha);
                    Color textColor = ThemeColors.Text * (introFinished?1:alpha);

                    Rectangle menuItemPos = new Rectangle((MainGame.screenWidth - menuRectWidth) / 2, (int)(MainGame.screenHeight / 5 * 1.5f) +
                    (int)(MainGame.screenHeight / 6.5f) * line, menuRectWidth, menuRectHeight);
                    gfx.spriteBatch.Draw(gfx.texture, menuItemPos,rectColor);
                    if (menuItemPos.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
                    {
                        menuSelect = (MenuSelect)line;
                        if(mouseState.LeftButton == ButtonState.Pressed && !mousePressed){
                            mousePressed = true;
                            return line;
                        }
                    }

                    Vector2 lineSize = gfx.menuFont.MeasureString(menuTexts[line]);
                    Vector2 linePos = new Vector2((MainGame.screenWidth - lineSize.X) / 2,(int)(MainGame.screenHeight / 5 * 1.5f) + 
                    (int)(MainGame.screenHeight / 6.5f) * line + menuRectHeight / 2 - lineSize.Y / 3);

                    gfx.spriteBatch.DrawString(gfx.menuFont, menuTexts[line], linePos, textColor);
                }
            }
            if(totalSeconds >= 5) introFinished = true;
            
            return (int)MenuSelect.empty;
        }

        public bool DrawOptionsMenu()
        {
            Color[] menuColors = new Color[optionTexts.Length];
            KeyboardState state = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
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

            if (mouseState.LeftButton == ButtonState.Released)
            {
                mousePressed = false;
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
                    OptionDecrease(optionSelect);
                }
                if(state.IsKeyDown(Keys.Right)){
                    OptionIncrease(optionSelect);
                }
            }

            if (state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right))
            {
                optionNav = true;
            }
            int optionTopOffset = 100;
            Vector2 textPos, textSize;
            Rectangle menuItemPos;
            for (int line = 0; line < optionTexts.Length; line++)
            {
                if (line != optionTexts.Length - 1)
                {
                    menuItemPos = new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset, optionTopOffset + menuLineSpacing * line, optionRectWidth, optionRectHeight);
                    Rectangle menuItemPosLeft = new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset, optionTopOffset + menuLineSpacing * line, optionRectWidth/2, optionRectHeight);
                    if (menuItemPosLeft.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
                    {
                        optionSelect = (OptionSelect)line;
                        if (mouseState.LeftButton == ButtonState.Pressed && !mousePressed)
                        {
                            mousePressed = true;
                            OptionDecrease(optionSelect);
                            if (line == optionTexts.Length - 1) return true;
                        }
                    }
                    Rectangle menuItemPosRight = new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset + optionRectWidth / 2, optionTopOffset + menuLineSpacing * line, optionRectWidth / 2, optionRectHeight);
                    if (menuItemPosRight.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
                    {
                        optionSelect = (OptionSelect)line;
                        if (mouseState.LeftButton == ButtonState.Pressed && !mousePressed)
                        {
                            mousePressed = true;
                            OptionIncrease(optionSelect);
                            if (line == optionTexts.Length - 1) return true;
                        }
                    }
                    gfx.spriteBatch.Draw(gfx.texture, menuItemPos, menuColors[line]);
                    textSize = gfx.menuFont.MeasureString(optionTexts[line]);
                    textPos = new Vector2(MainGame.screenWidth / 2 - textSize.X - 40, optionTopOffset + optionRectHeight / 2 - textSize.Y / 3 + menuLineSpacing * line);
                    gfx.spriteBatch.DrawString(gfx.menuFont, optionTexts[line], textPos, ThemeColors.Text);
                } 
            }
            int pos = optionTexts.Length - 1;
            menuItemPos = new Rectangle(50, 50, 50, 50);
            if (menuItemPos.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
            {
                optionSelect = (OptionSelect)pos;
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    mousePressed = true;
                    if (pos == optionTexts.Length - 1) return true;
                }
            }
            gfx.spriteBatch.Draw(gfx.texture, menuItemPos, menuColors[pos]);
            gfx.spriteBatch.DrawString(gfx.menuFont, "<", new Vector2(menuItemPos.X + 17, menuItemPos.Y + 7), ThemeColors.Text);

            gfx.spriteBatch.DrawString(gfx.menuFont, themes[SaveManager.theme], new Vector2(MainGame.screenWidth/2 + optionRectWidth/2- gfx.menuFont.MeasureString(themes[SaveManager.theme]).X/2 + 20, optionTopOffset+20), ThemeColors.Text);
            gfx.spriteBatch.DrawString(gfx.menuFont, "<             >", new Vector2((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+10, optionTopOffset+20), ThemeColors.Text);
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+20, optionTopOffset + menuLineSpacing+20, optionRectWidth-40, 40), ThemeColors.Background);
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+25, optionTopOffset + menuLineSpacing+25, optionRectWidth-50, 30), ThemeColors.Background);
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+25, optionTopOffset + menuLineSpacing+25, (optionRectWidth-50)/10*SaveManager.volume, 30), ThemeColors.Selected);
            gfx.spriteBatch.DrawString(gfx.menuFont, sizes[SaveManager.size], new Vector2(MainGame.screenWidth/2 + optionRectWidth/2- gfx.menuFont.MeasureString(sizes[SaveManager.size]).X/2+20, optionTopOffset+20 + menuLineSpacing*2), ThemeColors.Text);
            gfx.spriteBatch.DrawString(gfx.menuFont, "<               >", new Vector2((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+10, optionTopOffset+20 + menuLineSpacing*2), ThemeColors.Text);
            string fullScr = SaveManager.fullscreen == 0?"off":"on";
            gfx.spriteBatch.DrawString(gfx.menuFont, fullScr, new Vector2(MainGame.screenWidth / 2 + optionRectWidth / 2 - gfx.menuFont.MeasureString(fullScr).X / 2 +20, optionTopOffset + 20 + menuLineSpacing * 3), ThemeColors.Text);
            return false;
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

        private void OptionDecrease(OptionSelect option)
        {
            switch (option)
            {
                case OptionSelect.theme:
                    if(SaveManager.theme != 0)
                    {
                        SaveManager.theme--;
                        optionNav = false;
                        if (!GameLogic.achievmentBools["SUN"])
                        {
                            GameLogic.achievmentBools["SUN"] = true;
                            GameLogic.writeAchievment = true;
                        }
                    }
                    break;
                case OptionSelect.volume:
                    if (SaveManager.volume > 0)
                    {
                        SaveManager.volume--;
                        MediaPlayer.Volume = SaveManager.volume / 10f;
                        optionNav = false;
                    }
                    break;
                case OptionSelect.size:
                    if (SaveManager.size != 0)
                    {
                        SaveManager.size--;
                        ChangeScreenSize(SaveManager.size);
                        optionNav = false;
                    }
                    break;
                case OptionSelect.fullScreen:
                    if (SaveManager.fullscreen == 0)
                    {
                        SaveManager.fullscreen = 1;
                        MainGame.graphics.IsFullScreen = true;
                    }
                    else
                    {
                        SaveManager.fullscreen = 0;
                        MainGame.graphics.IsFullScreen = false;
                    }
                    MainGame.graphics.ApplyChanges();
                    optionNav = false;
                    break;
                case OptionSelect.back:
                    optionSelect = OptionSelect.theme;
                    optionNav = false;
                    break;
                default: break;
            }
        }

        private void OptionIncrease(OptionSelect option)
        {
            switch (option)
            {
                case OptionSelect.theme:
                    if(SaveManager.theme != themes.Length - 1)
                    {
                        SaveManager.theme++;
                        optionNav = false;
                        if (!GameLogic.achievmentBools["SUN"])
                        {
                            GameLogic.achievmentBools["SUN"] = true;
                            GameLogic.writeAchievment = true;
                        }
                    }
                    break;
                case OptionSelect.volume:
                    if(SaveManager.volume < 10)
                    {
                        SaveManager.volume++;
                        MediaPlayer.Volume = SaveManager.volume / 10f;
                        optionNav = false;
                    }
                    break;
                case OptionSelect.size:
                    if(SaveManager.size != sizes.Length - 1){
                        SaveManager.size++;
                        ChangeScreenSize(SaveManager.size);
                        optionNav = false;
                    }
                    break;
                case OptionSelect.fullScreen:
                    if (SaveManager.fullscreen == 0)
                    {
                        SaveManager.fullscreen = 1;
                        MainGame.graphics.IsFullScreen = true;
                    }
                    else
                    {
                        SaveManager.fullscreen = 0;
                        MainGame.graphics.IsFullScreen = false;
                    }
                    MainGame.graphics.ApplyChanges();
                    optionNav = false;
                    break;
                case OptionSelect.back:
                    optionSelect = OptionSelect.theme;
                    optionNav = false;
                    break;
                default: break;
            }
        }
    }
}