using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using typatro.GameFolder.Logic;
using typatro.GameFolder.Models;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.Services;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;
using static typatro.GameFolder.Services.UnlockManager;

namespace typatro.GameFolder
{
    public class UserAction{
        public string action;
        public string data;
        public UserAction(string action, string data){
            this.action = action;
            this.data = data;
        }

        public string[] ToStringArray(){
            return new string[]{ action, data };
        }

        public static UserAction FromStringArray(string[] data){
            return new UserAction(data[0],data[1]);
        }
    }

    public partial class GameLogic
    {
        public enum GameState
        {
            LOADGAME,
            NEWGAME,
            OPTIONS,
            EXIT,
            MENU,
            RUNES,
        }

        public GameState gameState = GameState.MENU;

        //Other
        readonly Menu menu;
        public double timeInSeconds = 0, lastTime = -1;
        long totalGameTimeMinutes = 0;
        Point windowPos, dragOffset;
        bool eyeOfHorusActive, isDragging, gameFinished, deadCounted, introPlayed;
        public bool mistake;
        public bool anubisActive;
        public bool mousePressed, tutorial;
        bool mapTutorialStarted = false;
        MainGame.SoundEffects sfx;

        //Player
        public Enhancements enhancements;
        public long coins = 0, startCoins = 30;
        public int selectedRune = 0, difficulty = 0, inventoryGlyphSelect = 1;
        public bool dead, inventoryMousePressed;
        KeyboardState prevKBState;
        public bool inventoryMove = true;
        public static bool keyboardUsed, windowActive;
        Point mousePosition = new Point();
        CharacterSelect characterSelect;
        GameUi gameUi;

        //Score calculator
        public double letterTimer = 0, timeSinceLastWord = 0, wordStreak = 1;
        ScoreCalculator scoreCalculator;

        //Final stats
        public long totalScore, maxScore, lettersWritten, mistakesWritten, wordsWritten;
        public long highestStreak, coinsGained, maxCoins;

        //Rooms
        Fight fight;
        public bool canStartFight;
        public bool startedTyping, roomSelected, isFightFinished;
        bool afterFightScreen, afterFightMove;
        int afterFightSelect = 0;
        List<LetterUpgrade> cards = new List<LetterUpgrade>();
        Treasure treasure;
        Shop shop;
        CurseRoom curseRoom;


        //Saving
        public GameSaveData gameSaveData;
        public static List<UserAction> actions = new List<UserAction>();
        List<UserAction> lastActions = actions;
        public static bool isReplay = false;
        public static int seed;
        public static Random seededRandom = new Random(), unseededRandom = new Random();

        //Writer
        readonly Writer writer;
        public string neededText;
        readonly List<string> jsonStrings;
        int xTextOffset = 0, yTextOffset = 0;
        public List<int> shinyWords = new List<int>(), stoneWords = new List<int>(), bloomWords = new List<int>();
        double textRotation = 0;
        Vector2 catPos = Vector2.One;

        //Map
        Map map;
        List<int[]> visitedNodes = new List<int[]>();
        MapNode selectedNode, lastSelectedNode;
        public int level = 1;
        bool firstEnter = true;
        public bool inventoryUp;

        public GameLogic(List<string> jsonStrings, Point windowPos, MainGame.SoundEffects sfx)
        {
            this.jsonStrings = jsonStrings;
            this.windowPos = windowPos;
            this.sfx = sfx;

            menu = new Menu();
            gameSaveData = SaveManager.LoadGame();

            map = new Map();
            enhancements = new Enhancements();

            writer = new Writer();
            neededText = RandomTextGenerate(10);

            treasure = new Treasure(enhancements);
            shop = new Shop(enhancements);
            curseRoom = new CurseRoom(enhancements);
            GlyphManager.SetUnlockedGlyphs();
            characterSelect = new CharacterSelect(this);
            gameUi = new GameUi(this);
            scoreCalculator = new ScoreCalculator(this);

        }

        public void Update(GameWindow window, bool isActive)
        {
            HandleWindowDragging(window);
            if (gameState == GameState.NEWGAME || gameState == GameState.LOADGAME)
            {
                TypingSystem(window);
            }
            if(totalGameTimeMinutes != (long)MainGame.time.TotalGameTime.Minutes)
            {
                totalGameTimeMinutes = (long)MainGame.time.TotalGameTime.Minutes;
                SteamManager.IncrementStat(SteamManager.SteamStats.GameMinutes);
            }
            sfx.musicIntro.Volume = (float)SaveManager.volume / 10;
            sfx.musicMainTheme.Volume = (float)SaveManager.volume / 10;
            if (!introPlayed && sfx.musicIntro.State == SoundState.Stopped)
            {
                introPlayed = true;
                sfx.musicMainTheme.IsLooped = true;
                sfx.musicMainTheme.Play();
                if (!sfx.musicIntro.IsDisposed) sfx.musicIntro.Dispose();
            }
            windowActive = isActive;
        }

        private void HandleWindowDragging(GameWindow window)
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && windowActive)
            {
                if (new Rectangle(0, 0, MainGame.screenWidth, 15).Contains(mouseState.Position) || new Rectangle(0, 0, 15, MainGame.screenHeight).Contains(mouseState.Position) ||
                    new Rectangle(0, MainGame.screenHeight - 15, MainGame.screenWidth, MainGame.screenHeight).Contains(mouseState.Position) || new Rectangle(MainGame.screenWidth - 15, 0, MainGame.screenWidth, MainGame.screenHeight).Contains(mouseState.Position))
                {
                    if (!isDragging)
                    {
                        isDragging = true;
                        dragOffset = new Point(mouseState.X, mouseState.Y);
                    }
                }
            }

            if (mouseState.LeftButton == ButtonState.Released)
            {
                isDragging = false;
            }

            if (isDragging)
            {
                window.Position = new Point(mouseState.X + window.Position.X - dragOffset.X, mouseState.Y + window.Position.Y - dragOffset.Y);
            }
        }

        private void TypingSystem(GameWindow window)
        {
            if ((int)timeInSeconds % 8 == 0 && timeInSeconds != 0)
            {
                if (!GlyphManager.IsActive(Glyph.House))
                {
                    writer.ReadKeyboardInput(MainGame.time);
                    writer.UpdateDiffIndexes(neededText);
                }
            }
            else
            {
                writer.ReadKeyboardInput(MainGame.time);
                writer.UpdateDiffIndexes(neededText);
            }
            if (!startedTyping && Writer.writtenText.Count > 0)
            {
                startedTyping = true;
                timeInSeconds = 0;
            }


            if (startedTyping)
            {
                if (!isFightFinished)
                {
                    timeInSeconds += MainGame.time.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    letterTimer = 0;
                }
                if ((int)timeInSeconds != lastTime)
                {
                    if ((int)timeInSeconds % 10 == 0)
                    {
                        if (!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.B)) textRotation += Math.PI;
                        if (GlyphManager.IsActive(Glyph.M)) coins += 10;
                    }
                    if ((int)timeInSeconds % 5 == 0 && timeInSeconds != 0)
                    {
                        if (!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.EyeOfHorus)) eyeOfHorusActive = true;
                        if (!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.M))
                        {
                            xTextOffset = unseededRandom.Next(-100, 101);
                            yTextOffset = unseededRandom.Next(0, 101);
                        }
                        else
                        {
                            xTextOffset = 0;
                            yTextOffset = 0;
                        }
                    }
                    else eyeOfHorusActive = false;
                }
                lastTime = (int)timeInSeconds;
                KeyboardState keyboardState = Keyboard.GetState();
                if (!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.J))
                {
                    if (keyboardState.GetPressedKeyCount() > 0 && keyboardState.IsKeyUp(Keys.Tab))
                    {
                        window.Position = new Point(windowPos.X + unseededRandom.Next(-5, 6), windowPos.Y + unseededRandom.Next(-5, 6));
                    }
                    else window.Position = windowPos;
                }
            }
        }

        float bgRotation = 0f, scaleDelta;
        bool scaleSwap;
        public void Draw(GraphicsDeviceManager graphicsDevice)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            

            MainGame.Gfx.spriteBatch.Begin(SpriteSortMode.Deferred);
            bgRotation -= 0.002f;
            if (!scaleSwap)
            {
                scaleDelta += 0.0002f;
                if (scaleDelta >= 0.2f) scaleSwap = true;
            }
            else
            {
                scaleDelta -= 0.0002f;
                if (scaleDelta <= 0f) scaleSwap = false;
            }


            Vector2 centerScreen = new Vector2(MainGame.screenWidth / 2f, MainGame.screenHeight / 2f);
            Vector2 bgOrigin = new Vector2(MainGame.Gfx.bg.Width / 2f, MainGame.Gfx.bg.Height / 2f);

            MainGame.Gfx.spriteBatch.GraphicsDevice.Clear(ThemeColors.Background);
            Color bgImageColor = ThemeColors.Background;
            bgImageColor.A = 150;
            float scale = 1f;
            if (SaveManager.size == 1) scale = 1.4f;
            if (SaveManager.size == 2) scale = 1.6f;
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.bg, centerScreen, null, bgImageColor, bgRotation, bgOrigin, scale+scaleDelta, SpriteEffects.None,0f);
            int lineWidth = 15;
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(0, 0, MainGame.screenWidth, lineWidth), ThemeColors.Foreground);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(0, 0, lineWidth, MainGame.screenHeight), ThemeColors.Foreground);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(0, MainGame.screenHeight - lineWidth, MainGame.screenWidth, lineWidth), ThemeColors.Foreground);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(MainGame.screenWidth - lineWidth, 0, lineWidth, MainGame.screenHeight), ThemeColors.Foreground);



            if (SaveManager.fullscreen == 1)
            {
                graphicsDevice.IsFullScreen = true;
            }
            else
            {
                graphicsDevice.IsFullScreen = false;
            }
            if (gameState == GameState.MENU)
            {
                gameState = (GameState)menu.DrawMainMenu(gameSaveData != null, gameFinished);
                if (Keyboard.GetState().IsKeyUp(Keys.Enter))
                    gameFinished = false;
                if (gameState == GameState.LOADGAME) LoadGame();
                if (gameState == GameState.NEWGAME) NewGame();
                firstEnter = true;
                roomSelected = false;
                canStartFight = false;
                UnlockManager.UnlockUnlock(UnlockManager.UnlockType.Uruz0);
            }
                else if (gameState == GameState.RUNES)
                {
                    KeyboardState state = Keyboard.GetState();

                    characterSelect.CharacterChoose(ref enhancements);
                if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.CharacterTutorial))
                {
                    SpriteFont font = SaveManager.size == 0 ? MainGame.Gfx.smallTextFont : MainGame.Gfx.menuFont;
                    if (TutorialManager.Draw(state, mouseState, font))
                        UnlockManager.UnlockUnlock(UnlockType.CharacterTutorial);
                    
                }
                }
                else if (gameState == GameState.LOADGAME)
                {
                    Play();
                }
                else if (gameState == GameState.OPTIONS)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    {
                        gameState = GameState.MENU;

                        SaveManager.SaveSettings(SaveManager.theme, SaveManager.volume, SaveManager.size, SaveManager.fullscreen);
                    }

                    ThemeColors.Apply(SaveManager.theme);


                    if (menu.DrawOptionsMenu()) gameState = GameState.MENU;
                }
                else if (gameState == GameState.EXIT)
                {
                    SteamManager.Shutdown();
                    Environment.Exit(0);
                }

            if (keyboardState.GetPressedKeyCount() > 0 && !(keyboardState.GetPressedKeyCount() == 1 && keyboardState.IsKeyDown(Keys.Tab)))
            {
                keyboardUsed = true;
            }
            Texture2D mouseTexture = MainGame.Gfx.mouse1;
            if (mouseState.LeftButton == ButtonState.Pressed) mouseTexture = MainGame.Gfx.mouse2;
            if (!keyboardUsed)
            {
                MainGame.Gfx.spriteBatch.Draw(mouseTexture, new Vector2(mouseState.X, mouseState.Y), null, ThemeColors.Mouse, 0f, new Vector2(16, 16), 1.6f, SpriteEffects.None, 0f);
            }
            else
            {
                if (mousePosition != mouseState.Position)
                {
                    keyboardUsed = false;
                }
            }

            mousePosition = mouseState.Position;
            MainGame.Gfx.spriteBatch.End();
        }



        private void LoadGame()
        {
            seed = gameSaveData.seed;
            seededRandom = new Random(seed);
            map = new Map();
            enhancements = new Enhancements();
            enhancements.letters = gameSaveData.letterScores;
            enhancements.damageResist = gameSaveData.enhancements[0];
            enhancements.mistakeBlock = gameSaveData.enhancements[1];
            enhancements.shinyChance = gameSaveData.enhChances[0];
            enhancements.stoneChance = gameSaveData.enhChances[1];
            enhancements.bloomChance = gameSaveData.enhChances[2];
            enhancements.streakMult = gameSaveData.enhChances[3];
            shop = new Shop(enhancements);
            treasure = new Treasure(enhancements);
            curseRoom = new CurseRoom(enhancements);
            coins = gameSaveData.coins;
            level = gameSaveData.level;
            selectedRune = gameSaveData.rune;
            difficulty = gameSaveData.difficulty;

            // Replay saved actions to get random back to the same state
            UserAction[] userActions = SaveManager.LoadActions();
            actions = new List<UserAction>(userActions);
            lastActions = actions;
            isReplay = true;
            foreach (var entry in userActions)
            {
                switch (entry.action)
                {
                    case "RandomTextGenerate":  RandomTextGenerate(int.Parse(entry.data)); break;
                    case "GenerateNodes":       map.GenerateNodes(); break;
                    case "GenerateNodeType":    map.GenerateNodeType(); break;
                    case "GenerateNodeTypeFromRandom": map.GenerateNodeTypeFromRandom(); break;
                    case "GenerateCard":        shop.GenerateCard(); break;
                    case "GetRandomUnusedGlyph": GlyphManager.GetRandomUnusedGlyph(); break;
                    case "randomLetter":        seededRandom.Next(0, 26); break;
                    default: System.Diagnostics.Debug.WriteLine($"Unknown action: {entry.action}"); break;
                }
            }
            map.NodeVisit(gameSaveData.visitedNodes);
            visitedNodes = gameSaveData.visitedNodes;
            isReplay = false;
            selectedNode = map.GetNodeFromPos(gameSaveData.mapNode[0], gameSaveData.mapNode[1]);
            lastSelectedNode = selectedNode;
            mousePressed = true;
            foreach (int glyph in gameSaveData.glyphs)
                GlyphManager.Add((Glyph)glyph);
        }

        private void NewGame()
        {
            level = 1;
            actions.Clear();
            seed = UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.MapTutorial) ? unseededRandom.Next() : 10;
            seededRandom = new Random(seed);
            map = new Map();
            map.GenerateNodes();
            selectedNode = map.GetFirstNode();
            lastSelectedNode = map.GetFirstNode();
            enhancements = new Enhancements();
            shop = new Shop(enhancements);
            treasure = new Treasure(enhancements);
            curseRoom = new CurseRoom(enhancements);
            coins = difficulty >= 1 ? 15 : startCoins;
            if (difficulty >= 3) enhancements.streakMult -= 1;
            GlyphManager.RemoveAllGlyphs();
            GlyphManager.Add(Glyph.NoGlyphsLeft);
            visitedNodes = new List<int[]>();
            mistake = false;
            deadCounted = false;
            mousePressed = true;
            tutorial = false;
            mapTutorialStarted = false;
            gameState = GameState.RUNES;
        }

        private void Reset()
        {
            lastActions = new List<UserAction>(actions);
            enhancements.ResetChange();
            isFightFinished = false;
            afterFightScreen = false;
            wordStreak = 1;
            inventoryGlyphSelect = 1;
            afterFightSelect = 0;
            pitch = 0;
            prevMistakes = 0;
            cards.Clear();
            shinyWords.Clear();
            stoneWords.Clear();
            scoreCalculator.Reset();
        }

        private string RandomTextGenerate(int length)
        {
            if (!isReplay) actions.Add(new UserAction("RandomTextGenerate", length.ToString()));
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                if (unseededRandom.NextDouble() >= 1 - enhancements.shinyChance) shinyWords.Add(i);
                else if (unseededRandom.NextDouble() >= 1 - enhancements.bloomChance) bloomWords.Add(i);
                else if (unseededRandom.NextDouble() >= 1 - enhancements.stoneChance) stoneWords.Add(i);


                string word = jsonStrings[seededRandom.Next(0, jsonStrings.Count)];
                if (GlyphManager.IsActive(Glyph.Snake) && unseededRandom.Next(0, 16) == 12)
                {
                    char[] wordToChar = word.ToCharArray();
                    wordToChar[unseededRandom.Next(0, word.Length)] = (char)(unseededRandom.Next(0, 26) + 'a');
                    word = new string(wordToChar);
                }
                stringBuilder.Append(word + " ");
            }
            return stringBuilder.ToString();
        }

        public static bool IsFight(NodeType nodeType) =>
            nodeType is NodeType.FIGHT or NodeType.ELITE or NodeType.BOSS;

        // Generates a unique reward card with a letter not already in usedChars.
        // Preserves the exact seededRandom call order needed for save/replay compatibility.
        private LetterUpgrade GenerateRewardCard(List<char> usedChars, bool mult, int valMin, int valMax)
        {
            char ch = (char)(seededRandom.Next(0, 26) + 'a');
            if (!isReplay) actions.Add(new UserAction("randomLetter", ""));
            while (usedChars.Contains(ch))
            {
                ch = (char)(seededRandom.Next(0, 26) + 'a');
                if (!isReplay) actions.Add(new UserAction("randomLetter", ""));
            }
            if (!isReplay) actions.Add(new UserAction("randomLetter", ""));
            return new LetterUpgrade(ch, mult, seededRandom.Next(valMin, valMax), 0);
        }

        private void DrawRunStats(int col1X, int col2X)
        {
            var letters = enhancements.HighestLetter();
            int accuracy = lettersWritten > 0 ? (int)((1.0 - (double)mistakesWritten / lettersWritten) * 100) : 100;
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont,
                $"Words written: {wordsWritten}\nLetters written:{lettersWritten}\nMistakes: {mistakesWritten}\nAccuracy: {accuracy}%",
                new Vector2(col1X, 150), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont,
                $"Most upgraded letter: {letters.bestLetter}:  {letters.bestLetterNum}\nTotal score: {totalScore}\nMax score: {maxScore}",
                new Vector2(col2X, 150), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont,
                $"Shiny words: {scoreCalculator.getShinyWritten()}\nStone words: {scoreCalculator.getStoneWritten()}\nBloom words: {scoreCalculator.getBloomWritten()}",
                new Vector2(col1X, 300), ThemeColors.Text);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont,
                $"Highest streak: {highestStreak}\nCoins gained: {coinsGained}\nMax coins: {maxCoins}",
                new Vector2(col2X, 300), ThemeColors.Text);
        }
    }
}
