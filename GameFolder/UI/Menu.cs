using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using typatro.GameFolder.UI;
using Microsoft.Xna.Framework.Media;
using typatro.GameFolder.Services;


namespace typatro.GameFolder
{

    class Menu
    {
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

        public Menu()
        {     
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
                System.Diagnostics.Debug.WriteLine("Couldn't load settings " + e.Message);
            }
            
        }

        //To add new new menu item just add an item into enum MenuSelect and in the class MainGame add the same thing to enum GameState
        public int DrawMainMenu(bool gameSaved, bool gameFinished)
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
            if (state.IsKeyDown(Keys.Enter) && ((introFinished && introSkip) || totalSeconds >= 6) && !gameFinished) return (int)menuSelect;
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
            Vector2 textSize = MainGame.Gfx.logoFont.MeasureString(title);
            Vector2 start = new Vector2(MainGame.screenWidth / 2 - textSize.X/2, MainGame.screenHeight / 2 - textSize.Y/2);
            Vector2 end = new Vector2(MainGame.screenWidth / 2 - textSize.X/2, MainGame.screenHeight / 5 - textSize.Y/2);
            Vector2 current = start;

            if (!introFinished && totalSeconds >= 2 && totalSeconds <= 4){
                float t = (float)((totalSeconds - 2) / 2);
                current = Vector2.Lerp(start, end, t);
            }
            else if (totalSeconds > 4 || introFinished){
                current = end;
            }

            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.logoFont, title, current, ThemeColors.Text, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.6f);

            if (totalSeconds >= 4 || introFinished){
                float alpha = MathHelper.Clamp((float)((totalSeconds - 4) / 2), 0f, 1f);

                for (int line = 0; line < menuTexts.Length; line++){
                    Color rectColor = (line == 0 && !gameSaved ? ThemeColors.Background : menuColors[line]) * (introFinished?1:alpha);
                    Color textColor = ThemeColors.Text * (introFinished?1:alpha);

                    Rectangle menuItemPos = new Rectangle((MainGame.screenWidth - menuRectWidth) / 2 - SaveManager.size * 5/2, (int)(MainGame.screenHeight / 5 * 1.5f) +
                    (int)(MainGame.screenHeight / 6.5f) * line - SaveManager.size * 5/2, menuRectWidth + SaveManager.size * 5, menuRectHeight + SaveManager.size * 5);
                    MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, menuItemPos, rectColor);
                    if ((int)menuSelect == line)
                        ThemeColors.DrawGlowCorners(menuItemPos, ThemeColors.Text * (introFinished ? 1 : alpha));
                    if (menuItemPos.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
                    {
                        menuSelect = (MenuSelect)line;
                        if(mouseState.LeftButton == ButtonState.Pressed && !mousePressed && GameLogic.windowActive)
                        {
                            mousePressed = true;
                            if (!(!gameSaved && line == 0)) return line;
                        }
                    }

                    Vector2 lineSize = MainGame.Gfx.menuFont.MeasureString(menuTexts[line]);
                    Vector2 linePos = new Vector2((MainGame.screenWidth - lineSize.X) / 2,(int)(MainGame.screenHeight / 5 * 1.5f) + 
                    (int)(MainGame.screenHeight / 6.5f) * line + menuRectHeight / 2 - lineSize.Y / 3);

                    MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, menuTexts[line], linePos, textColor);
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
            int optionTopOffset = MainGame.screenHeight/5;
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
                        if (mouseState.LeftButton == ButtonState.Pressed && !mousePressed && GameLogic.windowActive)
                        {
                            mousePressed = true;
                            OptionDecrease(optionSelect);
                        }
                    }
                    Rectangle menuItemPosRight = new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset + optionRectWidth / 2, optionTopOffset + menuLineSpacing * line, optionRectWidth / 2, optionRectHeight);
                    if (menuItemPosRight.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
                    {
                        optionSelect = (OptionSelect)line;
                        if (mouseState.LeftButton == ButtonState.Pressed && !mousePressed && GameLogic.windowActive)
                        {
                            mousePressed = true;
                            OptionIncrease(optionSelect);
                        }
                    }
                    MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, menuItemPos, menuColors[line]);
                    if ((int)optionSelect == line) ThemeColors.DrawGlowCorners(menuItemPos, ThemeColors.Text);
                    textSize = MainGame.Gfx.menuFont.MeasureString(optionTexts[line]);
                    textPos = new Vector2(MainGame.screenWidth / 2 - textSize.X - 40, optionTopOffset + optionRectHeight / 2 - textSize.Y / 3 + menuLineSpacing * line);
                    MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, optionTexts[line], textPos, ThemeColors.Text);
                } 
            }
            int pos = optionTexts.Length - 1;
            menuItemPos = new Rectangle(50, 50, 50, 50);
            if (menuItemPos.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
            {
                optionSelect = (OptionSelect)pos;
                if (mouseState.LeftButton == ButtonState.Pressed && GameLogic.windowActive)
                {
                    mousePressed = true;
                    if (pos == optionTexts.Length - 1)
                    {
                        SaveManager.SaveSettings(SaveManager.theme, SaveManager.volume, SaveManager.size, SaveManager.fullscreen);
                        return true;
                    }
                }
            }
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, menuItemPos, menuColors[pos]);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "<", new Vector2(menuItemPos.X + 17, menuItemPos.Y + 7), ThemeColors.Text);

            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, themes[SaveManager.theme], new Vector2(MainGame.screenWidth/2 + optionRectWidth/2- MainGame.Gfx.menuFont.MeasureString(themes[SaveManager.theme]).X/2 + 20, optionTopOffset+20), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "<             >", new Vector2((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+10, optionTopOffset+20), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+20, optionTopOffset + menuLineSpacing+20, optionRectWidth-40, 40), ThemeColors.Background);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+25, optionTopOffset + menuLineSpacing+25, optionRectWidth-50, 30), ThemeColors.Background);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+25, optionTopOffset + menuLineSpacing+25, (optionRectWidth-50)/10*SaveManager.volume, 30), ThemeColors.Selected);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, sizes[SaveManager.size], new Vector2(MainGame.screenWidth/2 + optionRectWidth/2- MainGame.Gfx.menuFont.MeasureString(sizes[SaveManager.size]).X/2+20, optionTopOffset+20 + menuLineSpacing*2), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, "<               >", new Vector2((MainGame.screenWidth - optionRectWidth) / 2 + leftOffset+10, optionTopOffset+20 + menuLineSpacing*2), ThemeColors.Text);
            string fullScr = SaveManager.fullscreen == 0?"off":"on";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, fullScr, new Vector2(MainGame.screenWidth / 2 + optionRectWidth / 2 - MainGame.Gfx.menuFont.MeasureString(fullScr).X / 2 +20, optionTopOffset + 20 + menuLineSpacing * 3), ThemeColors.Text);
            return false;
        }
        private void ChangeScreenSize(int size)
        {
            (int width, int height)[] resolutions = { (800, 600), (1152, 648), (1280, 720) };
            if (size < 0 || size >= resolutions.Length) return;
            MainGame.screenWidth = resolutions[size].width;
            MainGame.screenHeight = resolutions[size].height;
            MainGame.graphics.PreferredBackBufferWidth = MainGame.screenWidth;
            MainGame.graphics.PreferredBackBufferHeight = MainGame.screenHeight;
            MainGame.graphics.ApplyChanges();
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
                        UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Sun);
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
                        UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Sun);
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