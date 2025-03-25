using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace typatro.GameFolder;

public class MainGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    SpriteFont font;
    
    List<char> writtenText = new List<char>();
    string neededText = "";
    List<string> jsonStrings;

    List<int> diffIndexes = new List<int>();
    Writer writer;

    enum GameState
    {
        MENU,
        OPTIONS,
        PLAY
    }
    enum MenuSelect
    {
        PLAY,
        OPTIONS,
        EXIT
    }
    private MenuSelect menuSelect = MenuSelect.PLAY;
    bool menuNav = true;
    private GameState gameState = GameState.MENU;
    Menu menu;



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
        Random rand = new Random();
        string jsonText = File.ReadAllText("Content/wordlist.json");
        jsonStrings = JsonSerializer.Deserialize<List<string>>(jsonText);
        for(int i = 0; i < 10; i++){
            neededText += jsonStrings[rand.Next(0,jsonStrings.Count)] + " ";
        }
        neededText += jsonStrings[rand.Next(0,jsonStrings.Count)];

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("Fonts/pixelFont");
        writer = new(_spriteBatch, font, diffIndexes, writtenText);
        //menu = new Menu(_spriteBatch,font, GraphicsDevice, gameState);
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
            
            int menuLineSpacing = 100, topOffset = 80, line = 0;
            int rectWidth = 400, rectHeight = 80;

            _spriteBatch.Draw(texture, new Rectangle((_graphics.PreferredBackBufferWidth - rectWidth) / 2, topOffset + menuLineSpacing * line, rectWidth, rectHeight), menu1);
            string menuText = "Play";
            Vector2 textSize = font.MeasureString(menuText);
            Vector2 textPos = new Vector2((_graphics.PreferredBackBufferWidth - textSize.X*2) / 2, 80 + rectHeight/2-textSize.Y/2 + menuLineSpacing * line++);
            _spriteBatch.DrawString(font, menuText, textPos, Color.Black, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

            _spriteBatch.Draw(texture, new Rectangle((_graphics.PreferredBackBufferWidth - rectWidth) / 2, 80 + menuLineSpacing * line, rectWidth, rectHeight), menu2);
            menuText = "Options";
            textSize = font.MeasureString(menuText);
            textPos = new Vector2((_graphics.PreferredBackBufferWidth - textSize.X*2) / 2, 80 + rectHeight/2-textSize.Y/2 + menuLineSpacing * line++);
            _spriteBatch.DrawString(font, menuText, textPos, Color.Black, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

            _spriteBatch.Draw(texture, new Rectangle((_graphics.PreferredBackBufferWidth - rectWidth) / 2, 80 + menuLineSpacing * line, rectWidth, rectHeight), menu3);
            menuText = "Exit";
            textSize = font.MeasureString(menuText);
            textPos = new Vector2((_graphics.PreferredBackBufferWidth - textSize.X*2) / 2, 80 + rectHeight/2-textSize.Y/2 + menuLineSpacing * line++);
            _spriteBatch.DrawString(font, menuText, textPos, Color.Black, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
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
