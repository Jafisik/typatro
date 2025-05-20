using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Steamworks;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.Runes;
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
        Writer writer;
        readonly List<char> writtenText = new List<char>();
        string neededText;
        readonly List<int> diffIndexes = new List<int>();
        readonly List<string> jsonStrings;
        Map map;
        MapNode selectedNode, lastSelectedNode;
        bool firstEnter = true, waitingForEnter, enterReleased, startedTyping, finished, dead = false, isDragging;
        readonly Menu menu;
        readonly SpriteBatch spriteBatch;
        readonly int wordsGenerated = 10, maxRunes;
        int level = 1, extraScore = 0, inventoryGlyphSelect = 1, afterFightSelect = 0;
        int charCounter = 0, lastCharCount = 0, wordCounter = 0, lastWordCount = 1, wordStreak = 0, lastCorrectWord = 0;
        int selectedRune = 0, difficulty = 0;
        double textRotation = 0, correctWordTimer = 0, letterTimer = 0;
        int xTextOffset = 0, yTextOffset = 0;
        bool eyeOfHorusActive = false, anubisActive = false, tabPressed = false, inventoryMove = true, afterFightScreen = false, afterFightMove = false, runeMove = false, diffMove = false, tutorial = false;
        long currentScore = 0, playerScore = 0, lastScore = 0, coins = 0, startCoins = 30, damagePrint = -1;
        public static int seed;
        Fight fight;
        Treasure treasure;
        Shop shop;
        SpriteFont bigFont, smallFont, smallTextFont;
        Texture2D texture, catPic;
        double timeInSeconds = 0, lastTime = -1, timeSinceLastWord = 0;
        Enhancements enhancements;
        public static Random seededRandom = new Random(), unseededRandom = new Random();
        Point windowPos, dragOffset;
        Vector2 catPos = Vector2.One;
        List<Card> cards = new List<Card>();
        List<int[]> visitedNodes = new List<int[]>();
        GameSaveData gameSaveData;
        public static List<UserAction> actions = new List<UserAction>();
        List<UserAction> lastActions = actions;
        public static bool isReplay = false;
        private Rectangle draggableZone = new Rectangle(0,0,MainGame.screenWidth, MainGame.screenHeight);

        public GameLogic(SpriteBatch spriteBatch, SpriteFont bigFont, SpriteFont smallFont, SpriteFont smallTextFont, SpriteFont textFont, Texture2D texture, List<string> jsonStrings, Point windowPos, Texture2D catPic)
        {
            this.spriteBatch = spriteBatch;
            this.jsonStrings = jsonStrings;
            this.bigFont = bigFont;
            this.texture = texture;
            this.smallFont = smallFont;
            this.windowPos = windowPos;
            this.catPic = catPic;
            this.smallTextFont = smallTextFont;
            maxRunes = Enum.GetValues(typeof(Runes.Runes)).Length;
            seed = seededRandom.Next();
            seededRandom = new Random(seed);

            menu = new Menu(spriteBatch, bigFont, texture);
            gameSaveData = SaveManager.LoadGame();

            map = new Map(spriteBatch, bigFont, smallTextFont, texture);

            writer = new Writer(spriteBatch, textFont, diffIndexes, writtenText);
            neededText = RandomTextGenerate(wordsGenerated);

            enhancements = new Enhancements();
            treasure = new Treasure(spriteBatch, bigFont, textFont, texture, enhancements);
            shop = new Shop(spriteBatch, smallTextFont, texture, enhancements);

            
        }

        public void Update(GameWindow window){
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed){
                if (draggableZone.Contains(mouseState.Position)){
                    if (!isDragging){
                        isDragging = true;
                        dragOffset = new Point(mouseState.X, mouseState.Y);
                    }
                }
            }

            if (mouseState.LeftButton == ButtonState.Released){
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
                if (!startedTyping && writtenText.Count > 0){
                    startedTyping = true;
                    timeInSeconds = 0;
                }
                

                if (startedTyping) {
                    if(!finished){
                        timeInSeconds += MainGame.time.ElapsedGameTime.TotalSeconds;
                    }
                    else{
                        letterTimer = 0;
                        correctWordTimer = 0;
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
                                yTextOffset = unseededRandom.Next(-100,101);
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

        public void Draw(GraphicsDeviceManager graphicsDevice){
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.GraphicsDevice.Clear(ThemeColors.Background);
            int lineWidth = 15;
            spriteBatch.Draw(texture, new Rectangle(0,0,MainGame.screenWidth,lineWidth), ThemeColors.Foreground);
            spriteBatch.Draw(texture, new Rectangle(0,0,lineWidth,MainGame.screenHeight), ThemeColors.Foreground);
            spriteBatch.Draw(texture, new Rectangle(0,MainGame.screenHeight-lineWidth,MainGame.screenWidth,lineWidth), ThemeColors.Foreground);
            spriteBatch.Draw(texture, new Rectangle(MainGame.screenWidth-lineWidth,0,lineWidth,MainGame.screenHeight), ThemeColors.Foreground);
            if(SaveManager.fullscreen == 1) graphicsDevice.IsFullScreen = true;
            else graphicsDevice.IsFullScreen = false;
            if (gameState == GameState.MENU){
                gameState = (GameState)menu.DrawMainMenu(gameSaveData==null?false:true);
                if(gameState == GameState.LOADGAME){
                    gameSaveData = SaveManager.LoadGame();

                    seed = gameSaveData.seed;
                    seededRandom = new Random(seed);
                    map = new Map(spriteBatch, bigFont, smallTextFont, texture);
                    enhancements = new Enhancements();
                    enhancements.letters = gameSaveData.letterScores;
                    enhancements.wordScore = gameSaveData.enhancements[0];
                    enhancements.damageResist = gameSaveData.enhancements[1];
                    enhancements.startingScore = gameSaveData.enhancements[2];
                    shop = new Shop(spriteBatch, smallTextFont, texture, enhancements);
                    treasure = new Treasure(spriteBatch, bigFont, smallTextFont, texture, enhancements);
                    coins = gameSaveData.coins;
                    level = gameSaveData.level;
                    selectedRune = gameSaveData.rune;
                    difficulty = gameSaveData.difficulty;
                    
                    UserAction[] UserActions = SaveManager.LoadActions();
                    actions = new List<UserAction>(UserActions);
                    isReplay = true;
                    foreach (var entry in UserActions){
                        string action = entry.action;
                        string data = entry.data;

                        switch (action){
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
                if(gameState == GameState.NEWGAME){
                    actions.Clear();
                    seed = unseededRandom.Next();
                    seededRandom = new Random(seed);
                    map = new Map(spriteBatch, bigFont, smallTextFont, texture);
                    map.GenerateNodes();
                    selectedNode = map.GetFirstNode();
                    lastSelectedNode = map.GetFirstNode();
                    enhancements = new Enhancements();
                    shop = new Shop(spriteBatch, smallTextFont, texture, enhancements);
                    treasure = new Treasure(spriteBatch, bigFont, smallTextFont, texture, enhancements);
                    coins = difficulty>=1?15:startCoins;
                    gameState = GameState.RUNES;
                    if(difficulty>=3) enhancements.wordScore -= 1;
                    GlyphManager.RemoveAllGlyphs();
                    visitedNodes = new List<int[]>();
                }
                firstEnter = true;
                waitingForEnter = false;
                enterReleased = false;
                SaveManager.UnlockUnlock("uruz0");
            } else if (gameState == GameState.RUNES){
                KeyboardState state = Keyboard.GetState();

                if(!SaveManager.IsUnlockUnlocked("characterTutorial")){
                    if(state.IsKeyUp(Keys.Enter)){
                        tutorial = true;
                    }
                    spriteBatch.Draw(texture, new Rectangle(15,15,MainGame.screenWidth-30,MainGame.screenHeight-30), Color.Black);
                    spriteBatch.DrawString(bigFont, "New game tutorial", new Vector2(70,50), ThemeColors.Text);
                    //spriteBatch.DrawString(smallFont, "With arrows, choose your rune,\nwhich gives you specific bonuses,\nand then choose your difficulty." +
                    //"\n\nNew runes unlock under\nspecific conditions.\n\nNew difficulties unlock after\nwinning a run with\na specific rune.", new Vector2(70,130), ThemeColors.Text);
                    writer.WriteText("Use the arrows to select a rune\nthat grants unique bonuses,\nthen choose your difficulty.\n\n" +
                        "New runes are unlocked by meeting\nspecific conditions.\n\n" +
                        "New difficulties become available\nonce you complete a run with\na specific rune.\n\n" +
                        "Press Enter to continue.", ThemeColors.Text, treasure:true, xExtraOffset:-30, yExtraOffset:-70);
                    if(tutorial && state.IsKeyDown(Keys.Enter)){
                        SaveManager.UnlockUnlock("characterTutorial");
                        tutorial = false;
                    }
                    
                } else{
                    CharacterChoose();
                }
                
                
            }
            else if (gameState == GameState.LOADGAME){
                Play();
            }
            else if (gameState == GameState.OPTIONS){
                if (Keyboard.GetState().IsKeyDown(Keys.Escape)){
                    gameState = GameState.MENU;
                    
                    SaveManager.SaveSettings(SaveManager.theme, SaveManager.volume, SaveManager.size, SaveManager.fullscreen);
                }

                ThemeColors.Apply(SaveManager.theme);
                    
                menu.DrawOptionsMenu();
            }
            else if (gameState == GameState.EXIT){
                SteamAPI.Shutdown();
                Environment.Exit(0);
            }
            spriteBatch.End();
        }

        public void Play(){
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape)){
                gameState = GameState.MENU;
                if(!dead){
                    SaveManager.SaveGame(seed, level, coins, lastSelectedNode, enhancements, difficulty, selectedRune, visitedNodes);
                    gameSaveData = SaveManager.LoadGame();
                    SaveManager.SaveActions(lastActions);
                }
                dead = false;
                return;
            }

            if(dead){
                spriteBatch.DrawString(bigFont, "You are dead", new Vector2(100), ThemeColors.Text);
                SaveManager.RemoveGameData();
                GlyphManager.RemoveAllGlyphs();
                gameSaveData = null;
                return;
            }

            if (waitingForEnter){
                if (!enterReleased && state.IsKeyUp(Keys.Enter)){
                    timeInSeconds = 0;
                    enterReleased = true;
                }

                if (enterReleased){
                    if(IsFight(selectedNode.type) && !afterFightScreen){
                        if(!SaveManager.IsUnlockUnlocked("fightTutorial")){
                            if(state.IsKeyUp(Keys.Enter)){
                                tutorial = true;
                            }
                            spriteBatch.Draw(texture, new Rectangle(15,15,MainGame.screenWidth-30,MainGame.screenHeight-30), Color.Black);
                            spriteBatch.DrawString(bigFont, "Fight tutorial", new Vector2(70,50), ThemeColors.Text);
                            writer.WriteText("In fights you type to get\nenemy health to 0.\n\n" +
                                "Each letter gives you score based on\nyour letter score upgrades.\n\n" +
                                "Correct words give you extra score.\n\n" +
                                "Each consecutive correct word\nadds to your word streak\nand gives you extra score.\n\n" +
                                "Mistakes subtract your score.\n\n" +
                                "Press Enter to continue.", ThemeColors.Text, treasure:true, xExtraOffset:-30, yExtraOffset:-70);
                            if(tutorial && state.IsKeyDown(Keys.Enter)){
                                SaveManager.UnlockUnlock("fightTutorial");
                                tutorial = false;
                            }
                            
                        } else{
                            if(state.IsKeyDown(Keys.Tab) && timeInSeconds == 0){
                                Inventory(state);
                            } else{
                                writer.WriteText(neededText, ThemeColors.NotSelected, isHintText: true, rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset);
                                Vector2 lastCharPos = writer.UserInputText(writtenText.ToArray(), rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset);
                                TopBannerDisplay(false);
                                CalculateScore(lastCharPos);
                                HealthBar();
                                
                                if(eyeOfHorusActive) spriteBatch.Draw(texture, new Rectangle(0,0,MainGame.screenWidth,MainGame.screenHeight), Color.Black);
                                if(!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.Cat)) spriteBatch.Draw(catPic, new Rectangle((int)catPos.X,(int)catPos.Y,80,60), ThemeColors.Background);
                                if(writtenText.Count == neededText.Length || currentScore >= fight.scoreNeeded) finished = true;
                            }
                        }
                    }
                    else if(state.IsKeyDown(Keys.Tab)){
                        Inventory(state);
                    }
                    else if(selectedNode.type == NodeType.TREASURE){
                        finished = treasure.DisplayTreasure(ref coins);
                        TopBannerDisplay(true);
                    }
                    else if(selectedNode.type == NodeType.SHOP){
                        if(!SaveManager.IsUnlockUnlocked("shopTutorial")){
                            if(state.IsKeyUp(Keys.Enter)){
                                tutorial = true;
                            }
                            spriteBatch.Draw(texture, new Rectangle(15,15,MainGame.screenWidth-30,MainGame.screenHeight-30), Color.Black);
                            spriteBatch.DrawString(bigFont, "Shop tutorial", new Vector2(70,50), ThemeColors.Text);
                            writer.WriteText("Navigate with arrow keys\nthrough the shop.\n\n" +
                                "Purchase upgrades by pressing enter.\n\n" +
                                "Check inventory by pressing tab.\n\n" +
                                "The prices are calculated based\non previous upgrades.\n\n" +
                                "Press Enter to continue.", ThemeColors.Text, treasure:true, xExtraOffset:-30, yExtraOffset:-70);
                            if(tutorial && state.IsKeyDown(Keys.Enter)){
                                SaveManager.UnlockUnlock("shopTutorial");
                                tutorial = false;
                            }
                            
                        } else{
                            finished = shop.DisplayShop(ref coins);
                            TopBannerDisplay(true);
                        }
                    }
                    
                
                    if (finished){
                        lastSelectedNode = selectedNode;
                        lastActions = actions;
                        if(IsFight(selectedNode.type)){
                            if(!afterFightScreen){
                                currentScore *= (long)((GlyphManager.IsActive(Glyph.Flower)?(1+0.1*GlyphManager.GetGlyphCount()):1) *
                                    (GlyphManager.IsActive(Glyph.Water)?2:1) * (GlyphManager.IsActive(Glyph.Heart)?(diffIndexes.Count>0?3:0.5):1));
                                if(currentScore >= fight.scoreNeeded){
                                    double cashMultiply = (GlyphManager.IsActive(Glyph.Woman) ? 0.8 : 1) * (GlyphManager.IsActive(Glyph.Man) ? 1.5 : 1);
                                    coins += (int)(fight.cashGain * cashMultiply);
                                    if(coins >= 100){
                                        SaveManager.UnlockUnlock("jera0");
                                        SteamUserStats.SetAchievement("JERA_0");
                                    }
                                    if(GlyphManager.IsActive(Glyph.B)) enhancements.startingScore += 5;
                                    
                                    if(GlyphManager.IsActive(Glyph.Woman)){
                                        if(!isReplay) actions.Add(new UserAction("randomLetter",""));
                                        enhancements.MultiplyLetterScore((char)(seededRandom.Next(0,26)+'a'),2);
                                    }
                                } 
                                else{
                                    if(GlyphManager.IsActive(Glyph.Osiris)){
                                        enhancements.AllLettersMultiplyScore(0.8);
                                    }
                                    else{
                                        dead = true;
                                    }
                                }
                                afterFightScreen = true;

                                int valMin = 1, valMax = 4;
                                bool mult = false;
                                if(selectedNode.type == NodeType.ELITE){
                                    valMin = 3;
                                    valMax = 6;
                                }
                                if(selectedNode.type == NodeType.BOSS){
                                    valMin = 2;
                                    mult = true;
                                }
                                for(int i = 0; i < 3; i++){
                                    if(!isReplay) actions.Add(new UserAction("randomLetter",""));
                                    if(!isReplay) actions.Add(new UserAction("randomLetter",""));
                                    cards.Add(new Card((char)(seededRandom.Next(0, 26)+'a'), mult, seededRandom.Next(valMin,valMax), 0));
                                }

                            }
                            else{
                                if(afterFightMove && state.IsKeyDown(Keys.Left) && afterFightSelect > 0){
                                    afterFightSelect--;
                                    afterFightMove = false;
                                }
                                if(afterFightMove && state.IsKeyDown(Keys.Right) && afterFightSelect < cards.Count-1){
                                    afterFightSelect++;
                                    afterFightMove = false;
                                }
                                if(state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right)){
                                    afterFightMove = true;
                                }
                                if(state.IsKeyUp(Keys.Tab)){
                                    if(selectedNode.type == NodeType.BOSS && level == 3){
                                        string fightWon = "You win";
                                        spriteBatch.DrawString(bigFont, fightWon, new Vector2(MainGame.screenWidth/2-bigFont.MeasureString(fightWon).X/2,70), ThemeColors.Text);
                                        string achievmentName = (((Runes.Runes)selectedRune).ToString()+(difficulty+1)).ToString().ToLower();
                                        SaveManager.UnlockUnlock(achievmentName);
                                        SteamUserStats.SetAchievement(achievmentName.ToUpper());
                                        if(state.IsKeyDown(Keys.Enter)){
                                            gameState = GameState.MENU;
                                        }
                                    } else{
                                        Color cardColor;
                                        for(int i = 0; i < cards.Count; i++){
                                            cardColor = (i == afterFightSelect)? ThemeColors.Selected:ThemeColors.Foreground;
                                            spriteBatch.Draw(texture, new Rectangle(MainGame.screenWidth/5*(i+1), 250, 160, 120), cardColor);
                                            spriteBatch.DrawString(smallFont,  cards[i].letter + (cards[i].mult?"  *":"  +") + cards[i].value, new Vector2(MainGame.screenWidth/5*(i+1)+10, 250+10), ThemeColors.Text);
                                        }
                                        SaveManager.UnlockUnlock("first_kill");
                                        string fightWon = "Fight won";
                                        spriteBatch.DrawString(bigFont, fightWon, new Vector2(MainGame.screenWidth/2-bigFont.MeasureString(fightWon).X/2,70), ThemeColors.Text);
                                        string chooseReward = "Choose your reward";
                                        spriteBatch.DrawString(bigFont, chooseReward, new Vector2(MainGame.screenWidth/2-bigFont.MeasureString(chooseReward).X/2,130), ThemeColors.Text);
                                        if(state.IsKeyDown(Keys.Enter)){
                                            if(cards[afterFightSelect].mult){
                                                enhancements.MultiplyLetterScore(cards[afterFightSelect].letter, cards[afterFightSelect].value);
                                            } else{
                                                enhancements.AddLetterScore(cards[afterFightSelect].letter, cards[afterFightSelect].value);
                                            }
                                            waitingForEnter = false;
                                            enterReleased = false;
                                            if(selectedNode.type == NodeType.BOSS){
                                                SaveManager.UnlockUnlock("naudhiz0");
                                                SteamUserStats.SetAchievement("NAUDHIZ0");
                                                map.GenerateNodes();
                                                selectedNode = map.GetFirstNode();
                                                level++;
                                            }
                                        }
                                    }
                                    
                                }
                            }
                        }
                        else{
                            waitingForEnter = false;
                            enterReleased = false;
                        }
                    }
                }
            }
            else{
                if(!SaveManager.IsUnlockUnlocked("mapTutorial")){
                    if(state.IsKeyUp(Keys.Enter)){
                        tutorial = true;
                    }
                    spriteBatch.Draw(texture, new Rectangle(15,15,MainGame.screenWidth-30,MainGame.screenHeight-30), Color.Black);
                    spriteBatch.DrawString(bigFont, "Map tutorial", new Vector2(70,50), ThemeColors.Text);
                    writer.WriteText("Press up or down to choose which room \nto go to on your current floor.\n\n" +
                        "F-Fight E-Elite B-Boss are fights.\nHere you type to defeat the enemies\nto get to the new floor.\n\n" +
                        "$-Shop lets you buy upgrades for coins.\n\n" +
                        "X-Treasure gives you a free glyph.\n\n" +
                        "?-Random turns into a random room.\n\n" +
                        "Press Enter to continue.", ThemeColors.Text, treasure:true, xExtraOffset:-30, yExtraOffset:-70);
                    if(tutorial && state.IsKeyDown(Keys.Enter)){
                        SaveManager.UnlockUnlock("mapTutorial");
                        tutorial = false;
                    }
                    
                } else{
                    if (!firstEnter || state.IsKeyUp(Keys.Enter)){
                        firstEnter = false;
                        if(state.IsKeyUp(Keys.Tab)){
                            MapNode newNode = map.NodeSelect(selectedNode);

                            if (newNode != selectedNode){
                                visitedNodes.Add(new int[] {selectedNode.column,selectedNode.row});
                                lastActions = new List<UserAction>(actions);
                                enhancements.ResetLettersChange();
                                finished = false;
                                afterFightScreen = false;
                                lastCharCount = 0;
                                lastWordCount = 1;
                                lastCorrectWord = 0;
                                extraScore = 0;
                                lastScore = 0;
                                wordStreak = 0;
                                inventoryGlyphSelect = 1;
                                afterFightSelect = 0;
                                cards.Clear();
                                if(GlyphManager.IsActive(Glyph.Cat)) catPos = new Vector2(unseededRandom.Next(100,MainGame.screenWidth-100),unseededRandom.Next(100,MainGame.screenHeight-100));
                                if(newNode.type == NodeType.RANDOM) newNode.type = map.GenerateNodeTypeFromRandom();
                                switch(newNode.type){
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
                                        if(GlyphManager.IsActive(Glyph.Life)){
                                            if(!isReplay) actions.Add(new UserAction("randomLetter",""));
                                            enhancements.AddLetterScore((char)(seededRandom.Next(0,26)+'a'),5);
                                        }
                                        break;
                                }
                                if(IsFight(newNode.type)) neededText = RandomTextGenerate(fight.words + (GlyphManager.IsActive(Glyph.Papyrus)?20:0) - (difficulty >= 5?5:0));
                                if(difficulty >= 4) fight.speed *= 2;
                                if(fight != null) damagePrint = (long)((GlyphManager.IsActive(Glyph.House) ? 0.25:1)* (fight.speed - enhancements.damageResist));
                                writtenText.Clear();
                                startedTyping = false;
                                lastSelectedNode = selectedNode;
                                selectedNode = newNode;
                                waitingForEnter = true;
                            }
                        }
                        
                    }
                    TopBannerDisplay(true);
                    if(state.IsKeyDown(Keys.Tab)){
                        Inventory(state);
                    }
                    else{
                        map.DrawNodes();
                    }
                }
            }
        }

        private string RandomTextGenerate(int length){
            if(!isReplay) actions.Add(new UserAction("RandomTextGenerate", length.ToString()));
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < length; i++){
                string word = jsonStrings[seededRandom.Next(0, jsonStrings.Count)];
                if(GlyphManager.IsActive(Glyph.Snake) && unseededRandom.Next(0,16) == 12){
                    char[] wordToChar = word.ToCharArray();
                    wordToChar[unseededRandom.Next(0,word.Length)] = (char)(unseededRandom.Next(0,26) + 'a');
                    word = new string(wordToChar);
                }
                stringBuilder.Append(word + " ");
            }
            return stringBuilder.ToString();
        }

        private void TopBannerDisplay(bool map){
            spriteBatch.Draw(texture, new Rectangle(15,15,MainGame.screenWidth-30,40), ThemeColors.Foreground);
            spriteBatch.Draw(texture, new Rectangle(15,15,MainGame.screenWidth-30,40), ThemeColors.Foreground);
            Vector2 textOffset = new Vector2(30,20);
            spriteBatch.DrawString(smallFont, $"coins:{coins}", textOffset, ThemeColors.Text);
            if(!tabPressed) spriteBatch.DrawString(smallFont, "tab -> inventory", new Vector2(MainGame.screenWidth/2 - smallFont.MeasureString("tab -> inventory").X/2,textOffset.Y), ThemeColors.Text);
            if(map) spriteBatch.DrawString(smallFont, $"level:{level}/3", new Vector2(MainGame.screenWidth - smallFont.MeasureString($"level:{level}/3").X-textOffset.X,textOffset.Y), ThemeColors.Text);
        }

        private static bool IsFight(NodeType nodeType){
            if(nodeType == NodeType.FIGHT || nodeType == NodeType.ELITE || nodeType == NodeType.BOSS)
                return true;
            return false;
        }

        private void CalculateScore(Vector2 lastCharPos)
        {
            long letterScore = 0;
            for(int i = 0; i < writtenText.Count; i++){
                bool canAdd = true;
                for(int j = 0; j < diffIndexes.Count; j++){
                    if(i == j) canAdd = false;
                }
                if(canAdd && writtenText[i] != ' '){
                    letterScore += enhancements.letters[writtenText[i]-'a'];
                }
            }

            if(writtenText.Count != lastCharCount){
                if(GlyphManager.IsActive(Glyph.Thousand)){
                    charCounter++;
                    if(charCounter == 1000){
                        charCounter = 0;
                        extraScore += 1000;
                    }
                }
                lastScore = playerScore;
                letterTimer = timeInSeconds;
                lastCharCount = writtenText.Count;
            }
            spriteBatch.DrawString(smallFont, $"Streak:{wordStreak}".ToString(), new Vector2(50,100), ThemeColors.Text);
            string rewardText = "Reward: " + fight.cashGain;
            spriteBatch.DrawString(smallFont, rewardText, new Vector2(MainGame.screenWidth-50-smallFont.MeasureString(rewardText).X,100), ThemeColors.Text); 

            int mistakeCount = Math.Max(diffIndexes.Count - (GlyphManager.IsActive(Glyph.EyeOfHorus)?2:0),0);
            if(GlyphManager.IsActive(Glyph.Star) && mistakeCount > 0) dead = true;

            string userWords = new string(writtenText.ToArray());
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
                    correctWordTimer = timeInSeconds;
                }
                else if(correctWords == lastCorrectWord){
                    lastCorrectWord = correctWords;
                    wordStreak = 0;
                }
            }
            //if(timeInSeconds - correctWordTimer < 1 && correctWordTimer != 0) spriteBatch.DrawString(smallFont, $"Correct word: +{enhancements.wordScore}".ToString(), new Vector2(50,150), ThemeColors.Correct);

            if(GlyphManager.IsActive(Glyph.Anubis) && wordCounter % 5 == 0){
                if(!anubisActive && wordCounter > 0) coins++;
                anubisActive = true;
            } else anubisActive = false;

            if(startedTyping){
                playerScore = extraScore + enhancements.startingScore + letterScore;
                long enemyDamage = (long)((GlyphManager.IsActive(Glyph.House) ? 0.25:1)*(int)timeInSeconds) * fight.speed - enhancements.damageResist;
                long mistakeDamage = (GlyphManager.IsActive(Glyph.Snake)?0:1)*(GlyphManager.IsActive(Glyph.R)?5:1)*(difficulty >= 2?5:1)*mistakeCount;
                currentScore = playerScore + correctWords * enhancements.wordScore - (enemyDamage<0?1*(int)timeInSeconds:enemyDamage) - mistakeDamage;
                if(writtenText.Count == 1) lastScore = 0;
                if(playerScore - lastScore != 0 && timeInSeconds - letterTimer < 0.25 && writtenText.Count > 0) spriteBatch.DrawString(bigFont, "+" + (playerScore - lastScore).ToString(), lastCharPos, ThemeColors.Correct);
            }
            else currentScore = enhancements.startingScore;
        }

        private void Inventory(KeyboardState state){
            tabPressed = true;
            int columns = 4, rows = 7;
            int columnSpacing = MainGame.screenWidth / 5;

            for(int column = 0; column < columns; column++){
                for(int row = 0; row < rows; row++){
                    if(column*rows+row >= 26) break;
                    spriteBatch.DrawString(smallFont, (char)(column*rows+row+'a') + ": " + enhancements.letters[column*rows+row], new Vector2(columnSpacing /2+ column*columnSpacing, 70+row*40), ThemeColors.Text);
                    long change = enhancements.lettersChange[column*rows+row];
                    if(change != 0) spriteBatch.DrawString(smallFont, (change<0?"":"+") + change, new Vector2(columnSpacing + column*columnSpacing+16, 70+row*40), change<0?ThemeColors.Wrong:ThemeColors.Correct);
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
                spriteBatch.Draw(texture, new Rectangle(xOffset*inventoryGlyphSelect-borderOffset, yOffset-borderOffset, imageSize+borderOffset*2, imageSize+borderOffset*2), ThemeColors.Selected);
                spriteBatch.DrawString(smallTextFont,GlyphManager.GetDescription(glyphs[inventoryGlyphSelect]), new Vector2(xOffset,yOffset+descOffset), ThemeColors.Text);
                columns = 0;
                foreach(Glyph glyph in glyphs){
                    if(glyph != Glyph.NoGlyphsLeft)
                        spriteBatch.Draw(GlyphManager.GetGlyphImage(glyph), new Rectangle(xOffset*columns,yOffset,imageSize,imageSize), ThemeColors.Background);
                    columns++;
                }
            }
        }

        private void HealthBar(){
            spriteBatch.Draw(texture, new Rectangle(40, 60, MainGame.screenWidth-80, 30), ThemeColors.Selected);
            int redBarLength = (int)((double)Math.Min(fight.scoreNeeded, fight.scoreNeeded - currentScore) / fight.scoreNeeded * (MainGame.screenWidth-90));
            spriteBatch.Draw(texture, new Rectangle(45, 65, redBarLength, 20), ThemeColors.Foreground);
            string score = $"{currentScore}/{fight.scoreNeeded}  -{fight.speed}/s";
            spriteBatch.DrawString(smallFont, score, new Vector2(MainGame.screenWidth/2-score.Length*10,100), ThemeColors.Text);
        }

        private void CharacterChoose(){
            KeyboardState state = Keyboard.GetState();
            if(state.IsKeyDown(Keys.Escape)){
                gameState = GameState.MENU;
            }
            if(enterReleased && state.IsKeyDown(Keys.Enter)){
                if(SaveManager.IsUnlockUnlocked((((Runes.Runes)selectedRune).ToString()+difficulty).ToString().ToLower())){
                    NewGameChoiceUpdate();
                    gameState = GameState.LOADGAME;
                }
            } else if(state.IsKeyUp(Keys.Enter)){
                enterReleased = true;
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
                spriteBatch.Draw(texture, new Rectangle(MainGame.screenWidth/5-rectWidth/4, MainGame.screenHeight/3- rectHeight/4, rectWidth/2, rectHeight/2), ThemeColors.NotSelected);
                string runeName = ((Runes.Runes)selectedRune-1).ToString().Substring(0,1);
                spriteBatch.DrawString(bigFont, runeName, new Vector2(MainGame.screenWidth/4 - bigFont.MeasureString(runeName).X/2, MainGame.screenHeight/3 - bigFont.MeasureString(runeName).Y/2), ThemeColors.Text);
                spriteBatch.DrawString(bigFont, "<", new Vector2(MainGame.screenWidth/3-bigFont.MeasureString("<").X*2, MainGame.screenHeight/3.5f), ThemeColors.Text);
            }
            {
                spriteBatch.Draw(texture, new Rectangle(MainGame.screenWidth/2-rectWidth/2, MainGame.screenHeight/3- rectHeight/2, rectWidth, rectHeight), diffMove?ThemeColors.NotSelected:ThemeColors.Selected);
                string runeName = ((Runes.Runes)selectedRune).ToString();
                int topOffset = 10;
                spriteBatch.DrawString(bigFont, runeName, new Vector2(MainGame.screenWidth/2 - bigFont.MeasureString(runeName).X/2, MainGame.screenHeight/3 - bigFont.MeasureString(runeName).Y*3+topOffset*2), ThemeColors.Text);
                if(SaveManager.IsUnlockUnlocked((((Runes.Runes)selectedRune).ToString()+0).ToString().ToLower())){
                    var field = ((Runes.Runes)selectedRune).GetType().GetField(((Runes.Runes)selectedRune).ToString());
                    var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
                    
                    string[] descStrings = attribute.GetDescription().Split('\n');
                    int line = 0;
                    foreach(string desc in descStrings){
                        spriteBatch.DrawString(smallFont, desc, new Vector2(MainGame.screenWidth/2 - smallFont.MeasureString(desc).X/2, MainGame.screenHeight/3 - smallFont.MeasureString(runeName).Y*(2-line++)+topOffset), ThemeColors.Text);
                    }
                } else{
                    var field = ((Runes.Runes)selectedRune).GetType().GetField(((Runes.Runes)selectedRune).ToString());
                    var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
                    
                    string[] descStrings = attribute.GetPrompt().Split('\n');
                    int line = 0;
                    foreach(string desc in descStrings){
                        spriteBatch.DrawString(smallFont, desc, new Vector2(MainGame.screenWidth/2 - smallFont.MeasureString(desc).X/2, MainGame.screenHeight/3 - smallFont.MeasureString(runeName).Y*(2-line++)+topOffset), ThemeColors.Wrong);
                    }
                }
            }
            if(selectedRune != maxRunes-1){
                spriteBatch.Draw(texture, new Rectangle(MainGame.screenWidth - MainGame.screenWidth/5-rectWidth/4, MainGame.screenHeight/3- rectHeight/4, rectWidth/2, rectHeight/2), ThemeColors.NotSelected);
                string runeName = ((Runes.Runes)selectedRune+1).ToString().Substring(0,1);
                spriteBatch.DrawString(bigFont, runeName, new Vector2(MainGame.screenWidth - MainGame.screenWidth/4 - bigFont.MeasureString(runeName).X/2, MainGame.screenHeight/3 - bigFont.MeasureString(runeName).Y/2), ThemeColors.Text);
                spriteBatch.DrawString(bigFont, ">", new Vector2(MainGame.screenWidth - MainGame.screenWidth/3 + bigFont.MeasureString(">").X, MainGame.screenHeight/3.5f), ThemeColors.Text);
            } 

            spriteBatch.DrawString(bigFont, "Difficulty: " , new Vector2(MainGame.screenWidth/2.5f-bigFont.MeasureString("Difficulty: ").X, MainGame.screenHeight-100), ThemeColors.Text);

            string diffString = $"<{difficulty}>{DifficultyText(difficulty)}";
            if(difficulty != 0 && !SaveManager.IsUnlockUnlocked((((Runes.Runes)selectedRune).ToString()+difficulty).ToString().ToLower())){
                diffString = "?";
            }
            Vector2 diffStringSize = bigFont.MeasureString(diffString);
            int padding = 10;
            if(diffMove) spriteBatch.Draw(texture, new Rectangle((int)(MainGame.screenWidth/2.5f)-padding, MainGame.screenHeight-100-padding,(int)diffStringSize.X+padding*2,(int)diffStringSize.Y+padding), (diffString == "?")?ThemeColors.Wrong:ThemeColors.Selected);
            spriteBatch.DrawString(bigFont, diffString, new Vector2(MainGame.screenWidth/2.5f, MainGame.screenHeight-100), ThemeColors.Text);
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
