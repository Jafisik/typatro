using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


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
        PLAY,
        OPTIONS,
        EXIT,
        MENU
    }

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
        for (int i = 0; i < 20; i++)
        {
            neededText += jsonStrings[rand.Next(0, jsonStrings.Count)] + " ";
        }
        neededText += jsonStrings[rand.Next(0, jsonStrings.Count)];

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("Fonts/pixelFont");
        writer = new(_spriteBatch, font, diffIndexes, writtenText);

        Texture2D texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData(new[] { Color.White });
        menu = new Menu(_spriteBatch, font, texture);
    }

    protected override void Update(GameTime gameTime)
    {
        if (gameState == GameState.PLAY)
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

        if (gameState == GameState.MENU)
        {
            gameState = (GameState)menu.DrawMenu(_graphics);
        }

        else if (gameState == GameState.PLAY)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) gameState = GameState.MENU;

            //Write background text
            writer.WriteText(neededText, Color.Gray, isHintText: true);
            //Write used inputed text and highlight mistakes
            writer.UserInputText(writtenText.ToArray(), Color.Black);
            //Calculate correct/wrong precentage
            writer.WriteText("Correct: " + (writtenText.Count > 0 ? ((1f - (diffIndexes.Count / (float)writtenText.Count)) * 100).ToString("0.00") + "%" : "0%"), Color.Black, 5);
        }

        else if (gameState == GameState.OPTIONS)
        {

        }

        else if (gameState == GameState.EXIT)
        {
            Exit();
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
