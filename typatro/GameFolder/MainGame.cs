using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder;

public class MainGame : Game
{
    public static GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    Texture2D texture;
    GameLogic gameLogic;
    public static int screenWidth = 800, screenHeight = 600;
    
    public MainGame(){
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        graphics.IsFullScreen = false;
        Window.IsBorderless = true;
    }

    protected override void Initialize(){
        graphics.PreferredBackBufferWidth = screenWidth;
        graphics.PreferredBackBufferHeight = screenHeight;
        Window.IsBorderless = true;
        Window.Title = "GLYPHORA";

        graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent(){
        string jsonText = File.ReadAllText("Content/wordlist.json");
        jsonText = jsonText.Trim();
        List<string> jsonStrings = JsonSerializer.Deserialize<List<string>>(jsonText);
        
        spriteBatch = new SpriteBatch(GraphicsDevice);
        SpriteFont gameFont = Content.Load<SpriteFont>("Fonts/pixelFont");
        SpriteFont smallTextFont = Content.Load<SpriteFont>("Fonts/smallPixelFont");
        SpriteFont menuFont = Content.Load<SpriteFont>("Fonts/menuFont");
        SpriteFont textFont = Content.Load<SpriteFont>("Fonts/textFont");
        
        GlyphImageLoad();
        
        int[] settings = SaveManager.LoadSettings();
        ThemeColors.Apply(settings[0]);
        Texture2D catPic = Content.Load<Texture2D>("Images/catPic");
        texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData(new[] { Color.White });

        Song backgroundMusic = Content.Load<Song>("Music/glyphoraFr");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = settings[1]/10f;
        MediaPlayer.Play(backgroundMusic);

        gameLogic = new GameLogic(spriteBatch, menuFont, gameFont, smallTextFont, textFont, texture, jsonStrings, Window.Position, catPic);
    }

    protected override void Update(GameTime gameTime){
        gameLogic.Update(gameTime, Window);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime){
        gameLogic.Draw(graphics);
        base.Draw(gameTime);
    }

    private void GlyphImageLoad(){
        string[] glyphNames = new string[]{"empty", "A", "B", "D", "H", "J", "M", "N", "R", "S", "sun", "house", "water", "king",
                                           "eyeOfHorus", "osiris", "woman", "man", "flower", "cat", "anubis", "scarab", "snake", "life", 
                                           "heart", "crocodile", "one", "ten", "hundred", "thousand", "bread", "papyrus", "star"};
        foreach(string glyphName in glyphNames){
            GlyphManager.glyphImage.Add(Content.Load<Texture2D>($"Glyphs/{glyphName}"));
        }
    }

}
