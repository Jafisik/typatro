using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Steamworks;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

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

    public class GameLogic
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

        //Graphics - contains spriteBatch, fonts and textures
        MainGame.Gfx gfx = new MainGame.Gfx();



        //Other
        readonly Menu menu;
        double timeInSeconds = 0, lastTime = -1;
        long totalGameTimeMinutes = 0;
        Point windowPos, dragOffset;
        public static Dictionary<string, bool> achievmentBools = new Dictionary<string, bool>
        {
            ["ANUBIS"] = false,
            ["CAT"] = false,
            ["PAPYRUS"] = false,
            ["THOUSAND"] = false,
            ["J"] = false,
            ["S"] = false,
            ["CROCODILE"] = false,
            ["EYEOFHORUS"] = false,
            ["H"] = false,
            ["HEART"] = false,
            ["HOUSE"] = false,
            ["HUNDRED"] = false,
            ["KING"] = false,
            ["M"] = false,
            ["MAN"] = false,
            ["N"] = false,
            ["OSIRIS"] = false,
            ["R"] = false,
            ["SNAKE"] = false,
            ["STAR"] = false,
            ["SUN"] = false,
            ["WATER"] = false,
            ["WOMAN"] = false,
            ["HALAGAZ0"] = false,
            ["NAUDHIZ0"] = false,
            ["JERA0"] = false,

        };
        bool eyeOfHorusActive, anubisActive, runeMove, diffMove, tutorial, isDragging, mistake, gameWin, deadCounted, introPlayed, mousePressed;
        public static bool writeAchievment;
        MainGame.SoundEffects sfx;

        //Player
        Enhancements enhancements;
        long coins = 0, startCoins = 30;
        int selectedRune = 0, difficulty = 0, inventoryGlyphSelect = 1;
        bool dead, inventoryMove = true, inventoryMousePressed;
        public static bool keyboardUsed;
        Point mousePosition = new Point();

        //Score calculator
        int charCounter = 0, lastCharCount = 0, wordCounter = 0, lastWordCount = 1, wordStreak = 0, lastCorrectWord = 0, extraScore = 0;
        long currentScore = 0, playerScore = 0, lastScore = 0;
        double letterTimer = 0, timeSinceLastWord = 0;

        //Final stats
        long totalScore, maxScore, lettersWritten, mistakesWritten, wordsWritten, shinyWritten, stoneWritten, bloomWritten;
        long highestStreak, coinsGained, maxCoins;

        //Rooms
        Fight fight;
        bool canStartFight, startedTyping, roomSelected, isFightFinished;
        bool afterFightScreen, afterFightMove;
        int afterFightSelect = 0;
        List<LetterUpgrade> cards = new List<LetterUpgrade>();
        Treasure treasure;
        Shop shop;
        CurseRoom curseRoom;


        //Saving
        GameSaveData gameSaveData;
        public static List<UserAction> actions = new List<UserAction>();
        List<UserAction> lastActions = actions;
        public static bool isReplay = false;
        public static int seed;
        public static Random seededRandom = new Random(), unseededRandom = new Random();

        //Writer
        readonly Writer writer;
        string neededText;
        readonly List<string> jsonStrings;
        int xTextOffset = 0, yTextOffset = 0;
        List<int> shinyWords = new List<int>(), stoneWords = new List<int>(), bloomWords = new List<int>();
        double textRotation = 0;
        Vector2 catPos = Vector2.One;

        //Map
        Map map;
        List<int[]> visitedNodes = new List<int[]>();
        MapNode selectedNode, lastSelectedNode;
        int level = 1;
        bool firstEnter = true, inventoryUp;

        public GameLogic(MainGame.Gfx gfx, List<string> jsonStrings, Point windowPos, MainGame.SoundEffects sfx)
        {
            this.gfx = gfx;
            this.jsonStrings = jsonStrings;
            this.windowPos = windowPos;
            this.sfx = sfx;

            seed = seededRandom.Next();
            seededRandom = new Random(seed);

            menu = new Menu(gfx);
            gameSaveData = SaveManager.LoadGame();

            map = new Map(gfx);
            enhancements = new Enhancements();

            writer = new Writer(gfx.spriteBatch, gfx.textFont);
            neededText = RandomTextGenerate(10);

            
            treasure = new Treasure(gfx, enhancements);
            shop = new Shop(gfx, enhancements);
            curseRoom = new CurseRoom(gfx, enhancements);
            GlyphManager.SetUnlockedGlyphs();
        }

        public void Update(GameWindow window)
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (new Rectangle(0, 0, MainGame.screenWidth, 15).Contains(mouseState.Position) || new Rectangle(0,0,15,MainGame.screenHeight).Contains(mouseState.Position) ||
                    new Rectangle(0, MainGame.screenHeight-15, MainGame.screenWidth, MainGame.screenHeight).Contains(mouseState.Position) || new Rectangle(MainGame.screenWidth-15, 0, MainGame.screenWidth, MainGame.screenHeight).Contains(mouseState.Position))
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
            if (gameState == GameState.NEWGAME || gameState == GameState.LOADGAME)
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
                            if (!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.H)) textRotation += Math.PI;
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
            if(totalGameTimeMinutes != (long)MainGame.time.TotalGameTime.Minutes)
            {
                totalGameTimeMinutes = (long)MainGame.time.TotalGameTime.Minutes;
                if (SteamUserStats.GetStat("game_minutes", out int minutes))
                {
                    SteamUserStats.SetStat("game_minutes", ++minutes);
                    if (minutes >= 240)
                    {
                        SteamUserStats.SetAchievement("HOUSE");
                        SaveManager.UnlockUnlock("HOUSE");
                    }
                    SteamUserStats.StoreStats();
                }
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
            WriteAchievment();
        }

        float bgRotation = 0f;
        public void Draw(GraphicsDeviceManager graphicsDevice)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            

            gfx.spriteBatch.Begin(SpriteSortMode.Deferred);
            bgRotation += 0.002f;


            if(keyboardState.GetPressedKeyCount() > 0 && !(keyboardState.GetPressedKeyCount() == 1 && keyboardState.IsKeyDown(Keys.Tab)))
            {
                keyboardUsed = true;
            }
            Texture2D mouseTexture = gfx.mouse1;
            if (mouseState.LeftButton == ButtonState.Pressed) mouseTexture = gfx.mouse2;
            if (!keyboardUsed)
            {
                gfx.spriteBatch.Draw(mouseTexture, new Vector2(mouseState.X, mouseState.Y), null, Color.White, 0f, new Vector2(16, 16), 1.6f, SpriteEffects.None, 0f);
            }
            else
            {
                if(mousePosition != mouseState.Position)
                {
                    keyboardUsed = false;
                }
            }

            mousePosition = mouseState.Position;


            Vector2 centerScreen = new Vector2(MainGame.screenWidth / 2f, MainGame.screenHeight / 2f);
            Vector2 bgOrigin = new Vector2(gfx.bg.Width / 2f, gfx.bg.Height / 2f);

            gfx.spriteBatch.GraphicsDevice.Clear(ThemeColors.Background);
            Color bgImageColor = ThemeColors.Background;
            bgImageColor.A = 150;
            float scale = 1f;
            if (SaveManager.size == 1) scale = 1.4f;
            if (SaveManager.size == 2) scale = 1.6f;
            gfx.spriteBatch.Draw(gfx.bg, centerScreen, null, bgImageColor, bgRotation, bgOrigin, scale, SpriteEffects.None,0f);
            int lineWidth = 15;
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(0, 0, MainGame.screenWidth, lineWidth), ThemeColors.Foreground);
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(0, 0, lineWidth, MainGame.screenHeight), ThemeColors.Foreground);
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(0, MainGame.screenHeight - lineWidth, MainGame.screenWidth, lineWidth), ThemeColors.Foreground);
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(MainGame.screenWidth - lineWidth, 0, lineWidth, MainGame.screenHeight), ThemeColors.Foreground);



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
                    gameState = (GameState)menu.DrawMainMenu(gameSaveData == null ? false : true, gameWin);
                    if (Keyboard.GetState().IsKeyUp(Keys.Enter))
                    {
                        gameWin = false;
                    }
                    if (gameState == GameState.LOADGAME)
                    {
                        seed = gameSaveData.seed;
                        seededRandom = new Random(seed);
                        map = new Map(gfx);
                        enhancements = new Enhancements();
                        enhancements.letters = gameSaveData.letterScores;
                        enhancements.wordScore = gameSaveData.enhancements[0];
                        enhancements.damageResist = gameSaveData.enhancements[1];
                        enhancements.startingScore = gameSaveData.enhancements[2];
                        enhancements.shinyChance = gameSaveData.enhChances[0];
                        enhancements.stoneChance = gameSaveData.enhChances[1];
                        enhancements.bloomChance = gameSaveData.enhChances[2];
                        shop = new Shop(gfx, enhancements);
                        treasure = new Treasure(gfx, enhancements);
                        curseRoom = new CurseRoom(gfx, enhancements);
                        coins = gameSaveData.coins;
                        level = gameSaveData.level;
                        selectedRune = gameSaveData.rune;
                        difficulty = gameSaveData.difficulty;

                        //Use the random from the seed same amount of times like saved game so that it's on the same random.next
                        UserAction[] UserActions = SaveManager.LoadActions();
                        actions = new List<UserAction>(UserActions);
                        lastActions = actions;
                        isReplay = true;
                        foreach (var entry in UserActions)
                        {
                            string action = entry.action;
                            string data = entry.data;
                            switch (action)
                            {
                                case "RandomTextGenerate":
                                    RandomTextGenerate(int.Parse(data));
                                    break;
                                case "GenerateNodes":
                                    map.GenerateNodes();
                                    break;
                                case "GenerateNodeType":
                                    map.GenerateNodeType();
                                    break;
                                case "GenerateNodeTypeFromRandom":
                                    map.GenerateNodeTypeFromRandom();
                                    break;
                                case "GenerateCard":
                                    shop.GenerateCard();
                                    break;
                                case "GetRandomUnusedGlyph":
                                    GlyphManager.GetRandomUnusedGlyph();
                                    break;
                                case "randomLetter":
                                    seededRandom.Next(0, 26);
                                    break;
                                default:
                                    Console.WriteLine($"Unknown methon: {action}");
                                    break;
                            }
                        }
                        map.NodeVisit(gameSaveData.visitedNodes);
                        visitedNodes = new List<int[]>();
                        visitedNodes = gameSaveData.visitedNodes;
                        isReplay = false;
                        selectedNode = map.GetNodeFromPos(gameSaveData.mapNode[0], gameSaveData.mapNode[1]);
                        lastSelectedNode = selectedNode;
                        mousePressed = true;
                        foreach (int glyph in gameSaveData.glyphs)
                        {
                            GlyphManager.Add((Glyph)glyph);
                        }

                    }
                    if (gameState == GameState.NEWGAME)
                    {
                        level = 1;
                        actions.Clear();
                        seed = unseededRandom.Next();
                        seededRandom = new Random(seed);
                        map = new Map(gfx);
                        map.GenerateNodes();
                        selectedNode = map.GetFirstNode();
                        lastSelectedNode = map.GetFirstNode();
                        enhancements = new Enhancements();
                        shop = new Shop(gfx, enhancements);
                        treasure = new Treasure(gfx, enhancements);
                        curseRoom = new CurseRoom(gfx, enhancements);
                        coins = difficulty >= 1 ? 15 : startCoins;
                        gameState = GameState.RUNES;
                        if (difficulty >= 3) enhancements.wordScore -= 1;
                        GlyphManager.RemoveAllGlyphs();
                        GlyphManager.Add(Glyph.NoGlyphsLeft);
                        visitedNodes = new List<int[]>();
                        mistake = false;
                        deadCounted = false;
                        mousePressed = true;
                    }
                    firstEnter = true;
                    roomSelected = false;
                    canStartFight = false;
                    SaveManager.UnlockUnlock("uruz0");
                }
                else if (gameState == GameState.RUNES)
                {
                    KeyboardState state = Keyboard.GetState();

                    if (!SaveManager.IsUnlockUnlocked("characterTutorial"))
                    {
                        if (state.IsKeyUp(Keys.Enter))
                        {
                            tutorial = true;
                        }
                        gfx.spriteBatch.Draw(gfx.texture, new Rectangle(15, 15, MainGame.screenWidth - 30, MainGame.screenHeight - 30), Color.Black);
                        gfx.spriteBatch.DrawString(gfx.gameFont, "New game tutorial", new Vector2(70, 50), ThemeColors.Text);
                        writer.WriteText("Use the arrows to select a rune\nthat grants unique bonuses,\nthen choose your difficulty.\n\n" +
                            "New runes are unlocked by meeting\nspecific conditions.\n\n" +
                            "New difficulties become available\nonce you complete a run with\na specific rune.\n\n" +
                            "Press Enter to continue.", ThemeColors.Text, treasure: true, xExtraOffset: -30, yExtraOffset: -70);
                        if (tutorial && state.IsKeyDown(Keys.Enter))
                        {
                            SaveManager.UnlockUnlock("characterTutorial");
                            tutorial = false;
                        }

                    }
                    else
                    {
                        CharacterChoose();
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
                    SteamAPI.Shutdown();
                    Environment.Exit(0);
                }
            gfx.spriteBatch.End();
        }


        public void Play()
        {
            KeyboardState kBState = Keyboard.GetState();

            //Return to menu on pressing escape
            if (kBState.IsKeyDown(Keys.Escape) && !gameWin)
            {
                gameState = GameState.MENU;
                if (!dead)
                {
                    gameSaveData = SaveManager.LoadGame();
                }
                dead = false;
                return;
            }

            //Checking if the player died
            if (dead)
            {
                gfx.spriteBatch.DrawString(gfx.gameFont, "You are dead", new Vector2(100), ThemeColors.Text);
                if (!deadCounted)
                {
                    if (SteamUserStats.GetStat("deaths", out int current))
                    {
                        current += 1;
                        SteamUserStats.SetStat("deaths", current);
                        if (current >= 1)
                        {
                            SteamUserStats.SetAchievement("ANUBIS");
                            SaveManager.UnlockUnlock("ANUBIS");
                        }
                        if (current >= 9)
                        {
                            SteamUserStats.SetAchievement("CAT");
                            SaveManager.UnlockUnlock("CAT");
                        }
                        SteamUserStats.StoreStats();
                    }
                    SaveManager.RemoveGameData();
                    GlyphManager.RemoveAllGlyphs();
                    gameSaveData = null;
                    deadCounted = true;
                }
                
                return;
            }

            //roomSelected is true if a room is selected and then it does the room logic, 
            // if roomSelected is false it does the map logic
            if (roomSelected)
            {
                //Can start typing only after enter from selecting the room is released
                if (!canStartFight && kBState.IsKeyUp(Keys.Enter))
                {
                    timeInSeconds = 0;
                    canStartFight = true;
                }

                if (canStartFight)
                {
                    RoomHandler(kBState);
                    if (isFightFinished)
                    {
                        FightFinished(kBState);
                    }
                }
            }
            else
            {
                MapHandler(kBState);
            }
        }

        Keys[] prevKeys = new Keys[0];
        float pitch = 0f;
        int prevMistakes = 0;
        //Does the room logic based on the selectedNode.type
        private void RoomHandler(KeyboardState state)
        {
            if (IsFight(selectedNode.type) && !afterFightScreen)
            {
                if (!SaveManager.IsUnlockUnlocked("fightTutorial"))
                {
                    if (state.IsKeyUp(Keys.Enter))
                    {
                        tutorial = true;
                    }
                    gfx.spriteBatch.Draw(gfx.texture, new Rectangle(15, 15, MainGame.screenWidth - 30, MainGame.screenHeight - 30), Color.Black);
                    gfx.spriteBatch.DrawString(gfx.gameFont, "Fight tutorial", new Vector2(70, 50), ThemeColors.Text);
                    writer.WriteText("In fights you type to get\nenemy health to 0.\n\n" +
                        "Each letter gives you score based on\nyour letter score upgrades.\n\n" +
                        "Correct words give you extra score.\n\n" +
                        "Each consecutive correct word\nadds to your word streak\nand gives you extra score.\n\n" +
                        "Shiny words give you a multiplier\nStone words give you flat score\nBloom words upgrade letters\nin the written word.\n\n" +
                        "Press Enter to continue.", ThemeColors.Text, treasure: true, xExtraOffset: -30, yExtraOffset: -70);
                    if (tutorial && state.IsKeyDown(Keys.Enter))
                    {
                        SaveManager.UnlockUnlock("fightTutorial");
                        tutorial = false;
                    }

                }
                else
                {
                    if (state.IsKeyDown(Keys.Tab) && timeInSeconds == 0)
                    {
                        Inventory(state);
                        TopBannerDisplay(false);
                    }
                    else
                    {
                        writer.WriteText(neededText, ThemeColors.Selected, shinyWords, stoneWords, bloomWords, isHintText: true, rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset);
                        Vector2 lastCharPos = writer.UserInputText(Writer.writtenText.ToArray(), rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset);
                        TopBannerDisplay(false);
                        CalculateScore(lastCharPos);
                        HealthBar();

                        Keys[] currKeys = state.GetPressedKeys();
                        if (Writer.diffIndexes.Count > prevMistakes)
                        {
                            pitch = 0f;
                            prevMistakes = Writer.diffIndexes.Count;
                        }
                        foreach (var key in currKeys)
                        {
                            if (!prevKeys.Contains(key))
                            {
                                sfx.typeSound.Play(0.1f, pitch, 0f);

                                pitch += 0.005f;
                                if (pitch > 1.0f) pitch = 1.0f;
                                break;
                            }
                        }

                        prevKeys = currKeys;

                        if (eyeOfHorusActive) gfx.spriteBatch.Draw(gfx.texture, new Rectangle(0, 0, MainGame.screenWidth, MainGame.screenHeight), Color.Black);
                        if (!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.Cat))
                        {
                            gfx.spriteBatch.Draw(gfx.catPic, new Rectangle((int)catPos.X, (int)catPos.Y, 120, 80), Color.White);
                        }
                        if (Writer.writtenText.Count == neededText.Length || currentScore >= fight.scoreNeeded) isFightFinished = true;
                    }
                }
            }
            else if (state.IsKeyDown(Keys.Tab))
            {
                Inventory(state);
                TopBannerDisplay(false);
            }
            else if (selectedNode.type == NodeType.TREASURE)
            {
                isFightFinished = treasure.DisplayTreasure(ref coins, ref mousePressed);
                TopBannerDisplay(false);
            }
            else if (selectedNode.type == NodeType.SHOP)
            {
                if (!SaveManager.IsUnlockUnlocked("shopTutorial"))
                {
                    if (state.IsKeyUp(Keys.Enter))
                    {
                        tutorial = true;
                    }
                    gfx.spriteBatch.Draw(gfx.texture, new Rectangle(15, 15, MainGame.screenWidth - 30, MainGame.screenHeight - 30), Color.Black);
                    gfx.spriteBatch.DrawString(gfx.gameFont, "Shop tutorial", new Vector2(70, 50), ThemeColors.Text);
                    writer.WriteText("Navigate with arrow keys\nthrough the shop.\n\n" +
                        "Purchase upgrades by pressing enter.\n\n" +
                        "Check inventory by pressing tab.\n\n" +
                        "The prices are calculated based\non previous upgrades.\n\n" +
                        "Press Enter to continue.", ThemeColors.Text, treasure: true, xExtraOffset: -30, yExtraOffset: -70);
                    if (tutorial && state.IsKeyDown(Keys.Enter))
                    {
                        SaveManager.UnlockUnlock("shopTutorial");
                        tutorial = false;
                    }

                }
                else
                {
                    isFightFinished = shop.DisplayShop(ref coins, ref mousePressed);
                    TopBannerDisplay(false);
                }
            }
            else if (selectedNode.type == NodeType.CURSE)
            {
                isFightFinished = curseRoom.CurseRoomDisplay(ref coins, ref mousePressed);
            }

        }

        //After fight checks if the player beat the needed score, and handles the rewards
        private void FightFinished(KeyboardState state)
        {
            MouseState mouseState = Mouse.GetState();
            lastSelectedNode = selectedNode;
            lastActions = actions;
            if (IsFight(selectedNode.type))
            {

                if (!afterFightScreen)
                {
                    currentScore *= (long)((GlyphManager.IsActive(Glyph.Flower) ? (1 + 0.1 * GlyphManager.GetGlyphCount()) : 1) *
                        (GlyphManager.IsActive(Glyph.Water) ? 2 : 1) * (GlyphManager.IsActive(Glyph.Heart) ? (Writer.diffIndexes.Count > 0 ? 3 : 0.5) : 1));
                    if (currentScore >= fight.scoreNeeded)
                    {
                        double cashMultiply = (GlyphManager.IsActive(Glyph.Woman) ? 0.8 : 1) * (GlyphManager.IsActive(Glyph.Man) ? 1.5 : 1);
                        coins += (int)(fight.cashGain * cashMultiply);
                        coinsGained += (int)(fight.cashGain * cashMultiply);
                        if (coins > maxCoins) maxCoins = coins;
                        if (coins >= 200 && !achievmentBools["JERA0"])
                        {
                            achievmentBools["JERA0"] = true;
                            writeAchievment = true;
                        }
                        if (coins >= 100 && !achievmentBools["HUNDRED"])
                        {
                            achievmentBools["HUNDRED"] = writeAchievment = true;
                            writeAchievment = true;
                        }
                        if (GlyphManager.IsActive(Glyph.B)) enhancements.AddToStartingScore(5);

                        if (GlyphManager.IsActive(Glyph.Woman))
                        {
                            if (!isReplay) actions.Add(new UserAction("randomLetter", ""));
                            enhancements.MultiplyLetterScore((char)(seededRandom.Next(0, 26) + 'a'), 2);
                        }
                        if (Writer.writtenText.Count >= neededText.Length-10)
                        {
                            if (!achievmentBools["HEART"])
                            {
                                achievmentBools["HEART"] = true;
                                writeAchievment = true;
                            }
                        }
                    }
                    else
                    {
                        if (GlyphManager.IsActive(Glyph.Osiris))
                        {
                            enhancements.AllLettersMultiplyScore(0.8);
                        }
                        else
                        {
                            dead = true;
                        }
                    }
                    afterFightScreen = true;

                    totalScore += currentScore;
                    if (currentScore > maxScore) maxScore = currentScore;
                    mistakesWritten += Writer.diffIndexes.Count;
                    lettersWritten += Writer.writtenText.Count;
                    wordsWritten += Writer.writtenText.Count(c => c == ' ') + 1;

                    int valMin = 1, valMax = 4;
                    bool mult = false;
                    if (selectedNode.type == NodeType.ELITE)
                    {
                        valMin = 3;
                        valMax = 6;
                    }
                    if (selectedNode.type == NodeType.BOSS)
                    {
                        valMin = 2;
                        mult = true;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        if (!isReplay) actions.Add(new UserAction("randomLetter", ""));
                        if (!isReplay) actions.Add(new UserAction("randomLetter", ""));
                        cards.Add(new LetterUpgrade((char)(seededRandom.Next(0, 26) + 'a'), mult, seededRandom.Next(valMin, valMax), 0));
                    }

                }
                else
                {
                    if (afterFightMove && state.IsKeyDown(Keys.Left) && afterFightSelect > 0)
                    {
                        afterFightSelect--;
                        afterFightMove = false;
                    }
                    if (afterFightMove && state.IsKeyDown(Keys.Right) && afterFightSelect < cards.Count - 1)
                    {
                        afterFightSelect++;
                        afterFightMove = false;
                    }
                    if (state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right))
                    {
                        afterFightMove = true;
                    }
                    if (state.IsKeyUp(Keys.Tab))
                    {
                        if (selectedNode.type == NodeType.BOSS && level == 3)
                        {
                            if (!gameWin)
                            {
                                GlyphManager.RemoveAllGlyphs();
                                if (SteamUserStats.GetStat("runs_won", out int runsWon))
                                {
                                    SteamUserStats.SetStat("runs_won", runsWon + 1);
                                    if (!achievmentBools["MAN"])
                                    {
                                        achievmentBools["MAN"] = true;
                                        writeAchievment = true;
                                    }
                                    if (runsWon >= 5 && !achievmentBools["KING"])
                                    {
                                        achievmentBools["KING"] = true;
                                        writeAchievment = true;
                                    }
                                }
                                gameWin = true;
                            }
                            string fightWon = "You won the run";
                            gfx.spriteBatch.DrawString(gfx.gameFont, fightWon, new Vector2(MainGame.screenWidth / 2 - gfx.gameFont.MeasureString(fightWon).X / 2, 70), ThemeColors.Text);

                            var letters = enhancements.GetBestLetter();
                            gfx.spriteBatch.DrawString(gfx.smallTextFont, "Words written: " + wordsWritten + "\nLetters written:" + lettersWritten + "\nMistakes: " + mistakesWritten + "\nAccuracy: " + (int)((1.0 - (double)mistakesWritten / lettersWritten) * 100) + "%", new Vector2(100, 150), ThemeColors.Text);
                            gfx.spriteBatch.DrawString(gfx.smallTextFont, "Most upgraded letter: " + letters.bestLetter + ":  " + letters.bestLetterNum + "\nTotal score: " + totalScore + "\nMax score: " + maxScore, new Vector2(450, 150), ThemeColors.Text);
                            gfx.spriteBatch.DrawString(gfx.smallTextFont, "Shiny words: " + shinyWritten + "\nStone words: " + stoneWritten + "\nBloom words: " + bloomWritten, new Vector2(100, 300), ThemeColors.Text);
                            gfx.spriteBatch.DrawString(gfx.smallTextFont, "Highest streak: " + highestStreak + "\nCoins gained: " + coinsGained + "\nMax coins: " + maxCoins, new Vector2(450, 300), ThemeColors.Text);

                            gfx.spriteBatch.DrawString(gfx.gameFont, "Press enter to continue", new Vector2(MainGame.screenWidth / 2 - gfx.gameFont.MeasureString("Press enter to continue").X / 2, 450), ThemeColors.Text);

                            string achievmentName = (((Runes.Runes)selectedRune).ToString() + (difficulty + 1)).ToString().ToLower();
                            SaveManager.UnlockUnlock(achievmentName);
                            SteamUserStats.SetAchievement(achievmentName.ToUpper());
                            if (!mistake && !achievmentBools["STAR"])
                            {
                                achievmentBools["STAR"] = writeAchievment = true;
                                writeAchievment = true;
                            }
                            if (state.IsKeyDown(Keys.Enter))
                            {
                                SaveManager.RemoveGameData();
                                GlyphManager.RemoveAllGlyphs();
                                gameSaveData = null;
                                Reset();
                                gameState = GameState.MENU;
                            }
                        }
                        else
                        {
                            Color cardColor;
                            for (int i = 0; i < cards.Count; i++)
                            {
                                cardColor = (i == afterFightSelect) ? ThemeColors.Selected : ThemeColors.Foreground;
                                Rectangle rewardRect = new Rectangle(MainGame.screenWidth / 5 * (i + 1), 250, 160, 120);
                                if (mouseState.LeftButton == ButtonState.Released) mousePressed = false;
                                if(rewardRect.Contains(mouseState.Position) && !keyboardUsed){
                                    if (!mousePressed && mouseState.LeftButton == ButtonState.Pressed)
                                    {
                                        mousePressed = true;
                                        FightToMap();
                                    }
                                    afterFightSelect = i;
                                }
                                
                                gfx.spriteBatch.Draw(gfx.texture, rewardRect, cardColor);
                                gfx.spriteBatch.DrawString(gfx.gameFont, cards[i].letter + (cards[i].mult ? "  *" : "  +") + cards[i].value, new Vector2(MainGame.screenWidth / 5 * (i + 1) + 25, 250 + 30), ThemeColors.Text);
                                gfx.spriteBatch.DrawString(gfx.smallTextFont, "Current: " + enhancements.GetLetterScore(cards[i].letter), new Vector2(MainGame.screenWidth / 5 * (i + 1) + 20, 250 + 90), ThemeColors.Text);
                            }

                            string fightWon = "Fight won";
                            gfx.spriteBatch.DrawString(gfx.menuFont, fightWon, new Vector2(MainGame.screenWidth / 2 - gfx.gameFont.MeasureString(fightWon).X / 2, 70), ThemeColors.Text);
                            string chooseReward = "Choose your reward";
                            gfx.spriteBatch.DrawString(gfx.menuFont, chooseReward, new Vector2(MainGame.screenWidth / 2 - gfx.gameFont.MeasureString(chooseReward).X / 2, 130), ThemeColors.Text);
                            if (state.IsKeyDown(Keys.Enter))
                            {
                                FightToMap();
                            }
                        }

                    }
                }
            }
            else
            {
                roomSelected = false;
                canStartFight = false;
                SaveManager.SaveGame(seed, level, coins, lastSelectedNode, enhancements, difficulty, selectedRune, visitedNodes);
                SaveManager.SaveActions(lastActions);
            }
        }

        private void FightToMap()
        {
            if (cards[afterFightSelect].mult)
            {
                enhancements.MultiplyLetterScore(cards[afterFightSelect].letter, cards[afterFightSelect].value);
            }
            else
            {
                enhancements.AddLetterScore(cards[afterFightSelect].letter, cards[afterFightSelect].value);
            }
            SaveManager.SaveGame(seed, level, coins, lastSelectedNode, enhancements, difficulty, selectedRune, visitedNodes);
            SaveManager.SaveActions(lastActions);
            roomSelected = false;
            canStartFight = false;
            if (selectedNode.type == NodeType.BOSS)
            {
                if (!achievmentBools["NAUDHIZ0"])
                {
                    achievmentBools["NAUDHIZ0"] = true;
                    writeAchievment = true;
                }
                if (Writer.diffIndexes.Count == 0 && !achievmentBools["R"])
                {
                    achievmentBools["R"] = true;
                    writeAchievment = true;
                }
                level++;
                visitedNodes = new List<int[]>();
                map.GenerateNodes();
                selectedNode = map.GetFirstNode();

            }
            if (selectedNode.type == NodeType.ELITE && !achievmentBools["S"])
            {
                achievmentBools["S"] = writeAchievment = true;
                writeAchievment = true;
            }

            if (Writer.diffIndexes.Count >= 10 && !achievmentBools["EYEOFHORUS"])
            {
                achievmentBools["EYEOFHORUS"] = writeAchievment = true;
                writeAchievment = true;
            }

            int letters = Writer.writtenText.Count;
            if (SteamUserStats.GetStat("letters", out int lettersCount))
            {
                SteamUserStats.SetStat("letters", lettersCount + letters);
                if (lettersCount >= 1000 && !achievmentBools["THOUSAND"])
                {
                    achievmentBools["THOUSAND"] = writeAchievment = true;
                    writeAchievment = true;
                }
                SteamUserStats.StoreStats();
            }

            int words = 0;
            foreach (char letter in Writer.writtenText)
            {
                if (letter == ' ') words++;
            }
            if (SteamUserStats.GetStat("words", out int wordsCount))
            {
                SteamUserStats.SetStat("words", wordsCount + words + 1);
                if (wordsCount >= 100 && !achievmentBools["PAPYRUS"])
                {
                    achievmentBools["PAPYRUS"] = writeAchievment = true;
                    writeAchievment = true;
                }
                SteamUserStats.StoreStats();
            }

            if (SteamUserStats.GetStat("fights_won", out int fightsWon))
            {
                SteamUserStats.SetStat("fights_won", fightsWon++);
                if (fightsWon >= 10 && !achievmentBools["N"])
                {
                    achievmentBools["N"] = true;
                    writeAchievment = true;
                }
                if (fightsWon >= 100 && !achievmentBools["WATER"])
                {
                    achievmentBools["WATER"] = true;
                    writeAchievment = true;
                }
                SteamUserStats.StoreStats();
            }
            GlyphManager.SetUnlockedGlyphs();
        }

        //Handles the map logic and the selection of new rooms
        private void MapHandler(KeyboardState state)
        {
            if (!SaveManager.IsUnlockUnlocked("mapTutorial"))
            {
                if (state.IsKeyUp(Keys.Enter))
                {
                    tutorial = true;
                }
                gfx.spriteBatch.Draw(gfx.texture, new Rectangle(15, 15, MainGame.screenWidth - 30, MainGame.screenHeight - 30), Color.Black);
                gfx.spriteBatch.DrawString(gfx.gameFont, "Map tutorial", new Vector2(70, 50), ThemeColors.Text);
                writer.WriteText("Press up or down to choose which room \nto go to on your current floor.\n\n" +
                    "F-Fight E-Elite B-Boss are fights.\nHere you type to defeat the enemies\nto get to the new floor.\n\n" +
                    "$-Shop lets you buy upgrades for coins.\n\n" +
                    "X-Treasure gives you a free glyph.\n\n" +
                    "C-Curse gives you bonuses for a sacrifice.\n\n" +
                    "Press Enter to continue.", ThemeColors.Text, treasure: true, xExtraOffset: -30, yExtraOffset: -70);
                if (tutorial && state.IsKeyDown(Keys.Enter))
                {
                    SaveManager.UnlockUnlock("mapTutorial");
                    tutorial = false;
                }

            }
            else
            {
                if (!firstEnter || state.IsKeyUp(Keys.Enter))
                {
                    firstEnter = false;
                    if (state.IsKeyUp(Keys.Tab) && !inventoryUp)
                    {

                        MapNode newNode = map.NodeSelect(selectedNode, ref mousePressed);

                        if (newNode != selectedNode)
                        {
                            visitedNodes.Add(new int[] { selectedNode.column, selectedNode.row });
                            Reset();
                            if (GlyphManager.IsActive(Glyph.Cat)) catPos = new Vector2(unseededRandom.Next(100, MainGame.screenWidth - 100), unseededRandom.Next(100, MainGame.screenHeight - 100));
                            if (newNode.type == NodeType.RANDOM) newNode.type = map.GenerateNodeTypeFromRandom();
                            switch (newNode.type)
                            {
                                case NodeType.FIGHT:
                                    fight = new NormalFight(level, newNode.column);
                                    break;
                                case NodeType.ELITE:
                                    fight = new EliteFight(level, newNode.column);
                                    break;
                                case NodeType.BOSS:
                                    fight = new BossFight(level, newNode.column);
                                    break;
                                case NodeType.TREASURE:
                                    treasure.NewGlyph();
                                    break;
                                case NodeType.SHOP:
                                    shop.NewShop();
                                    if (GlyphManager.IsActive(Glyph.Life))
                                    {
                                        if (!isReplay) actions.Add(new UserAction("randomLetter", ""));
                                        enhancements.AddLetterScore((char)(seededRandom.Next(0, 26) + 'a'), 5);
                                    }
                                    break;
                                case NodeType.CURSE:
                                    curseRoom.NewCurse();
                                    break;
                            }
                            if (IsFight(newNode.type)) neededText = RandomTextGenerate(fight.words + (GlyphManager.IsActive(Glyph.Papyrus) ? 20 : 0) - (difficulty >= 5 ? 5 : 0));
                            if (difficulty >= 4) fight.speed *= 2;
                            Writer.writtenText.Clear();
                            startedTyping = false;
                            lastSelectedNode = selectedNode;
                            selectedNode = newNode;
                            roomSelected = true;
                        }
                    }

                }
                TopBannerDisplay(true);
                if (state.IsKeyDown(Keys.Tab))
                {
                    Inventory(state);
                }
                else if(!inventoryUp)
                {
                    map.DrawNodes();
                }
            }
        }

        private void Reset()
        {
            lastActions = new List<UserAction>(actions);
            enhancements.ResetChange();
            isFightFinished = false;
            afterFightScreen = false;
            lastCharCount = 0;
            lastWordCount = 1;
            lastCorrectWord = 0;
            extraScore = 0;
            lastScore = 0;
            wordStreak = 0;
            inventoryGlyphSelect = 1;
            afterFightSelect = 0;
            pitch = 0;
            prevMistakes = 0;
            cards.Clear();
            shinyWords.Clear();
            stoneWords.Clear();
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

        private void TopBannerDisplay(bool onMap)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            inventoryUp = false;
            if(mouseState.LeftButton == ButtonState.Released)
            {
                inventoryMousePressed = false;
            }

            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(15, 15, MainGame.screenWidth - 30, 40), ThemeColors.Foreground);
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(15, 15, MainGame.screenWidth - 30, 40), ThemeColors.Foreground);
            Vector2 textOffset = new Vector2(30, 20);
            gfx.spriteBatch.DrawString(gfx.gameFont, $"coins:{coins}", textOffset, ThemeColors.Text);
            //if (!tabPressed) gfx.spriteBatch.DrawString(gfx.gameFont, "tab -> inventory", new Vector2(MainGame.screenWidth / 2 - gfx.gameFont.MeasureString("tab -> inventory").X / 2, textOffset.Y), ThemeColors.Text);
            gfx.spriteBatch.DrawString(gfx.gameFont, $"level:{level}/3", new Vector2(MainGame.screenWidth - gfx.gameFont.MeasureString($"level:{level}/3").X - textOffset.X, textOffset.Y), ThemeColors.Text);

            if (onMap && !keyboardState.IsKeyDown(Keys.Tab))
            {
                Rectangle inventoryRect = new Rectangle(MainGame.screenWidth - 70, 120, 45, 45);
                Color invIconColor = ThemeColors.NotSelected;
                if (inventoryRect.Contains(mouseState.Position) || (inventoryMousePressed && mouseState.LeftButton == ButtonState.Pressed))
                {
                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        inventoryMousePressed = true;
                        Inventory();
                    }
                    invIconColor = ThemeColors.Selected;
                }
                if (!inventoryUp)
                {
                    gfx.spriteBatch.Draw(gfx.texture, inventoryRect, invIconColor);
                    gfx.spriteBatch.DrawString(gfx.gameFont, "i", new Vector2(inventoryRect.X + 20, inventoryRect.Y + 7), ThemeColors.Text);
                }

                Rectangle exitRect = new Rectangle(MainGame.screenWidth - 70, 65, 45, 45);
                Color exitIconColor = ThemeColors.NotSelected;
                if (exitRect.Contains(mouseState.Position))
                {
                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        gameSaveData = SaveManager.LoadGame();

                        gameState = GameState.MENU;
                    }
                    exitIconColor = ThemeColors.Selected;
                }
                if (!inventoryUp)
                {
                    gfx.spriteBatch.Draw(gfx.texture, exitRect, exitIconColor);
                    gfx.spriteBatch.DrawString(gfx.gameFont, "<", new Vector2(exitRect.X + 15, exitRect.Y + 8), ThemeColors.Text);
                }
            }
            
        }

        private static bool IsFight(NodeType nodeType)
        {
            if (nodeType == NodeType.FIGHT || nodeType == NodeType.ELITE || nodeType == NodeType.BOSS)
                return true;
            return false;
        }

        private void CalculateScore(Vector2 lastCharPos)
        {
            if (Writer.diffIndexes.Count > 0) mistake = true;
            long letterScore = 0;
            for (int i = 0; i < Writer.writtenText.Count; i++)
            {
                bool canAdd = true;
                for (int j = 0; j < Writer.diffIndexes.Count; j++)
                {
                    if (i == j) canAdd = false;
                }
                if (canAdd && Writer.writtenText[i] != ' ')
                {
                    letterScore += enhancements.letters[Writer.writtenText[i] - 'a'];
                }
            }

            if (Writer.writtenText.Count != lastCharCount)
            {
                if (GlyphManager.IsActive(Glyph.Thousand))
                {
                    charCounter++;
                    if (charCounter == 1000)
                    {
                        charCounter = 0;
                        extraScore += 1000;
                    }
                }
                lastScore = playerScore;
                letterTimer = timeInSeconds;
                lastCharCount = Writer.writtenText.Count;
            }

            if ((int)timeInSeconds == 60 && !achievmentBools["M"])
            {
                achievmentBools["M"] = true;
                writeAchievment = true;
            }

            gfx.spriteBatch.DrawString(gfx.gameFont, $"Streak:{wordStreak}".ToString(), new Vector2(50, 100), ThemeColors.Text);
            string rewardText = "Reward: " + fight.cashGain;
            gfx.spriteBatch.DrawString(gfx.gameFont, rewardText, new Vector2(MainGame.screenWidth - 50 - gfx.gameFont.MeasureString(rewardText).X, 100), ThemeColors.Text);

            int mistakeCount = Math.Max(Writer.diffIndexes.Count - (GlyphManager.IsActive(Glyph.EyeOfHorus) ? 2 : 0), 0);
            if (GlyphManager.IsActive(Glyph.Star) && mistakeCount > 0) dead = true;

            string userWords = new string(Writer.writtenText.ToArray());
            int correctWords = 0;

            List<int> neededStarts = new List<int>();

            int start = 0;
            for (int i = 0; i <= neededText.Length; i++)
            {
                if (i == neededText.Length || neededText[i] == ' ')
                {
                    if (i > start) neededStarts.Add(start);
                    start = i;
                }
            }


            int word = -1;
            double shinyMultiplier = 1;
            int stoneScore = 0;
            for (int i = 0; i < neededStarts.Count - 1; i++)
            {
                int wordLength = neededStarts[i + 1] - neededStarts[i] - 1;
                if (userWords.Length < neededStarts[i] + wordLength + 1) break;

                word++;
                //these (i==0?-1:0) are to adjust for the lack of spaces in the first word
                string neededWord = neededText.Substring(neededStarts[i] + 1 + (i==0?-1:0), wordLength + (i==0?1:0));
                string userWord = userWords.Substring(neededStarts[i] + 1 + (i==0?-1:0), wordLength + (i==0?1:0));
                if (userWord == neededWord)
                {
                    correctWords++;
                    if (shinyWords.Contains(word))
                    {
                        shinyMultiplier *= enhancements.shinyScore;
                    }
                    else if (stoneWords.Contains(word))
                    {
                        stoneScore += enhancements.stoneScore;
                    }
                }
            }

            if (userWords.Length != 0 && userWords.Length != lastWordCount && neededStarts.Contains(userWords.Length))
            {
                bool under3Sec = timeInSeconds - timeSinceLastWord < 3;
                if (!under3Sec && GlyphManager.IsActive(Glyph.N)) wordStreak = 0;
                timeSinceLastWord = timeInSeconds;
                lastWordCount = userWords.Length;
                wordCounter++;
                if (correctWords > lastCorrectWord)
                {
                    lastCorrectWord = correctWords;
                    wordStreak += GlyphManager.IsActive(Glyph.Scarab) ? 3 : 1;
                    if (wordStreak > highestStreak) highestStreak = wordStreak;
                    extraScore += wordStreak * (GlyphManager.IsActive(Glyph.N) && under3Sec ? 2 : 0);
                    if (shinyWords.Contains(word))
                    {
                        shinyWritten++;
                    }
                    else if (stoneWords.Contains(word))
                    {
                        stoneWritten++;
                    }
                    else if (bloomWords.Contains(word))
                    {
                        bloomWritten++;
                        string correctWord = userWords.Substring(neededStarts[word] + 1 + (word == 0 ? -1 : 0), neededStarts[word + 1] - neededStarts[word] - 1 + (word == 0 ? 1 : 0));
                        char[] correctWordChars = correctWord.ToCharArray();
                        foreach (char correctLetter in correctWordChars)
                        {
                            enhancements.AddLetterScore(correctLetter, 1);
                        }
                    }
                }
                else if (correctWords == lastCorrectWord)
                {
                    lastCorrectWord = correctWords;
                    wordStreak = 0;
                }
            }

            if (GlyphManager.IsActive(Glyph.Anubis) && wordCounter % 5 == 0)
            {
                if (!anubisActive && wordCounter > 0) coins++;
                anubisActive = true;
            }
            else anubisActive = false;

            if (startedTyping)
            {
                playerScore = (int)((extraScore + enhancements.startingScore + letterScore + stoneScore) * shinyMultiplier);
                long enemyDamage = (long)((GlyphManager.IsActive(Glyph.House) ? 0.25 : 1) * (int)timeInSeconds) * fight.speed - enhancements.damageResist;
                long mistakeDamage = (GlyphManager.IsActive(Glyph.Snake) ? 0 : 1) * (GlyphManager.IsActive(Glyph.R) ? 5 : 1) * (difficulty >= 2 ? 5 : 1) * mistakeCount;
                currentScore = playerScore + correctWords * enhancements.wordScore - (enemyDamage < 0 ? 1 * (int)timeInSeconds : enemyDamage) - mistakeDamage;
                if (Writer.writtenText.Count == 1) lastScore = 0;
                if (playerScore - lastScore != 0 && timeInSeconds - letterTimer < 0.25 && Writer.writtenText.Count > 0) gfx.spriteBatch.DrawString(gfx.gameFont, "+" + (playerScore - lastScore).ToString(), lastCharPos, ThemeColors.Correct);
            }
            else currentScore = enhancements.startingScore;
            if (currentScore <= -100 && !achievmentBools["SNAKE"])
            {
                achievmentBools["SNAKE"] = true;
                writeAchievment = true;
            }
        }

        private void Inventory(KeyboardState state = default)
        {
            MouseState mouseState = Mouse.GetState();
            inventoryUp = true;
            int columns = 4, rows = 8;
            int columnSpacing = (int)(MainGame.screenWidth / 4.5);
            int leftOffset = 40;

            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (column * rows + row >= 26) break;
                    SpriteFont font = enhancements.overHundred? gfx.smallTextFont : gfx.gameFont;
                    gfx.spriteBatch.DrawString(font, (char)(column * rows + row + 'a') + ": " + enhancements.letters[column * rows + row], new Vector2(columnSpacing / 2 + column * columnSpacing - leftOffset, 70 + row * 40), ThemeColors.Text);
                    long change = enhancements.lettersChange[column * rows + row];
                    if (change != 0) gfx.spriteBatch.DrawString(font, (change < 0 ? "" : "+") + change, new Vector2(columnSpacing + column * columnSpacing + 25 - leftOffset, 70 + row * 40), change < 0 ? ThemeColors.Wrong : ThemeColors.Correct);
                }
            }

            int lineRow = -3, changeOffset = 150;
            gfx.spriteBatch.DrawString(gfx.smallTextFont, $"Shiny: {(int)(enhancements.shinyChance * 100)}%", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if(enhancements.shChange != 0)gfx.spriteBatch.DrawString(gfx.smallTextFont, $"+{(int)(enhancements.shChange*100)}%", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);
            gfx.spriteBatch.DrawString(gfx.smallTextFont, $"Stone: {(int)(enhancements.stoneChance * 100)}%", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if(enhancements.stChange != 0)gfx.spriteBatch.DrawString(gfx.smallTextFont, $"+{(int)(enhancements.stChange*100)}%", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);
            gfx.spriteBatch.DrawString(gfx.smallTextFont, $"Bloom: {(int)(enhancements.bloomChance * 100)}%", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if(enhancements.blChange != 0)gfx.spriteBatch.DrawString(gfx.smallTextFont, $"+{(int)(enhancements.blChange*100)}%", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);

            lineRow++;
            gfx.spriteBatch.DrawString(gfx.smallTextFont, $"Shiny mult: {enhancements.shinyScore.ToString("0.##")}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if(enhancements.shinyScoreChange != 0)gfx.spriteBatch.DrawString(gfx.smallTextFont, $"+{enhancements.shinyScoreChange}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);
            gfx.spriteBatch.DrawString(gfx.smallTextFont, $"Stone add: {enhancements.stoneScore}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if(enhancements.stoneScoreChange != 0)gfx.spriteBatch.DrawString(gfx.smallTextFont, $"+{enhancements.stoneScoreChange}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);


            lineRow++;
            gfx.spriteBatch.DrawString(gfx.smallTextFont, $"Streak: {enhancements.wordScore}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if(enhancements.wordChange != 0)gfx.spriteBatch.DrawString(gfx.smallTextFont, $"+{enhancements.wordChange}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);
            gfx.spriteBatch.DrawString(gfx.smallTextFont, $"Resist: {enhancements.damageResist}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if(enhancements.damageChange != 0)gfx.spriteBatch.DrawString(gfx.smallTextFont, $"+{enhancements.damageChange}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);
            gfx.spriteBatch.DrawString(gfx.smallTextFont, $"Start: {enhancements.startingScore}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset, 75 + 3 * 40 + ++lineRow * 24), ThemeColors.Text);
            if(enhancements.startChange != 0)gfx.spriteBatch.DrawString(gfx.smallTextFont, $"+{enhancements.startChange}", new Vector2(columnSpacing / 2 + 3 * columnSpacing - leftOffset + changeOffset, 75 + 3 * 40 + lineRow * 24), ThemeColors.Correct);

            Glyph[] glyphs = GlyphManager.GetGlyphs();
            if (glyphs.Length > 1)
            {
                if (state.IsKeyDown(Keys.Left) && inventoryMove && inventoryGlyphSelect > 1)
                {
                    inventoryGlyphSelect--;
                    inventoryMove = false;
                }
                if (state.IsKeyDown(Keys.Right) && inventoryMove && inventoryGlyphSelect < GlyphManager.GetGlyphCount() - 1)
                {
                    inventoryGlyphSelect++;
                    inventoryMove = false;
                }
                if (state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right)) inventoryMove = true;

                int borderOffset = 5, imageSize = 64, yOffset = 400, descOffset = 80, xColumnOffset = 80, xSideOffset = -30;
                gfx.spriteBatch.Draw(gfx.texture, new Rectangle(xColumnOffset * inventoryGlyphSelect - borderOffset + xSideOffset, yOffset - borderOffset, imageSize + borderOffset * 2, imageSize + borderOffset * 2), ThemeColors.Selected);
                gfx.spriteBatch.DrawString(gfx.smallTextFont, GlyphManager.GetDescription(glyphs[inventoryGlyphSelect]), new Vector2(xColumnOffset + xSideOffset, yOffset + descOffset), ThemeColors.Text);
                columns = 0;
                foreach (Glyph glyph in glyphs)
                {
                    if (glyph != Glyph.NoGlyphsLeft)
                    {
                        Rectangle glyphRect = new Rectangle(xColumnOffset * columns + xSideOffset, yOffset, imageSize, imageSize);
                        if (glyphRect.Contains(mouseState.Position)) inventoryGlyphSelect = columns;
                        gfx.spriteBatch.Draw(GlyphManager.GetGlyphImage(glyph), glyphRect, ThemeColors.Foreground);
                    }
                    columns++;
                }
            }
        }

        private void HealthBar()
        {
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(40, 60, MainGame.screenWidth - 80, 35), ThemeColors.Selected);
            int redBarLength = (int)((double)Math.Min(fight.scoreNeeded, fight.scoreNeeded - currentScore) / fight.scoreNeeded * (MainGame.screenWidth - 90));
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(45, 65, redBarLength, 25), ThemeColors.Foreground);
            string score = $"{currentScore}/{fight.scoreNeeded}  -{fight.speed}/s";
            gfx.spriteBatch.DrawString(gfx.smallTextFont, score, new Vector2(MainGame.screenWidth / 2 - gfx.smallTextFont.MeasureString(score).X/2, 68), ThemeColors.NotSelected);
        }

        private void CharacterChoose()
        {
            KeyboardState state = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            int maxRunes = Enum.GetValues(typeof(Runes.Runes)).Length;
            Rectangle backRect = new Rectangle(50, 50, 50, 50);
            Color backSelected = ThemeColors.NotSelected;
            if (backRect.Contains(mouseState.Position)) backSelected = ThemeColors.Selected;
            gfx.spriteBatch.Draw(gfx.texture, backRect, backSelected);
            gfx.spriteBatch.DrawString(gfx.menuFont, "<", new Vector2(67, 57), ThemeColors.Text);
            if (state.IsKeyDown(Keys.Escape) || (!mousePressed && backRect.Contains(mouseState.Position) && mouseState.LeftButton == ButtonState.Pressed))
            {
                gameState = GameState.MENU;
                gameSaveData = SaveManager.LoadGame();
            }
            if (canStartFight && (state.IsKeyDown(Keys.Enter)))
            {
                if (SaveManager.IsUnlockUnlocked((((Runes.Runes)selectedRune).ToString() + difficulty).ToString().ToUpper()) || (Runes.Runes)selectedRune == Runes.Runes.Uruz)
                {
                    NewGameChoiceUpdate();
                    gameState = GameState.LOADGAME;
                }
            }
            else if (state.IsKeyUp(Keys.Enter))
            {
                canStartFight = true;
            }

            if (mouseState.LeftButton == ButtonState.Released) mousePressed = false;

            int rectWidth = MainGame.screenWidth / 3, rectHeight = MainGame.screenHeight / 2;
            if (runeMove)
            {
                if (state.IsKeyDown(Keys.Left))
                {
                    if (!diffMove && selectedRune != 0)
                    {
                        selectedRune--;
                        difficulty = 0;
                    }
                    else if (diffMove && difficulty != 0)
                    {
                        difficulty--;
                    }

                    runeMove = false;
                }
                if (state.IsKeyDown(Keys.Right))
                {
                    if (!diffMove && selectedRune != maxRunes - 1)
                    {
                        selectedRune++;
                        difficulty = 0;
                    }
                    else if (diffMove && difficulty != 5)
                    {
                        difficulty++;
                    }
                    runeMove = false;
                }
            }
            if (state.IsKeyDown(Keys.Down)) diffMove = true;
            else if (state.IsKeyDown(Keys.Up)) diffMove = false;

            if (!runeMove)
            {
                if (state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right)) runeMove = true;
            }

            if (selectedRune != 0)
            {
                Rectangle runeSelect = new Rectangle(MainGame.screenWidth / 5 - rectWidth / 4, MainGame.screenHeight / 3 - rectHeight / 4, rectWidth / 2, rectHeight / 2);
                if (!mousePressed && runeSelect.Contains(mouseState.Position) && mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (!diffMove && selectedRune != 0)
                    {
                        selectedRune--;
                        difficulty = 0;
                    }
                    mousePressed = true;
                    diffMove = false;
                }
                gfx.spriteBatch.Draw(gfx.texture, runeSelect, ThemeColors.NotSelected);
                string runeName = ((Runes.Runes)selectedRune - 1).ToString().Substring(0, 1);
                gfx.spriteBatch.DrawString(gfx.menuFont, runeName, new Vector2(MainGame.screenWidth / 4 - gfx.menuFont.MeasureString(runeName).X / 2, MainGame.screenHeight / 3 - gfx.menuFont.MeasureString(runeName).Y / 2), ThemeColors.Text);
                gfx.spriteBatch.DrawString(gfx.menuFont, "<", new Vector2(MainGame.screenWidth / 3 - gfx.menuFont.MeasureString("<").X * 2, MainGame.screenHeight / 3.5f), ThemeColors.Text);
            }
            {
                Rectangle runeSelect = new Rectangle(MainGame.screenWidth / 2 - rectWidth / 2, MainGame.screenHeight / 3 - rectHeight / 2, rectWidth, rectHeight);
                if (runeSelect.Contains(mouseState.Position))
                {
                    if (!mousePressed && mouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (SaveManager.IsUnlockUnlocked((((Runes.Runes)selectedRune).ToString() + difficulty).ToString().ToUpper()) || (Runes.Runes)selectedRune == Runes.Runes.Uruz)
                        {
                            NewGameChoiceUpdate();
                            gameState = GameState.LOADGAME;
                        }
                        mousePressed = true;
                    }
                    diffMove = false;
                }
                
                gfx.spriteBatch.Draw(gfx.texture, runeSelect, diffMove ? ThemeColors.NotSelected : ThemeColors.Selected);
                string runeName = ((Runes.Runes)selectedRune).ToString();
                int topOffset = 10;
                gfx.spriteBatch.DrawString(gfx.menuFont, runeName, new Vector2(MainGame.screenWidth / 2 - gfx.menuFont.MeasureString(runeName).X / 2, MainGame.screenHeight / 3 - gfx.menuFont.MeasureString(runeName).Y * 3 + topOffset * 2), ThemeColors.Text);
                if (SaveManager.IsUnlockUnlocked((((Runes.Runes)selectedRune).ToString() + 0).ToString().ToUpper()) || (Runes.Runes)selectedRune == Runes.Runes.Uruz)
                {
                    var field = ((Runes.Runes)selectedRune).GetType().GetField(((Runes.Runes)selectedRune).ToString());
                    var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));

                    string[] descStrings = attribute.GetDescription().Split('\n');
                    int line = 0;
                    foreach (string desc in descStrings)
                    {
                        if(line == 0)gfx.spriteBatch.DrawString(gfx.smallTextFont, desc, new Vector2(MainGame.screenWidth / 2 - gfx.gameFont.MeasureString(desc).X / 4 + 7, MainGame.screenHeight / 3 - gfx.gameFont.MeasureString(runeName).Y * (2 - line++) + topOffset), ThemeColors.Text);
                        else gfx.spriteBatch.DrawString(gfx.gameFont, desc, new Vector2(MainGame.screenWidth / 2 - gfx.gameFont.MeasureString(desc).X / 2, MainGame.screenHeight / 3 - gfx.gameFont.MeasureString(runeName).Y * (2 - line++) + topOffset - 20), ThemeColors.Text);
                    }
                }
                else
                {
                    var field = ((Runes.Runes)selectedRune).GetType().GetField(((Runes.Runes)selectedRune).ToString());
                    var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));

                    string[] descStrings = attribute.GetPrompt().Split('\n');
                    int line = 0;
                    foreach (string desc in descStrings)
                    {
                        gfx.spriteBatch.DrawString(gfx.gameFont, desc, new Vector2(MainGame.screenWidth / 2 - gfx.gameFont.MeasureString(desc).X / 2, MainGame.screenHeight / 3 - gfx.gameFont.MeasureString(runeName).Y * (2 - line++) + topOffset), ThemeColors.Wrong);
                    }
                }
            }
            if (selectedRune != maxRunes - 1)
            {
                Rectangle runeSelect = new Rectangle(MainGame.screenWidth - MainGame.screenWidth / 5 - rectWidth / 4, MainGame.screenHeight / 3 - rectHeight / 4, rectWidth / 2, rectHeight / 2);
                if (!mousePressed && runeSelect.Contains(mouseState.Position) && mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (selectedRune != maxRunes - 1)
                    {
                        selectedRune++;
                        difficulty = 0;
                    }
                    mousePressed = true;
                    diffMove = false;
                }
                gfx.spriteBatch.Draw(gfx.texture, runeSelect, ThemeColors.NotSelected);
                string runeName = ((Runes.Runes)selectedRune + 1).ToString().Substring(0, 1);
                gfx.spriteBatch.DrawString(gfx.menuFont, runeName, new Vector2(MainGame.screenWidth - MainGame.screenWidth / 4 - gfx.menuFont.MeasureString(runeName).X / 2, MainGame.screenHeight / 3 - gfx.menuFont.MeasureString(runeName).Y / 2), ThemeColors.Text);
                gfx.spriteBatch.DrawString(gfx.menuFont, ">", new Vector2(MainGame.screenWidth - MainGame.screenWidth / 3 + gfx.menuFont.MeasureString(">").X, MainGame.screenHeight / 3.5f), ThemeColors.Text);
            }

            gfx.spriteBatch.DrawString(gfx.menuFont, "Difficulty: ", new Vector2(MainGame.screenWidth / 2.5f - gfx.menuFont.MeasureString("Difficulty: ").X, MainGame.screenHeight - 100), ThemeColors.Text);

            int padding = 10;
            
            string diffString = $"<{difficulty}: {DifficultyText(difficulty)}>";
            if (difficulty != 0 && !SaveManager.IsUnlockUnlocked((((Runes.Runes)selectedRune).ToString() + difficulty).ToString().ToLower()))
            {
                diffString = "<?>";
            }
            Vector2 diffStringSize = gfx.menuFont.MeasureString(diffString);
            Rectangle diffRectLeft = new Rectangle((int)(MainGame.screenWidth / 2.5f) - padding, MainGame.screenHeight - 100 - padding, ((int)diffStringSize.X + padding * 2)/2, (int)diffStringSize.Y + padding);
            Rectangle diffRectRight = new Rectangle((int)(MainGame.screenWidth / 2.5f) - padding + ((int)diffStringSize.X + padding * 2) / 2, MainGame.screenHeight - 100 - padding, ((int)diffStringSize.X + padding * 2) / 2, (int)diffStringSize.Y + padding);
            if (diffRectLeft.Contains(mouseState.Position) || diffRectRight.Contains(mouseState.Position))
            {
                if (!mousePressed && mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (diffRectLeft.Contains(mouseState.Position))
                    {
                        if (difficulty != 0)
                        {
                            difficulty--;
                        }
                    }
                    else if (diffRectRight.Contains(mouseState.Position))
                    {
                        if (difficulty != 5)
                        {
                            difficulty++;
                        }
                    }
                    mousePressed = true;
                }
                canStartFight = false;
                diffMove = true;
            }
            if (diffMove)
            {
                gfx.spriteBatch.Draw(gfx.texture, diffRectLeft, (diffString == "?") ? ThemeColors.Wrong : ThemeColors.Selected);
                gfx.spriteBatch.Draw(gfx.texture, diffRectRight, (diffString == "?") ? ThemeColors.Wrong : ThemeColors.Selected);
            }
            gfx.spriteBatch.DrawString(gfx.menuFont, diffString, new Vector2(MainGame.screenWidth / 2.5f, MainGame.screenHeight - 100), ThemeColors.Text);
        }


        private void NewGameChoiceUpdate()
        {
            switch (selectedRune)
            {
                case (int)Runes.Runes.Uruz:
                    enhancements.AllLettersAddScore(1);
                    break;
                case (int)Runes.Runes.Halagaz:
                    enhancements.AllLettersAddScore(-6);
                    for (int i = 0; i < 13; i++)
                    {
                        int index = unseededRandom.Next(0, 26);
                        while (enhancements.letters[index] != -5)
                        {
                            index = unseededRandom.Next(0, 26);
                        }
                        enhancements.AddLetterScore((char)('a' + index), 15);
                    }
                    break;
                case (int)Runes.Runes.Jera:
                    coins = 80;
                    break;
                case (int)Runes.Runes.Naudhiz:
                    enhancements.bloomChance = 0.1;
                    break;
            }
        }

        private string DifficultyText(int difficulty)
        {
            return difficulty switch
            {
                0 => " Normal",
                1 => " -15 coins",
                2 => " Mistakes -5",
                3 => " Word score -1",
                4 => " Double damage",
                5 => " -5 words",
                _ => "",
            };
        }

        private static void WriteAchievment()
        {
            if (writeAchievment)
            {
                foreach (var keyVal in achievmentBools)
                {
                    if (keyVal.Value)
                    {
                        SaveManager.UnlockUnlock(keyVal.Key);
                        SteamUserStats.SetAchievement(keyVal.Key);
                        Console.WriteLine($"Achievement unlocked: {keyVal.Key}");
                    }
                }
                writeAchievment = false;
            }
        }
    }
}
