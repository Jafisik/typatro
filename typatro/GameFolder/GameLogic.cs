using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        Point windowPos, dragOffset;
        bool eyeOfHorusActive, anubisActive, runeMove, diffMove, tutorial, isDragging, mUnlocked, mistake;
        SoundEffect typeSound;

        //Player
        Enhancements enhancements;
        long coins = 0, startCoins = 30;
        int selectedRune = 0, difficulty = 0, inventoryGlyphSelect = 1;
        bool dead, inventoryMove = true, tabPressed;

        //Score calculator
        int charCounter = 0, lastCharCount = 0, wordCounter = 0, lastWordCount = 1, wordStreak = 0, lastCorrectWord = 0, extraScore = 0;
        long currentScore = 0, playerScore = 0, lastScore = 0;
        double letterTimer = 0, timeSinceLastWord = 0;
        
        //Rooms
        Fight fight;
        bool canStartFight, startedTyping, roomSelected, isFightFinished;
        bool afterFightScreen, afterFightMove;
        int afterFightSelect = 0;
        List<Card> cards = new List<Card>();
        Treasure treasure;
        Shop shop;
        

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
        double textRotation = 0;
        Vector2 catPos = Vector2.One;

        //Map
        Map map;
        List<int[]> visitedNodes = new List<int[]>();
        MapNode selectedNode, lastSelectedNode;
        int level = 1;
        bool firstEnter = true;

        public GameLogic(MainGame.Gfx gfx, List<string> jsonStrings, Point windowPos, SoundEffect typeSound)
        {
            this.gfx = gfx;
            this.jsonStrings = jsonStrings;
            this.windowPos = windowPos;
            this.typeSound = typeSound;

            seed = seededRandom.Next();
            seededRandom = new Random(seed);

            menu = new Menu(gfx);
            gameSaveData = SaveManager.LoadGame();

            map = new Map(gfx);

            writer = new Writer(gfx.spriteBatch, gfx.textFont);
            neededText = RandomTextGenerate(10);

            enhancements = new Enhancements();
            treasure = new Treasure(gfx, enhancements);
            shop = new Shop(gfx, enhancements);
            GlyphManager.SetUnlockedGlyphs();
        }

        public void Update(GameWindow window){
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed){
                if (new Rectangle(0,0,MainGame.screenWidth, MainGame.screenHeight).Contains(mouseState.Position)){
                    if (!isDragging){
                        isDragging = true;
                        dragOffset = new Point(mouseState.X, mouseState.Y);
                    }
                }
            }

            if (mouseState.LeftButton == ButtonState.Released)
            {
                isDragging = false;
            }

            if (isDragging){
                window.Position = new Point(mouseState.X + window.Position.X-dragOffset.X, mouseState.Y + window.Position.Y-dragOffset.Y);
            }
            if (gameState == GameState.NEWGAME || gameState == GameState.LOADGAME){
                if((int)timeInSeconds % 8 == 0 && timeInSeconds != 0){
                    if(!GlyphManager.IsActive(Glyph.House)){
                        writer.ReadKeyboardInput(MainGame.time);
                        writer.UpdateDiffIndexes(neededText);
                    }
                }
                else{
                    writer.ReadKeyboardInput(MainGame.time);
                    writer.UpdateDiffIndexes(neededText);
                }
                if (!startedTyping && Writer.writtenText.Count > 0){
                    startedTyping = true;
                    timeInSeconds = 0;
                }
                

                if (startedTyping) {
                    if(!isFightFinished){
                        timeInSeconds += MainGame.time.ElapsedGameTime.TotalSeconds;
                    }
                    else{
                        letterTimer = 0;
                    }
                    if((int)timeInSeconds != lastTime){
                        if((int)timeInSeconds % 10 == 0){
                            if(!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.H)) textRotation += Math.PI;
                            if(GlyphManager.IsActive(Glyph.M)) coins += 10;
                        }
                        if((int)timeInSeconds % 5 == 0 && timeInSeconds != 0){
                            if(!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.EyeOfHorus)) eyeOfHorusActive = true;
                            if(!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.M)){
                                xTextOffset = unseededRandom.Next(-100,101);
                                yTextOffset = unseededRandom.Next(0,101);
                            }
                            else{
                                xTextOffset = 0;
                                yTextOffset = 0;
                            }
                        } else eyeOfHorusActive = false;
                    }
                    lastTime = (int)timeInSeconds;
                    KeyboardState keyboardState = Keyboard.GetState();
                    if(!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.J)){
                        if(keyboardState.GetPressedKeyCount() > 0 && keyboardState.IsKeyUp(Keys.Tab)){
                            window.Position = new Point(windowPos.X+unseededRandom.Next(-5,6), windowPos.Y+unseededRandom.Next(-5,6));
                        }
                        else window.Position = windowPos;
                    }
                }
            }
            
        }

        float bgRotation = 0f;
        public void Draw(GraphicsDeviceManager graphicsDevice)
        {
            gfx.spriteBatch.Begin(SpriteSortMode.Deferred);
            bgRotation += 0.002f;

            Vector2 centerScreen = new Vector2(MainGame.screenWidth / 2f, MainGame.screenHeight / 2f);
            Vector2 bgOrigin = new Vector2(gfx.bg.Width / 2.5f, gfx.bg.Height / 2f);

            gfx.spriteBatch.GraphicsDevice.Clear(ThemeColors.Background);
            gfx.spriteBatch.Draw(gfx.bg,centerScreen,null,new Color(5, 15, 5),bgRotation,bgOrigin,2.5f,SpriteEffects.None,0f);
            int lineWidth = 15;
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(0, 0, MainGame.screenWidth, lineWidth), ThemeColors.Foreground);
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(0, 0, lineWidth, MainGame.screenHeight), ThemeColors.Foreground);
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(0, MainGame.screenHeight - lineWidth, MainGame.screenWidth, lineWidth), ThemeColors.Foreground);
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(MainGame.screenWidth - lineWidth, 0, lineWidth, MainGame.screenHeight), ThemeColors.Foreground);

            if (SaveManager.fullscreen == 1) graphicsDevice.IsFullScreen = true;
            else graphicsDevice.IsFullScreen = false;
            if (gameState == GameState.MENU)
            {
                gameState = (GameState)menu.DrawMainMenu(gameSaveData == null ? false : true);
                if (gameState == GameState.LOADGAME)
                {
                    gameSaveData = SaveManager.LoadGame();

                    seed = gameSaveData.seed;
                    seededRandom = new Random(seed);
                    map = new Map(gfx);
                    enhancements = new Enhancements();
                    enhancements.letters = gameSaveData.letterScores;
                    enhancements.wordScore = gameSaveData.enhancements[0];
                    enhancements.damageResist = gameSaveData.enhancements[1];
                    enhancements.startingScore = gameSaveData.enhancements[2];
                    shop = new Shop(gfx, enhancements);
                    treasure = new Treasure(gfx, enhancements);
                    coins = gameSaveData.coins;
                    level = gameSaveData.level;
                    selectedRune = gameSaveData.rune;
                    difficulty = gameSaveData.difficulty;

                    //Use the random from the seed same amount of times like saved game so that it's on the same random.next
                    UserAction[] UserActions = SaveManager.LoadActions();
                    actions = new List<UserAction>(UserActions);
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
                                Console.WriteLine($"Neznámá metoda: {action}");
                                break;
                        }
                    }
                    map.NodeVisit(gameSaveData.visitedNodes);
                    visitedNodes = new List<int[]>();
                    visitedNodes = gameSaveData.visitedNodes;
                    isReplay = false;
                    selectedNode = map.GetNodeFromPos(gameSaveData.mapNode[0], gameSaveData.mapNode[1]);
                    lastSelectedNode = selectedNode;
                }
                if (gameState == GameState.NEWGAME)
                {
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
                    coins = difficulty >= 1 ? 15 : startCoins;
                    gameState = GameState.RUNES;
                    if (difficulty >= 3) enhancements.wordScore -= 1;
                    GlyphManager.RemoveAllGlyphs();
                    GlyphManager.Add(Glyph.NoGlyphsLeft);
                    visitedNodes = new List<int[]>();
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

                menu.DrawOptionsMenu();
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
            if (kBState.IsKeyDown(Keys.Escape))
            {
                gameState = GameState.MENU;
                if (!dead)
                {
                    SaveManager.SaveGame(seed, level, coins, lastSelectedNode, enhancements, difficulty, selectedRune, visitedNodes);
                    gameSaveData = SaveManager.LoadGame();
                    SaveManager.SaveActions(lastActions);
                }
                dead = false;
                return;
            }

            //Checking if the player died
            if (dead)
            {
                gfx.spriteBatch.DrawString(gfx.gameFont, "You are dead", new Vector2(100), ThemeColors.Text);
                if (SteamUserStats.GetStat("deaths", out int current))
                {
                    current += 1;
                    SteamUserStats.SetStat("deaths", current);
                    if (current >= 1)
                    {
                        SteamUserStats.SetAchievement("Anubis");
                        SaveManager.UnlockUnlock("anubis");
                    }
                    if (current >= 9)
                    {
                        SteamUserStats.SetAchievement("Cat");
                        SaveManager.UnlockUnlock("cat");
                    }
                    SteamUserStats.StoreStats();
                }
                SaveManager.RemoveGameData();
                GlyphManager.RemoveAllGlyphs();
                gameSaveData = null;
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
                        "Mistakes subtract your score.\n\n" +
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
                    }
                    else
                    {
                        writer.WriteText(neededText, ThemeColors.NotSelected, isHintText: true, rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset);
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
                                typeSound.Play(0.5f, pitch, 0f);

                                pitch += 0.01f;
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
            }
            else if (selectedNode.type == NodeType.TREASURE)
            {
                isFightFinished = treasure.DisplayTreasure(ref coins);
                TopBannerDisplay(true);
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
                    isFightFinished = shop.DisplayShop(ref coins);
                    TopBannerDisplay(true);
                }
            }
        }

        //After fight checks if the player beat the needed score, and handles the rewards
        private void FightFinished(KeyboardState state)
        {
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
                        if (coins >= 200)
                        {
                            SaveManager.UnlockUnlock("jera0");
                            SteamUserStats.SetAchievement("JERA_0");
                        }
                        if (coins >= 100)
                        {
                            SaveManager.UnlockUnlock("HUNDRED");
                            SteamUserStats.SetAchievement("HUNDRED");
                        }
                        if (GlyphManager.IsActive(Glyph.B)) enhancements.startingScore += 5;

                        if (GlyphManager.IsActive(Glyph.Woman))
                        {
                            if (!isReplay) actions.Add(new UserAction("randomLetter", ""));
                            enhancements.MultiplyLetterScore((char)(seededRandom.Next(0, 26) + 'a'), 2);
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
                        cards.Add(new Card((char)(seededRandom.Next(0, 26) + 'a'), mult, seededRandom.Next(valMin, valMax), 0));
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
                            string fightWon = "You win";
                            gfx.spriteBatch.DrawString(gfx.gameFont, fightWon, new Vector2(MainGame.screenWidth / 2 - gfx.gameFont.MeasureString(fightWon).X / 2, 70), ThemeColors.Text);
                            string achievmentName = (((Runes.Runes)selectedRune).ToString() + (difficulty + 1)).ToString().ToLower();
                            SaveManager.UnlockUnlock(achievmentName);
                            SteamUserStats.SetAchievement(achievmentName.ToUpper());
                            if (!mistake)
                            {
                                SaveManager.UnlockUnlock("STAR");
                                SteamUserStats.SetAchievement("STAR");
                            } 
                            if (state.IsKeyDown(Keys.Enter))
                            {
                                gameState = GameState.MENU;
                            }
                        }
                        else
                        {
                            Color cardColor;
                            for (int i = 0; i < cards.Count; i++)
                            {
                                cardColor = (i == afterFightSelect) ? ThemeColors.Selected : ThemeColors.Foreground;
                                gfx.spriteBatch.Draw(gfx.texture, new Rectangle(MainGame.screenWidth / 5 * (i + 1), 250, 160, 120), cardColor);
                                gfx.spriteBatch.DrawString(gfx.gameFont, cards[i].letter + (cards[i].mult ? "  *" : "  +") + cards[i].value, new Vector2(MainGame.screenWidth / 5 * (i + 1) + 10, 250 + 10), ThemeColors.Text);
                            }
                            SaveManager.UnlockUnlock("first_kill");
                            SteamUserStats.SetAchievement("FIRST_KILL");
                            SteamUserStats.StoreStats();
                            string fightWon = "Fight won";
                            gfx.spriteBatch.DrawString(gfx.menuFont, fightWon, new Vector2(MainGame.screenWidth / 2 - gfx.gameFont.MeasureString(fightWon).X / 2, 70), ThemeColors.Text);
                            string chooseReward = "Choose your reward";
                            gfx.spriteBatch.DrawString(gfx.menuFont, chooseReward, new Vector2(MainGame.screenWidth / 2 - gfx.gameFont.MeasureString(chooseReward).X / 2, 130), ThemeColors.Text);
                            if (state.IsKeyDown(Keys.Enter))
                            {
                                if (cards[afterFightSelect].mult)
                                {
                                    enhancements.MultiplyLetterScore(cards[afterFightSelect].letter, cards[afterFightSelect].value);
                                }
                                else
                                {
                                    enhancements.AddLetterScore(cards[afterFightSelect].letter, cards[afterFightSelect].value);
                                }
                                roomSelected = false;
                                canStartFight = false;
                                if (selectedNode.type == NodeType.BOSS)
                                {
                                    SaveManager.UnlockUnlock("naudhiz0");
                                    SteamUserStats.SetAchievement("NAUDHIZ0");
                                    map.GenerateNodes();
                                    selectedNode = map.GetFirstNode();
                                    level++;
                                }
                                if (selectedNode.type == NodeType.ELITE)
                                {
                                    SaveManager.UnlockUnlock("S");
                                    SteamUserStats.SetAchievement("S");
                                }

                                if (mUnlocked)
                                {
                                    SaveManager.UnlockUnlock("M");
                                    SteamUserStats.SetAchievement("M");
                                }

                                int letters = Writer.writtenText.Count;
                                if (SteamUserStats.GetStat("letters", out int lettersCount))
                                {
                                    lettersCount += letters;
                                    SteamUserStats.SetStat("letters", lettersCount);
                                    if (lettersCount >= 10)
                                    {
                                        SteamUserStats.SetAchievement("THOUSAND");
                                        SaveManager.UnlockUnlock("THOUSAND");
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
                                    wordsCount += 1;
                                    SteamUserStats.SetStat("letters", wordsCount);
                                    if (wordsCount >= 100)
                                    {
                                        SteamUserStats.SetAchievement("PAPYRUS");
                                        SaveManager.UnlockUnlock("PAPYRUS");
                                    }
                                    SteamUserStats.StoreStats();
                                }

                                if (SteamUserStats.GetStat("fights_won", out int fightsWon))
                                {
                                    fightsWon += 1;
                                    SteamUserStats.SetStat("fights_won", fightsWon);
                                    if (fightsWon >= 10)
                                    {
                                        SteamUserStats.SetAchievement("J");
                                        SaveManager.UnlockUnlock("J");
                                    }
                                    SteamUserStats.StoreStats();
                                }
                                Console.WriteLine();
                                GlyphManager.SetUnlockedGlyphs();
                            }
                        }

                    }
                }
            }
            else
            {
                roomSelected = false;
                canStartFight = false;
            }
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
                    "?-Random turns into a random room.\n\n" +
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
                    if (state.IsKeyUp(Keys.Tab))
                    {
                        MapNode newNode = map.NodeSelect(selectedNode);

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
                else
                {
                    map.DrawNodes();
                }
            }
        }

        private void Reset()
        {
            lastActions = new List<UserAction>(actions);
            enhancements.ResetLettersChange();
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
        }

        private string RandomTextGenerate(int length)
        {
            if (!isReplay) actions.Add(new UserAction("RandomTextGenerate", length.ToString()));
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
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

        private void TopBannerDisplay(bool map){
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(15,15,MainGame.screenWidth-30,40), ThemeColors.Foreground);
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(15,15,MainGame.screenWidth-30,40), ThemeColors.Foreground);
            Vector2 textOffset = new Vector2(30,20);
            gfx.spriteBatch.DrawString(gfx.gameFont, $"coins:{coins}", textOffset, ThemeColors.Text);
            if(!tabPressed) gfx.spriteBatch.DrawString(gfx.gameFont, "tab -> inventory", new Vector2(MainGame.screenWidth/2 - gfx.gameFont.MeasureString("tab -> inventory").X/2,textOffset.Y), ThemeColors.Text);
            if(map) gfx.spriteBatch.DrawString(gfx.gameFont, $"level:{level}/3", new Vector2(MainGame.screenWidth - gfx.gameFont.MeasureString($"level:{level}/3").X-textOffset.X,textOffset.Y), ThemeColors.Text);
        }

        private static bool IsFight(NodeType nodeType){
            if(nodeType == NodeType.FIGHT || nodeType == NodeType.ELITE || nodeType == NodeType.BOSS)
                return true;
            return false;
        }

        private void CalculateScore(Vector2 lastCharPos)
        {
            long letterScore = 0;
            for(int i = 0; i < Writer.writtenText.Count; i++){
                bool canAdd = true;
                for(int j = 0; j < Writer.diffIndexes.Count; j++){
                    if(i == j) canAdd = false;
                }
                if(canAdd && Writer.writtenText[i] != ' '){
                    letterScore += enhancements.letters[Writer.writtenText[i]-'a'];
                }
            }

            if(Writer.writtenText.Count != lastCharCount){
                if(GlyphManager.IsActive(Glyph.Thousand)){
                    charCounter++;
                    if(charCounter == 1000){
                        charCounter = 0;
                        extraScore += 1000;
                    }
                }
                lastScore = playerScore;
                letterTimer = timeInSeconds;
                lastCharCount = Writer.writtenText.Count;
            }
            if ((int)timeInSeconds == 60) mUnlocked = true;

            gfx.spriteBatch.DrawString(gfx.gameFont, $"Streak:{wordStreak}".ToString(), new Vector2(50,100), ThemeColors.Text);
            string rewardText = "Reward: " + fight.cashGain;
            gfx.spriteBatch.DrawString(gfx.gameFont, rewardText, new Vector2(MainGame.screenWidth-50-gfx.gameFont.MeasureString(rewardText).X,100), ThemeColors.Text); 

            int mistakeCount = Math.Max(Writer.diffIndexes.Count - (GlyphManager.IsActive(Glyph.EyeOfHorus)?2:0),0);
            if(GlyphManager.IsActive(Glyph.Star) && mistakeCount > 0) dead = true;

            string userWords = new string(Writer.writtenText.ToArray());
            int correctWords = 0;

            List<int> neededStarts = new List<int>();

            int start = 0;
            for (int i = 0; i <= neededText.Length; i++) {
                if (i == neededText.Length || neededText[i] == ' ') {
                    if (i > start) neededStarts.Add(start);
                    start = i;
                }
            }

            for (int i = 0; i < neededStarts.Count-1; i++) {
                int wordLength = neededStarts[i+1] - neededStarts[i] - 1;
                if (userWords.Length < neededStarts[i] + wordLength+1) break;

                string neededWord = neededText.Substring(neededStarts[i]+1, wordLength);
                string userWord = userWords.Substring(neededStarts[i]+1, wordLength);
                if (userWord == neededWord) correctWords++;
            }

            if(userWords.Length != 0 && userWords.Length != lastWordCount && neededStarts.Contains(userWords.Length)){
                bool under3Sec = timeInSeconds - timeSinceLastWord < 3;
                if(!under3Sec && GlyphManager.IsActive(Glyph.N)) wordStreak = 0;
                timeSinceLastWord = timeInSeconds;
                lastWordCount = userWords.Length;
                wordCounter++;
                if(correctWords > lastCorrectWord){
                    lastCorrectWord = correctWords;
                    wordStreak += GlyphManager.IsActive(Glyph.Scarab)?3:1;
                    extraScore += wordStreak * (GlyphManager.IsActive(Glyph.N) && under3Sec?2:0);
                }
                else if(correctWords == lastCorrectWord){
                    lastCorrectWord = correctWords;
                    wordStreak = 0;
                }
            }
            
            if(GlyphManager.IsActive(Glyph.Anubis) && wordCounter % 5 == 0){
                if(!anubisActive && wordCounter > 0) coins++;
                anubisActive = true;
            } else anubisActive = false;

            if(startedTyping){
                playerScore = extraScore + enhancements.startingScore + letterScore;
                long enemyDamage = (long)((GlyphManager.IsActive(Glyph.House) ? 0.25:1)*(int)timeInSeconds) * fight.speed - enhancements.damageResist;
                long mistakeDamage = (GlyphManager.IsActive(Glyph.Snake)?0:1)*(GlyphManager.IsActive(Glyph.R)?5:1)*(difficulty >= 2?5:1)*mistakeCount;
                currentScore = playerScore + correctWords * enhancements.wordScore - (enemyDamage<0?1*(int)timeInSeconds:enemyDamage) - mistakeDamage;
                if(Writer.writtenText.Count == 1) lastScore = 0;
                if(playerScore - lastScore != 0 && timeInSeconds - letterTimer < 0.25 && Writer.writtenText.Count > 0) gfx.spriteBatch.DrawString(gfx.gameFont, "+" + (playerScore - lastScore).ToString(), lastCharPos, ThemeColors.Correct);
            }
            else currentScore = enhancements.startingScore;
        }

        private void Inventory(KeyboardState state){
            tabPressed = true;
            int columns = 4, rows = 7;
            int columnSpacing = (int)(MainGame.screenWidth / 4.5);
            int leftOffset = 40;

            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (column * rows + row >= 26) break;
                    gfx.spriteBatch.DrawString(gfx.gameFont, (char)(column * rows + row + 'a') + ": " + enhancements.letters[column * rows + row], new Vector2(columnSpacing / 2 + column * columnSpacing - leftOffset, 70 + row * 40), ThemeColors.Text);
                    long change = enhancements.lettersChange[column * rows + row];
                    if (change != 0) gfx.spriteBatch.DrawString(gfx.gameFont, (change < 0 ? "" : "+") + change, new Vector2(columnSpacing + column * columnSpacing + 25 - leftOffset, 70 + row * 40), change < 0 ? ThemeColors.Wrong : ThemeColors.Correct);
                }
            }
            Glyph[] glyphs = GlyphManager.GetGlyphs();
            if(glyphs.Length > 1){
                if(state.IsKeyDown(Keys.Left) && inventoryMove && inventoryGlyphSelect > 1){
                    inventoryGlyphSelect--;
                    inventoryMove = false;
                }
                if(state.IsKeyDown(Keys.Right) && inventoryMove && inventoryGlyphSelect < GlyphManager.GetGlyphCount()-1){
                    inventoryGlyphSelect++;
                    inventoryMove = false;
                }
                if(state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right)) inventoryMove = true;

                int borderOffset = 5, imageSize = 64, yOffset = 370, descOffset = 80, xOffset = 80;
                gfx.spriteBatch.Draw(gfx.texture, new Rectangle(xOffset*inventoryGlyphSelect-borderOffset, yOffset-borderOffset, imageSize+borderOffset*2, imageSize+borderOffset*2), ThemeColors.Selected);
                gfx.spriteBatch.DrawString(gfx.smallTextFont,GlyphManager.GetDescription(glyphs[inventoryGlyphSelect]), new Vector2(xOffset,yOffset+descOffset), ThemeColors.Text);
                columns = 0;
                foreach(Glyph glyph in glyphs){
                    if(glyph != Glyph.NoGlyphsLeft)
                        gfx.spriteBatch.Draw(GlyphManager.GetGlyphImage(glyph), new Rectangle(xOffset*columns,yOffset,imageSize,imageSize), ThemeColors.Background);
                    columns++;
                }
            }
        }

        private void HealthBar(){
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(40, 60, MainGame.screenWidth-80, 30), ThemeColors.Selected);
            int redBarLength = (int)((double)Math.Min(fight.scoreNeeded, fight.scoreNeeded - currentScore) / fight.scoreNeeded * (MainGame.screenWidth-90));
            gfx.spriteBatch.Draw(gfx.texture, new Rectangle(45, 65, redBarLength, 20), ThemeColors.Foreground);
            string score = $"{currentScore}/{fight.scoreNeeded}  -{fight.speed}/s";
            gfx.spriteBatch.DrawString(gfx.gameFont, score, new Vector2(MainGame.screenWidth/2-score.Length*10,100), ThemeColors.Text);
        }

        private void CharacterChoose(){
            KeyboardState state = Keyboard.GetState();
            int maxRunes = Enum.GetValues(typeof(Runes.Runes)).Length;
            if (state.IsKeyDown(Keys.Escape))
            {
                gameState = GameState.MENU;
            }
            if(canStartFight && state.IsKeyDown(Keys.Enter)){
                if(SaveManager.IsUnlockUnlocked((((Runes.Runes)selectedRune).ToString()+difficulty).ToString().ToLower())){
                    NewGameChoiceUpdate();
                    gameState = GameState.LOADGAME;
                }
            } else if(state.IsKeyUp(Keys.Enter)){
                canStartFight = true;
            }
            
            int rectWidth = MainGame.screenWidth/3, rectHeight = MainGame.screenHeight/2;
            if(runeMove){
                if(state.IsKeyDown(Keys.Left)){
                    if(!diffMove && selectedRune != 0){
                        selectedRune--;
                        difficulty = 0;
                    } else if(diffMove && difficulty != 0){
                        difficulty--;
                    }
                    
                    runeMove = false;
                }
                if(state.IsKeyDown(Keys.Right)){
                    if(!diffMove && selectedRune != maxRunes-1){
                        selectedRune++;
                        difficulty = 0;
                    } else if(diffMove && difficulty != 5){
                        difficulty++;
                    }
                    runeMove = false;
                }
            }
            if(state.IsKeyDown(Keys.Down)) diffMove = true;
            else if(state.IsKeyDown(Keys.Up)) diffMove = false;

            if(!runeMove){
                if(state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right)) runeMove = true;
            }

            if(selectedRune != 0) {
                gfx.spriteBatch.Draw(gfx.texture, new Rectangle(MainGame.screenWidth/5-rectWidth/4, MainGame.screenHeight/3- rectHeight/4, rectWidth/2, rectHeight/2), ThemeColors.NotSelected);
                string runeName = ((Runes.Runes)selectedRune-1).ToString().Substring(0,1);
                gfx.spriteBatch.DrawString(gfx.menuFont, runeName, new Vector2(MainGame.screenWidth/4 - gfx.menuFont.MeasureString(runeName).X/2, MainGame.screenHeight/3 - gfx.menuFont.MeasureString(runeName).Y/2), ThemeColors.Text);
                gfx.spriteBatch.DrawString(gfx.menuFont, "<", new Vector2(MainGame.screenWidth/3-gfx.menuFont.MeasureString("<").X*2, MainGame.screenHeight/3.5f), ThemeColors.Text);
            }
            {
                gfx.spriteBatch.Draw(gfx.texture, new Rectangle(MainGame.screenWidth/2-rectWidth/2, MainGame.screenHeight/3- rectHeight/2, rectWidth, rectHeight), diffMove?ThemeColors.NotSelected:ThemeColors.Selected);
                string runeName = ((Runes.Runes)selectedRune).ToString();
                int topOffset = 10;
                gfx.spriteBatch.DrawString(gfx.menuFont, runeName, new Vector2(MainGame.screenWidth/2 - gfx.menuFont.MeasureString(runeName).X/2, MainGame.screenHeight/3 - gfx.menuFont.MeasureString(runeName).Y*3+topOffset*2), ThemeColors.Text);
                if(SaveManager.IsUnlockUnlocked((((Runes.Runes)selectedRune).ToString()+0).ToString().ToLower())){
                    var field = ((Runes.Runes)selectedRune).GetType().GetField(((Runes.Runes)selectedRune).ToString());
                    var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
                    
                    string[] descStrings = attribute.GetDescription().Split('\n');
                    int line = 0;
                    foreach(string desc in descStrings){
                        gfx.spriteBatch.DrawString(gfx.gameFont, desc, new Vector2(MainGame.screenWidth/2 - gfx.gameFont.MeasureString(desc).X/2, MainGame.screenHeight/3 - gfx.gameFont.MeasureString(runeName).Y*(2-line++)+topOffset), ThemeColors.Text);
                    }
                } else{
                    var field = ((Runes.Runes)selectedRune).GetType().GetField(((Runes.Runes)selectedRune).ToString());
                    var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
                    
                    string[] descStrings = attribute.GetPrompt().Split('\n');
                    int line = 0;
                    foreach(string desc in descStrings){
                        gfx.spriteBatch.DrawString(gfx.gameFont, desc, new Vector2(MainGame.screenWidth/2 - gfx.gameFont.MeasureString(desc).X/2, MainGame.screenHeight/3 - gfx.gameFont.MeasureString(runeName).Y*(2-line++)+topOffset), ThemeColors.Wrong);
                    }
                }
            }
            if(selectedRune != maxRunes-1){
                gfx.spriteBatch.Draw(gfx.texture, new Rectangle(MainGame.screenWidth - MainGame.screenWidth/5-rectWidth/4, MainGame.screenHeight/3- rectHeight/4, rectWidth/2, rectHeight/2), ThemeColors.NotSelected);
                string runeName = ((Runes.Runes)selectedRune+1).ToString().Substring(0,1);
                gfx.spriteBatch.DrawString(gfx.menuFont, runeName, new Vector2(MainGame.screenWidth - MainGame.screenWidth/4 - gfx.menuFont.MeasureString(runeName).X/2, MainGame.screenHeight/3 - gfx.menuFont.MeasureString(runeName).Y/2), ThemeColors.Text);
                gfx.spriteBatch.DrawString(gfx.menuFont, ">", new Vector2(MainGame.screenWidth - MainGame.screenWidth/3 + gfx.menuFont.MeasureString(">").X, MainGame.screenHeight/3.5f), ThemeColors.Text);
            } 

            gfx.spriteBatch.DrawString(gfx.menuFont, "Difficulty: " , new Vector2(MainGame.screenWidth/2.5f-gfx.menuFont.MeasureString("Difficulty: ").X, MainGame.screenHeight-100), ThemeColors.Text);

            string diffString = $"<{difficulty}>{DifficultyText(difficulty)}";
            if(difficulty != 0 && !SaveManager.IsUnlockUnlocked((((Runes.Runes)selectedRune).ToString()+difficulty).ToString().ToLower())){
                diffString = "?";
            }
            Vector2 diffStringSize = gfx.menuFont.MeasureString(diffString);
            int padding = 10;
            if(diffMove) gfx.spriteBatch.Draw(gfx.texture, new Rectangle((int)(MainGame.screenWidth/2.5f)-padding, MainGame.screenHeight-100-padding,(int)diffStringSize.X+padding*2,(int)diffStringSize.Y+padding), (diffString == "?")?ThemeColors.Wrong:ThemeColors.Selected);
            gfx.spriteBatch.DrawString(gfx.menuFont, diffString, new Vector2(MainGame.screenWidth/2.5f, MainGame.screenHeight-100), ThemeColors.Text);
        }

        private void NewGameChoiceUpdate(){
            switch(selectedRune){
                case (int)Runes.Runes.Uruz:
                    enhancements.AllLettersAddScore(1);
                    break;
                case (int)Runes.Runes.Halagaz:
                    enhancements.AllLettersAddScore(-4);
                    for(int i = 0; i < 13; i++){
                        int index = unseededRandom.Next(0,26);
                        while(enhancements.letters[index] != -3){
                            index = unseededRandom.Next(0,26);
                        }
                        enhancements.AddLetterScore((char)('a'+index),8);
                    }
                    break;
                case (int)Runes.Runes.Jera:
                    coins = 60;
                    break;
                case (int)Runes.Runes.Naudhiz:
                    enhancements.startingScore = 50;
                    break;
            }
        }

        private string DifficultyText(int difficulty){
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
    }
}
