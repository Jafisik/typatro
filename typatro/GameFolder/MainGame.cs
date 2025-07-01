using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;
using Steamworks;
using System;
using Microsoft.Xna.Framework.Audio;

namespace typatro.GameFolder;

public class MainGame : Game
{
    public static GraphicsDeviceManager graphics;
    public struct Gfx
    {
        public SpriteBatch spriteBatch;
        public Texture2D texture, catPic, bg;
        public Texture2D mouse1, mouse2;
        public SpriteFont gameFont, smallTextFont, menuFont, textFont, smallMapFont;
    }
    public struct SoundEffects
    {
        public SoundEffect typeSound;
        public SoundEffectInstance musicIntro, musicMainTheme;
    }
    GameLogic gameLogic;
    public static GameTime time;
    public static int screenWidth = 800, screenHeight = 600;
    
    public MainGame(){
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        graphics.IsFullScreen = false;
        Window.IsBorderless = true;
    }

    protected override void Initialize(){
        graphics.PreferredBackBufferWidth = screenWidth;
        graphics.PreferredBackBufferHeight = screenHeight;
        Window.IsBorderless = true;
        Window.Title = "GLYPHORA";
        if (!SteamAPI.Init())
        {
            Console.WriteLine("SteamAPI.Init() selhalo.");
        }
        graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent(){
        string jsonText = File.ReadAllText("Content/wordlist.json");
        jsonText = jsonText.Trim();
        List<string> jsonStrings = JsonSerializer.Deserialize<List<string>>(jsonText);
        Gfx gfx = new()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice),
            gameFont = Content.Load<SpriteFont>("Fonts/pixelFont"),
            smallTextFont = Content.Load<SpriteFont>("Fonts/smallPixelFont"),
            smallMapFont = Content.Load<SpriteFont>("Fonts/smallMapFont"),
            menuFont = Content.Load<SpriteFont>("Fonts/menuFont"),
            textFont = Content.Load<SpriteFont>("Fonts/textFont"),
            catPic = Content.Load<Texture2D>("Images/catPic"),
            bg = Content.Load<Texture2D>("Images/bg"),
            mouse1 = Content.Load<Texture2D>("Images/mouseOpenY"),
            mouse2 = Content.Load<Texture2D>("Images/mouseClosedY"),
            texture = new Texture2D(GraphicsDevice, 1, 1),
        };
        gfx.texture.SetData(new[] { Color.White });

        GlyphImageLoad();
        
        int[] settings = SaveManager.LoadSettings();
        ThemeColors.Apply(settings[0]);
        SoundEffects sfx = new()
        {
            musicIntro = Content.Load<SoundEffect>("Music/intro").CreateInstance(),
            musicMainTheme = Content.Load<SoundEffect>("Music/mainTheme").CreateInstance(),
            typeSound = Content.Load<SoundEffect>("Music/typing2"),
        };
        
        MediaPlayer.IsRepeating = false;
        MediaPlayer.Volume = settings[1]/10f;
        sfx.musicIntro.Volume = settings[1] / 10f;
        sfx.musicMainTheme.Volume = settings[1] / 10f;
        sfx.musicIntro.IsLooped = false;
        sfx.musicIntro.Play();
        

        gameLogic = new GameLogic(gfx, jsonStrings, Window.Position, sfx);
    }

    protected override void Update(GameTime gameTime){
        time = gameTime;
        gameLogic.Update(Window, IsActive);
        base.Update(time);
    }

    protected override void Draw(GameTime gameTime){
        gameLogic.Draw(graphics);
        base.Draw(time);
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
