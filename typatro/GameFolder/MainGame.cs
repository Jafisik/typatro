using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder;

public class MainGame : Game
{
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    Texture2D texture;
    GameLogic gameLogic;
    Color bgColor = Color.DarkGray;

    public MainGame(){
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize(){
        graphics.PreferredBackBufferWidth = 1024;
        graphics.PreferredBackBufferHeight = 576;
        graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent(){
        string jsonText = File.ReadAllText("Content/wordlist.json");
        List<string> jsonStrings = JsonSerializer.Deserialize<List<string>>(jsonText);
        
        spriteBatch = new SpriteBatch(GraphicsDevice);
        SpriteFont gameFont = Content.Load<SpriteFont>("Fonts/pixelFont");
        SpriteFont menuFont = Content.Load<SpriteFont>("Fonts/menuFont");

        texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData(new[] { Color.White });

        gameLogic = new GameLogic(spriteBatch, menuFont, gameFont, texture, jsonStrings, Window.Position);
    }

    protected override void Update(GameTime gameTime){
        gameLogic.Update(gameTime, Window);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime){
        GraphicsDevice.Clear(bgColor);
        gameLogic.Draw(graphics);
        base.Draw(gameTime);
    }
}
