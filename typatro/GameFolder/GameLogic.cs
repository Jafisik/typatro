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
        bool firstEnter = true, waitingForEnter, enterReleased, startedTyping, finished;
        Color textColor = Color.GreenYellow;
        readonly Menu menu;
        readonly SpriteBatch spriteBatch;
        readonly int wordsGenerated = 10;
        int coins = 0, level = 1;
        long currentScore = 0;
        Fight fight;
        Treasure treasure;
        Shop shop;
        SpriteFont bigFont, smallFont;
        Texture2D texture;
        double timeSinceLastDecrease = 0;
        Enhancements enhancements;

        public GameLogic(SpriteBatch spriteBatch, SpriteFont menuFont, SpriteFont gameFont, Texture2D texture, List<string> jsonStrings)
        {
            this.spriteBatch = spriteBatch;
            this.jsonStrings = jsonStrings;
            bigFont = menuFont;
            this.texture = texture;
            this.smallFont = gameFont;

            map = new Map(spriteBatch, menuFont, gameFont, texture);
            map.GenerateNodes();
            selectedNode = map.GetFirstNode();

            writer = new Writer(spriteBatch, gameFont, diffIndexes, writtenText);
            menu = new Menu(spriteBatch, menuFont, texture);

            
            treasure = new Treasure(spriteBatch, bigFont, gameFont, texture);

            neededText = RandomTextGenerate(wordsGenerated);
            enhancements = new Enhancements();
            shop = new Shop(spriteBatch,gameFont,texture, enhancements);
        }

        public void Update(GameTime gameTime){
            if (gameState == GameState.PLAY){
                writer.ReadKeyboardInput(gameTime);
                writer.UpdateDiffIndexes(neededText);
            }
            if (!startedTyping && writtenText.Count > 0){
                startedTyping = true;
                timeSinceLastDecrease = 0;
            }

            if (startedTyping) {
                timeSinceLastDecrease += gameTime.ElapsedGameTime.TotalSeconds;
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
                return;
            }

            if (waitingForEnter){
                if (!enterReleased && state.IsKeyUp(Keys.Enter)){
                    timeSinceLastDecrease = 0;
                    enterReleased = true;
                }

                if (enterReleased){
                    if(IsFight(selectedNode.type)){
                        writer.WriteText(neededText, Color.Gray, isHintText: true);
                        writer.UserInputText(writtenText.ToArray(), textColor);
                        TopBannerDisplay(false);
                        CalculateScore();
                        finished = true;

                        writer.WriteText("Correct: " + (writtenText.Count > 0 ?
                            ((1f - (diffIndexes.Count / (float)writtenText.Count)) * 100).ToString("0.00") + "%" : "0%"), Color.White, 7);
                    }
                    else if(selectedNode.type == NodeType.TREASURE){
                        treasure.DisplayTreasure();
                        TopBannerDisplay(true);
                        finished = true;
                    }
                    else if(selectedNode.type == NodeType.SHOP){
                        finished = shop.DisplayShop(ref coins);
                        TopBannerDisplay(true);
                    }
                
                    if (state.IsKeyDown(Keys.Enter) && finished){
                        if(IsFight(selectedNode.type) && currentScore >= fight.scoreNeeded) coins += fight.cashGain;
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
                        if(newNode.type == NodeType.RANDOM) newNode.type = Map.GenerateNodeTypeFromRandom();
                        switch(newNode.type){
                            case NodeType.FIGHT:
                                fight = new NormalFight(level, newNode.column);
                                neededText = RandomTextGenerate(fight.letters);
                                break;
                            case NodeType.ELITE:
                                fight = new EliteFight(level, newNode.column);
                                neededText = RandomTextGenerate(fight.letters);
                                break;
                            case NodeType.BOSS:
                                fight = new BossFight(level, newNode.column);
                                neededText = RandomTextGenerate(fight.letters);
                                break;
                            case NodeType.TREASURE:
                                treasure.NewGlyph();
                                break;
                            case NodeType.SHOP:
                                shop.NewShop();
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
                stringBuilder.Append(jsonStrings[rand.Next(0, jsonStrings.Count)] + " ");
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

            string userText = new string(writtenText.ToArray());
            string[] userWords = userText.Split(' ');
            string[] neededWords = neededText.Split(' ');

            int correctWords = 0;
            for (int i = 0; i < Math.Min(userWords.Length, neededWords.Length); i++){
                if (userWords[i] == neededWords[i])
                    correctWords++;
            }

            if(startedTyping) currentScore = correctWords * 2 + letterScore - (int)timeSinceLastDecrease * fight.speed - diffIndexes.Count;
            else currentScore = 0;
        }
    }
}
