using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder
{
    public class GameLogic
    {
        public enum GameState
        {
            PLAY,
            OPTIONS,
            EXIT,
            MENU
        }

        public GameState gameState = GameState.MENU;
        readonly Random rand = new Random();
        Writer writer;
        readonly List<char> writtenText = new List<char>();
        string neededText;
        readonly List<int> diffIndexes = new List<int>();
        readonly List<string> jsonStrings;
        readonly Map map;
        MapNode selectedNode;
        bool firstEnter = true, waitingForEnter, enterReleased, startedTyping, finished, dead = false;
        Color textColor = Color.GreenYellow;
        readonly Menu menu;
        readonly SpriteBatch spriteBatch;
        readonly int wordsGenerated = 10;
        int coins = 0, level = 1, startingScore = 0, extraScore = 0;
        int charCounter = 0, lastCharCount = 0, wordCounter = 0, lastWordCount = 1, wordStreak = 0, lastCorrectWord = 0;
        double textRotation = 0;
        int xTextOffset = 0, yTextOffset = 0;
        bool eyeOfHorusActive = false, anubisActive = false;
        long currentScore = 0;
        Fight fight;
        Treasure treasure;
        Shop shop;
        SpriteFont bigFont, smallFont;
        Texture2D texture;
        double timeInSeconds = 0, lastTime = -1;
        Enhancements enhancements;
        Random random = new Random();
        Point windowPos;

        public GameLogic(SpriteBatch spriteBatch, SpriteFont menuFont, SpriteFont gameFont, Texture2D texture, List<string> jsonStrings, Point windowPos)
        {
            this.spriteBatch = spriteBatch;
            this.jsonStrings = jsonStrings;
            bigFont = menuFont;
            this.texture = texture;
            this.smallFont = gameFont;
            this.windowPos = windowPos;


            map = new Map(spriteBatch, menuFont, gameFont, texture);
            map.GenerateNodes();
            selectedNode = map.GetFirstNode();

            writer = new Writer(spriteBatch, gameFont, diffIndexes, writtenText);
            menu = new Menu(spriteBatch, menuFont, texture);

            neededText = RandomTextGenerate(wordsGenerated);
            enhancements = new Enhancements();
            treasure = new Treasure(spriteBatch, bigFont, gameFont, texture, enhancements);
            shop = new Shop(spriteBatch,gameFont,texture, enhancements);
        }

        public void Update(GameTime gameTime, GameWindow window){
            if (gameState == GameState.PLAY){
                writer.ReadKeyboardInput(gameTime);
                writer.UpdateDiffIndexes(neededText);
            }
            if (!startedTyping && writtenText.Count > 0){
                startedTyping = true;
                timeInSeconds = 0;
            }
            

            if (startedTyping) {
                timeInSeconds += gameTime.ElapsedGameTime.TotalSeconds;
                if((int)timeInSeconds != lastTime){
                    if((int)timeInSeconds % 10 == 0){
                        if(GlyphManager.IsActive(Glyph.H)) textRotation += Math.PI;
                        if(GlyphManager.IsActive(Glyph.F)) coins += 10;
                    }
                    if((int)timeInSeconds % 5 == 0){
                        if(GlyphManager.IsActive(Glyph.EyeOfHorus)) eyeOfHorusActive = true;
                        if(GlyphManager.IsActive(Glyph.F)){
                            xTextOffset = random.Next(-100,101);
                            yTextOffset = random.Next(-100,101);
                        }
                    } else eyeOfHorusActive = false;
                }
                lastTime = (int)timeInSeconds;
                KeyboardState keyboardState = Keyboard.GetState();
                if(GlyphManager.IsActive(Glyph.K)){
                    if(keyboardState.GetPressedKeyCount() > 0){
                        window.Position = new Point(windowPos.X+random.Next(-5,6), windowPos.Y+random.Next(-5,6));
                    }
                    else window.Position = windowPos;
                }
            }
        }

        public void Draw(GraphicsDeviceManager graphicsDevice){
            spriteBatch.Begin();
            if (gameState == GameState.MENU){
                gameState = (GameState)menu.DrawMainMenu(graphicsDevice);
                firstEnter = true;
            }
            else if (gameState == GameState.PLAY){
                Play();
            }
            else if (gameState == GameState.OPTIONS){
                if (Keyboard.GetState().IsKeyDown(Keys.Escape)) 
                    gameState = GameState.MENU;
                
                menu.DrawOptionsMenu(graphicsDevice);
            }
            else if (gameState == GameState.EXIT){
                Environment.Exit(0);
            }
            spriteBatch.End();
        }

        private void Play(){
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape)){
                map.GenerateNodes();
                selectedNode = map.GetFirstNode();
                gameState = GameState.MENU;
                dead = false;
                return;
            }

            if(dead){
                spriteBatch.DrawString(bigFont, "You are dead", new Vector2(100), Color.Black);
                return;
            }

            if (waitingForEnter){
                if (!enterReleased && state.IsKeyUp(Keys.Enter)){
                    timeInSeconds = 0;
                    enterReleased = true;
                }

                if (enterReleased){
                    if(IsFight(selectedNode.type)){

                        writer.WriteText(neededText, Color.Gray, isHintText: true, rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset);
                        writer.UserInputText(writtenText.ToArray(), textColor, rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset);
                        TopBannerDisplay(false);
                        CalculateScore();
                        finished = true;

                        writer.WriteText("Correct: " + (writtenText.Count > 0 ?
                            ((1f - (diffIndexes.Count / (float)writtenText.Count)) * 100).ToString("0.00") + "%" : "0%"), Color.White, 7);

                        if(eyeOfHorusActive) spriteBatch.Draw(texture, new Rectangle(0,0,1200,600), Color.Black);
                    }
                    else if(selectedNode.type == NodeType.TREASURE){
                        treasure.DisplayTreasure(ref coins);
                        TopBannerDisplay(true);
                        finished = true;
                    }
                    else if(selectedNode.type == NodeType.SHOP){
                        finished = shop.DisplayShop(ref coins);
                        TopBannerDisplay(true);
                    }
                
                    if (state.IsKeyDown(Keys.Enter) && finished){
                        if(IsFight(selectedNode.type)){
                            currentScore = currentScore * (long)((GlyphManager.IsActive(Glyph.Existence)?(1+0.1*GlyphManager.glyphCount):1) *
                                (GlyphManager.IsActive(Glyph.Water)?2:1) * (GlyphManager.IsActive(Glyph.Heart)?(diffIndexes.Count>0?3:0.5):1));
                            if(currentScore >= fight.scoreNeeded){
                                double cashMultiply = (GlyphManager.IsActive(Glyph.M) ? 0.8 : 1) * (GlyphManager.IsActive(Glyph.Man) ? 1.5 : 1);
                                coins += (int)(fight.cashGain * cashMultiply);
                                if(GlyphManager.IsActive(Glyph.B)) startingScore += 5;
                                if(GlyphManager.IsActive(Glyph.M)) enhancements.MultiplyLetterScore((char)(random.Next(0,26)+'a'),2);
                            } 
                            else{
                                if(GlyphManager.IsActive(Glyph.Osiris)){
                                    enhancements.AllLettersMultiplyScore(0.8);
                                }
                                else dead = false;
                            }
                        }
                        if(selectedNode.type == NodeType.BOSS){
                            map.GenerateNodes();
                            selectedNode = map.GetFirstNode();
                            level++;
                        }
                        waitingForEnter = false;
                        enterReleased = false;
                    }
                }
            }
            else{
                if (!firstEnter || state.IsKeyUp(Keys.Enter)){
                    firstEnter = false;
                    MapNode newNode = map.NodeSelect(selectedNode);

                    if (newNode != selectedNode){
                        finished = false;
                        lastCharCount = 0;
                        lastWordCount = 1;
                        if(newNode.type == NodeType.RANDOM) newNode.type = Map.GenerateNodeTypeFromRandom();
                        switch(newNode.type){
                            case NodeType.FIGHT:
                                fight = new NormalFight(level, newNode.column);
                                neededText = RandomTextGenerate(fight.words + (GlyphManager.IsActive(Glyph.Papyrus)?20:0));
                                break;
                            case NodeType.ELITE:
                                fight = new EliteFight(level, newNode.column);
                                neededText = RandomTextGenerate(fight.words + (GlyphManager.IsActive(Glyph.Papyrus)?20:0));
                                break;
                            case NodeType.BOSS:
                                fight = new BossFight(level, newNode.column);
                                neededText = RandomTextGenerate(fight.words + (GlyphManager.IsActive(Glyph.Papyrus)?20:0));
                                break;
                            case NodeType.TREASURE:
                                treasure.NewGlyph();
                                break;
                            case NodeType.SHOP:
                                shop.NewShop();
                                if(GlyphManager.IsActive(Glyph.Life)){
                                    enhancements.AddLetterScore((char)(random.Next(0,26)+'a'),5);
                                }
                                break;
                        }
                        writtenText.Clear();
                        startedTyping = false;
                        selectedNode = newNode;
                        waitingForEnter = true;
                    }
                }
                TopBannerDisplay(true);
                map.DrawNodes();
            }
        }

        private string RandomTextGenerate(int length){
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < length; i++){
                string word = jsonStrings[rand.Next(0, jsonStrings.Count)];
                if(GlyphManager.IsActive(Glyph.Snake) && random.Next(0,16) == 12){
                    char[] wordToChar = word.ToCharArray();
                    wordToChar[random.Next(0,word.Length)] = (char)(random.Next(0,26) + 'a');
                    word = new string(wordToChar);
                }
                stringBuilder.Append(word + " ");
            }
            stringBuilder.Append(jsonStrings[rand.Next(0, jsonStrings.Count)]);
            return stringBuilder.ToString();
        }

        private void TopBannerDisplay(bool map){
            spriteBatch.Draw(texture, new Rectangle(0,0,1500,45),Color.Silver);
            if(map) spriteBatch.DrawString(bigFont, $"coins:{coins}", new Vector2(10,10), Color.Black);
            else spriteBatch.DrawString(bigFont, $"coins:{coins} {currentScore}/{fight.scoreNeeded} speed:-{fight.speed} reward:{fight.cashGain}", new Vector2(10,10), Color.Black);
        }

        private static bool IsFight(NodeType nodeType){
            if(nodeType == NodeType.FIGHT || nodeType == NodeType.ELITE || nodeType == NodeType.BOSS)
                return true;
            return false;
        }

        private void CalculateScore()
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

            if(GlyphManager.IsActive(Glyph.Thousand) && writtenText.Count != lastCharCount){
                charCounter++;
                if(charCounter == 1000){
                    charCounter = 0;
                    extraScore += 1000;
                }
                lastCharCount = writtenText.Count;
            }


            int mistakeCount = Math.Max(diffIndexes.Count - (GlyphManager.IsActive(Glyph.EyeOfHorus)?2:0),0);
            if(GlyphManager.IsActive(Glyph.Star) && mistakeCount > 0) dead = true;

            string userText = new string(writtenText.ToArray());
            string[] userWords = userText.Split(' ');
            string[] neededWords = neededText.Split(' ');

            int correctWords = 0;
            for (int i = 0; i < Math.Min(userWords.Length, neededWords.Length); i++){
                if (userWords[i] == neededWords[i])
                    correctWords++;
            }

            if(userWords.Length != lastWordCount){
                lastWordCount = userWords.Length;
                wordCounter++;
                if(correctWords > lastCorrectWord){
                    lastCorrectWord = correctWords;
                    wordStreak += GlyphManager.IsActive(Glyph.Scarab)?3:1;
                    extraScore += wordStreak;
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

            if(startedTyping) currentScore = extraScore + startingScore + correctWords * 2 + letterScore - 
                (long)((GlyphManager.IsActive(Glyph.House) ? 0.25:1)*(int)timeInSeconds) * fight.speed - 
                (GlyphManager.IsActive(Glyph.Snake)?0:1)*(GlyphManager.IsActive(Glyph.R)?5:1)*mistakeCount;
            else currentScore = startingScore;
        }
    }
}
