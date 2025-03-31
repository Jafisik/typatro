using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;
using System.Text.Json;


namespace typatro.GameFolder;

public class MainGame : Game
{
    Random rand = new Random();
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    SpriteFont gameFont;

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
    Texture2D texture;
    Color bgColor = Color.DarkGray, textColor = Color.GreenYellow;
    Map map;

    public MainGame()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        originalPosition = Window.Position;
        graphics.PreferredBackBufferWidth = 1024;
        graphics.PreferredBackBufferHeight = 576;
        graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        string jsonText = File.ReadAllText("Content/wordlist.json");
        jsonStrings = JsonSerializer.Deserialize<List<string>>(jsonText);
        neededText = RandomTextGenerate(10);
        
        spriteBatch = new SpriteBatch(GraphicsDevice);
        gameFont = Content.Load<SpriteFont>("Fonts/pixelFont");
        writer = new(spriteBatch, gameFont, diffIndexes, writtenText);

        SpriteFont menuFont = Content.Load<SpriteFont>("Fonts/menuFont");
        texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData(new[] { Color.White });
        menu = new Menu(spriteBatch, menuFont, texture);
        
        map = new Map(spriteBatch, menuFont, rand.Next());
    }

    Point originalPosition;
    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Space)) // Spustí otřes při stisknutí mezerníku
        {
            int shakeAmount = 5; // Maximální posun v pixelech
            int offsetX = rand.Next(-shakeAmount, shakeAmount + 1);
            int offsetY = rand.Next(-shakeAmount, shakeAmount + 1);

            Window.Position = new Point(originalPosition.X + offsetX, originalPosition.Y + offsetY);
        }
        else
        {
            Window.Position = originalPosition; // Vrátí okno zpět
        }
        if (gameState == GameState.PLAY)
        {
            writer.ReadKeyboardInput(gameTime);
            writer.UpdateDiffIndexes(neededText);
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(bgColor);
        spriteBatch.Begin();

        if (gameState == GameState.MENU)
        {
            gameState = (GameState)menu.DrawMainMenu(graphics);
        }

        else if (gameState == GameState.PLAY)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)){
                neededText = RandomTextGenerate(10);
                gameState = GameState.MENU;
            }

            map.DrawNodes();

            /*
            //Write background text
            writer.WriteText(neededText, Color.Gray, isHintText: true);
            //Write used inputed text and highlight mistakes
            writer.UserInputText(writtenText.ToArray(), textColor);
            //Calculate correct/wrong precentage
            writer.WriteText("Correct: " + (writtenText.Count > 0 ? ((1f - (diffIndexes.Count / (float)writtenText.Count)) * 100).ToString("0.00") + "%" : "0%"), Color.White, 5);
            */
        }

        else if (gameState == GameState.OPTIONS)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) gameState = GameState.MENU;
            menu.DrawOptionsMenu(graphics);
        }

        else if (gameState == GameState.EXIT)
        {
            Exit();
        }

        spriteBatch.End();
        base.Draw(gameTime);
    }

    public string RandomTextGenerate(int length){
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            stringBuilder.Append(jsonStrings[rand.Next(0, jsonStrings.Count)] + " ");
        }
        stringBuilder.Append(jsonStrings[rand.Next(0, jsonStrings.Count)]);
        return stringBuilder.ToString();
    }
}
