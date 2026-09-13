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
    public static GameWindow gameWindow;
    public static class Gfx
    {
        public static SpriteBatch spriteBatch;
        public static Texture2D texture, catPic, bg, foxy;
        public static Texture2D treasureBg, curseBg, yesOverlay, noOverlay;
        public static Texture2D mouse1, mouse2;
        public static SpriteFont gameFont, smallTextFont, menuFont, textFont, smallMapFont, logoFont;
        public static BlendState invertBlend;
        // Everything is drawn into this fixed-size target, then the target is scaled up to
        // fit the real window/monitor - keeps every hardcoded UI position valid regardless
        // of window size or fullscreen resolution.
        public static RenderTarget2D renderTarget;
    }
    public struct SoundEffects
    {
        public SoundEffect typeSound, jumpscareSound;
        public SoundEffect specialWordHit, specialWordMiss, enemyDefeated, enemyIntro;
        public SoundEffectInstance musicIntro, musicMainTheme;
    }
    GameLogic gameLogic;
    public static GameTime time;
    // Fixed design resolution every UI position in the game is authored against - the
    // window defaults to this size, and fullscreen scales a render of this size up to fit.
    public static int screenWidth = 1280, screenHeight = 720;

    public MainGame(){
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        graphics.IsFullScreen = false;
        Window.IsBorderless = true;
    }

    protected override void Initialize(){
        gameWindow = Window;
        graphics.PreferredBackBufferWidth = screenWidth;
        graphics.PreferredBackBufferHeight = screenHeight;
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
        Gfx.treasureBg = Content.Load<Texture2D>("Backgrounds/treasure");
        Gfx.curseBg = Content.Load<Texture2D>("Backgrounds/curse");
        Gfx.yesOverlay = Content.Load<Texture2D>("Backgrounds/yes");
        Gfx.noOverlay = Content.Load<Texture2D>("Backgrounds/no");
        Gfx.bg = Content.Load<Texture2D>("Images/bg");
        Gfx.mouse1 = Content.Load<Texture2D>("Images/mouseOpenY");
        Gfx.mouse2 = Content.Load<Texture2D>("Images/mouseClosedY");
        Gfx.texture = new Texture2D(GraphicsDevice, 1, 1);
        Gfx.texture.SetData(new[] { Color.White });
        Gfx.renderTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight);
        // Drawing a white rect with this blend state inverts everything drawn so far this
        // frame (result = white - dest), used for Ictus's screen-invert flash.
        Gfx.invertBlend = new BlendState
        {
            ColorBlendFunction = BlendFunction.Subtract,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One
        };

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
            specialWordHit = Content.Load<SoundEffect>("Audio/impactMetal_004"),
            specialWordMiss = Content.Load<SoundEffect>("Audio/laserRetro_000"),
            enemyDefeated = Content.Load<SoundEffect>("Audio/explosionCrunch_002"),
            enemyIntro = Content.Load<SoundEffect>("Audio/doorOpen_000"),
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
        GraphicsDevice.SetRenderTarget(Gfx.renderTarget);
        gameLogic.Draw();
        GraphicsDevice.SetRenderTarget(null);

        GraphicsDevice.Clear(Color.Black);
        Gfx.spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
        Gfx.spriteBatch.Draw(Gfx.renderTarget, FullscreenDestRect(), Color.White);
        Gfx.spriteBatch.End();

        base.Draw(time);
    }

    // Scales the fixed-size render up to fit the actual window/monitor while preserving
    // its aspect ratio, letterboxing with black bars if the target doesn't match 16:9.
    private Rectangle FullscreenDestRect(){
        int backBufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        int backBufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        float scale = Math.Min((float)backBufferWidth / screenWidth, (float)backBufferHeight / screenHeight);
        int width = (int)(screenWidth * scale);
        int height = (int)(screenHeight * scale);
        return new Rectangle((backBufferWidth - width) / 2, (backBufferHeight - height) / 2, width, height);
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
