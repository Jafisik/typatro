using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.Rooms;

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
        bool firstEnter = true, waitingForEnter, enterReleased;
        Color textColor = Color.GreenYellow;
        readonly Menu menu;
        readonly SpriteBatch spriteBatch;
        readonly int wordsGenerated = 10;
        int coins = 0;
        Fight fight;
        Treasure treasure;
        Shop shop;
        SpriteFont bigFont;
        Texture2D texture;

        public GameLogic(SpriteBatch spriteBatch, SpriteFont menuFont, SpriteFont gameFont, Texture2D texture, List<string> jsonStrings)
        {
            this.spriteBatch = spriteBatch;
            this.jsonStrings = jsonStrings;
            bigFont = menuFont;
            this.texture = texture;

            map = new Map(spriteBatch, menuFont, gameFont, texture);
            map.GenerateNodes();
            selectedNode = map.GetFirstNode();

            writer = new Writer(spriteBatch, gameFont, diffIndexes, writtenText);
            menu = new Menu(spriteBatch, menuFont, texture);

            neededText = RandomTextGenerate(wordsGenerated);
        }

        public void Update(GameTime gameTime){
            if (gameState == GameState.PLAY){
                writer.ReadKeyboardInput(gameTime);
                writer.UpdateDiffIndexes(neededText);
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
            TopBannerDisplay();

            if (waitingForEnter){
                if (!enterReleased && state.IsKeyUp(Keys.Enter)){
                    enterReleased = true;
                }

                if (enterReleased){
                    if(selectedNode.type == NodeType.FIGHT || selectedNode.type == NodeType.ELITE || selectedNode.type == NodeType.BOSS){
                        writer.WriteText(neededText, Color.Gray, isHintText: true);
                        writer.UserInputText(writtenText.ToArray(), textColor);
                        writer.WriteText("Correct: " + (writtenText.Count > 0 ?
                            ((1f - (diffIndexes.Count / (float)writtenText.Count)) * 100).ToString("0.00") + "%" : "0%"), Color.White, 7);
                    }
                    else if(selectedNode.type == NodeType.TREASURE){
                        writer.WriteText("Treasure",Color.Black);
                    }
                    else if(selectedNode.type == NodeType.SHOP){
                        writer.WriteText("Shop",Color.Black);
                    }
                
                    

                    if (state.IsKeyDown(Keys.Enter))
                    {
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
                        switch(newNode.type){
                            case NodeType.FIGHT:
                                fight = new NormalFight(newNode.row, newNode.column);
                                neededText = RandomTextGenerate(fight.letters);
                                break;
                            case NodeType.ELITE:
                                fight = new EliteFight(newNode.row, newNode.column);
                                neededText = RandomTextGenerate(fight.letters);
                                break;
                            case NodeType.BOSS:
                                fight = new BossFight(newNode.row, newNode.column);
                                neededText = RandomTextGenerate(fight.letters);
                                break;
                            case NodeType.TREASURE:
                                //TODO
                                break;
                            case NodeType.SHOP:
                                //TODO
                                break;
                        }
                        writtenText.Clear();
                        selectedNode = newNode;
                        waitingForEnter = true;
                    }
                }

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

        private void TopBannerDisplay(){
            spriteBatch.Draw(texture, new Rectangle(0,0,1500,45),Color.Silver);
            spriteBatch.DrawString(bigFont, $"Coins:{coins}", new Vector2(10,10), Color.Black);
            
        }
    }
}
