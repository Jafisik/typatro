using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;


namespace typatro.GameFolder;

public class MainGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    SpriteFont font;
    
    List<char> writtenText = new List<char>();
    string neededText = "hello world this is pretty sick";

    List<int> diffIndexes = new List<int>();
    Writer writer;

    enum GameState
    {
        MENU,
        OPTIONS,
        PLAY
    }
    private GameState gameState = GameState.MENU;
    enum MenuSelect
    {
        PLAY,
        OPTIONS,
        EXIT
    }
    private MenuSelect menuSelect = MenuSelect.PLAY;
    bool menuNav = true;



    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("Fonts/pixelFont");
        writer = new(_spriteBatch, font, diffIndexes, writtenText);
    }

    protected override void Update(GameTime gameTime)
    {
        if(gameState == GameState.PLAY)
        {
            writer.ReadKeyboardInput(gameTime);
            writer.UpdateDiffIndexes(neededText);
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();

        if(gameState == GameState.MENU)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            int rectWidth = 400, rectHeight = 80;
            Color menu1 = Color.LightGray, menu2 = Color.LightGray, menu3 = Color.LightGray;
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Down))
            {
                if (menuNav)
                {
                    menuNav = false;
                    if(menuSelect != MenuSelect.EXIT)
                    {
                        menuSelect++;
                    }
                    
                }
            }else if(state.IsKeyDown(Keys.Up))
            {
                if (menuNav)
                {
                    menuNav = false;
                    if (menuSelect != MenuSelect.PLAY)
                    {
                        menuSelect--;
                    }
                }
            }
            if(state.IsKeyUp(Keys.Up) && state.IsKeyUp(Keys.Down))
            {
                menuNav = true;
            }
            switch (menuSelect)
            {
                case (MenuSelect.PLAY):
                    menu1 = Color.Gray;
                    menu2 = Color.LightGray;
                    menu3 = Color.LightGray;
                    if (state.IsKeyDown(Keys.Enter)) gameState = GameState.PLAY;
                    break;
                case (MenuSelect.OPTIONS):
                    menu1 = Color.LightGray;
                    menu2 = Color.Gray;
                    menu3 = Color.LightGray;
                    break;
                case (MenuSelect.EXIT):
                    menu1 = Color.LightGray;
                    menu2 = Color.LightGray;
                    menu3 = Color.Gray;
                    if (state.IsKeyDown(Keys.Enter)) Exit();
                    break;
            }
            
            _spriteBatch.Draw(texture, new Rectangle((_graphics.PreferredBackBufferWidth - rectWidth) / 2, 80, rectWidth, rectHeight), menu1);
            _spriteBatch.Draw(texture, new Rectangle((_graphics.PreferredBackBufferWidth - rectWidth) / 2, 80+100, rectWidth, rectHeight), menu2);
            _spriteBatch.Draw(texture, new Rectangle((_graphics.PreferredBackBufferWidth - rectWidth) / 2, 80+200, rectWidth, rectHeight), menu3);
            
        }

        else if(gameState == GameState.PLAY)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) gameState = GameState.MENU;
            writer.WriteText(neededText, Color.Gray);
            writer.WriteText(writtenText.ToArray(), Color.Black);

            if (neededText == new string(writtenText.ToArray()))
            {
                writer.WriteText("You win", Color.Red, 5);
            }
            writer.WriteText("Correct: " + (writtenText.Count > 0 ? ((1f - (diffIndexes.Count / (float)writtenText.Count)) * 100).ToString("0.00") + "%" : "0%"), Color.Black, 6);
        }
       
        _spriteBatch.End();
        base.Draw(gameTime);
    }




    
}
