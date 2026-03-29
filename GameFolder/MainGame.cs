using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;
using System;
using Microsoft.Xna.Framework.Audio;
using typatro.GameFolder.Services;

namespace typatro.GameFolder;

public class MainGame : Game
{
    public static GraphicsDeviceManager graphics;
    public static class Gfx
    {
        public static SpriteBatch spriteBatch;
        public static Texture2D texture, catPic, bg, foxy;
        public static Texture2D mouse1, mouse2;
        public static SpriteFont gameFont, smallTextFont, menuFont, textFont, smallMapFont, logoFont;
    }
    public struct SoundEffects
    {
        public SoundEffect typeSound, jumpscareSound;
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
        graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        Window.Title = "GLYPHORA";
        graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent(){
        string jsonText = File.ReadAllText("Content/wordlist.json");
        jsonText = jsonText.Trim();
        List<string> jsonStrings = JsonSerializer.Deserialize<List<string>>(jsonText);

        Gfx.spriteBatch = new SpriteBatch(GraphicsDevice);
        Gfx.gameFont = Content.Load<SpriteFont>("Fonts/pixelFont");
        Gfx.smallTextFont = Content.Load<SpriteFont>("Fonts/smallPixelFont");
        Gfx.smallMapFont = Content.Load<SpriteFont>("Fonts/smallMapFont");
        Gfx.menuFont = Content.Load<SpriteFont>("Fonts/menuFont");
        Gfx.textFont = Content.Load<SpriteFont>("Fonts/textFont");
        Gfx.logoFont = Content.Load<SpriteFont>("Fonts/logoFont");
        Gfx.catPic = Content.Load<Texture2D>("Images/catPic");
        Gfx.foxy = Content.Load<Texture2D>("Images/foxy");
        Gfx.bg = Content.Load<Texture2D>("Images/bg");
        Gfx.mouse1 = Content.Load<Texture2D>("Images/mouseOpenY");
        Gfx.mouse2 = Content.Load<Texture2D>("Images/mouseClosedY");
        Gfx.texture = new Texture2D(GraphicsDevice, 1, 1);
        Gfx.texture.SetData(new[] { Color.White });

        GlyphImageLoad();
        EnemyManager.Load(Content);
        
        int[] settings = SaveManager.LoadSettings();
        ThemeColors.Apply(settings[0]);
        SoundEffects sfx = new()
        {
            musicIntro = Content.Load<SoundEffect>("Music/intro").CreateInstance(),
            musicMainTheme = Content.Load<SoundEffect>("Music/mainTheme").CreateInstance(),
            typeSound = Content.Load<SoundEffect>("Music/typing2"),
            jumpscareSound = Content.Load<SoundEffect>("Music/jumpscare"),
        };
        
        MediaPlayer.IsRepeating = false;
        MediaPlayer.Volume = settings[1]/10f;
        sfx.musicIntro.Volume = settings[1] / 10f;
        sfx.musicMainTheme.Volume = settings[1] / 10f;
        sfx.musicIntro.IsLooped = false;
        sfx.musicIntro.Play();

        SteamManager.Init();
        gameLogic = new GameLogic(jsonStrings, Window.Position, sfx);
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
