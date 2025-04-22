using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.UI;
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
        Writer writer;
        readonly List<char> writtenText = new List<char>();
        string neededText;
        readonly List<int> diffIndexes = new List<int>();
        readonly List<string> jsonStrings;
        readonly Map map;
        MapNode selectedNode;
        bool firstEnter = true, waitingForEnter, enterReleased, startedTyping, finished, dead = false;
        readonly Menu menu;
        readonly SpriteBatch spriteBatch;
        readonly int wordsGenerated = 10;
        int level = 1, extraScore = 0, inventoryGlyphSelect = 1, afterFightSelect = 0;
        int charCounter = 0, lastCharCount = 0, wordCounter = 0, lastWordCount = 1, wordStreak = 0, lastCorrectWord = 0;
        double textRotation = 0, correctWordTimer = 0, letterTimer = 0;
        int xTextOffset = 0, yTextOffset = 0;
        bool eyeOfHorusActive = false, anubisActive = false, tabPressed = false, inventoryMove = true, afterFightScreen = false, afterFightMove = false;
        long currentScore = 0, playerScore = 0, lastScore = 0, coins = 0, startCoins = 500, damagePrint = -1;
        Rectangle topBanner = new Rectangle(0,0,1024,45);
        Vector2 topBannerTextOffset = new Vector2(10,10);
        Fight fight;
        Treasure treasure;
        Shop shop;
        SpriteFont bigFont, smallFont, smallTextFont;
        Texture2D texture, catPic;
        double timeInSeconds = 0, lastTime = -1, timeSinceLastWord = 0;
        Enhancements enhancements;
        Random random = new Random();
        Point windowPos;
        Vector2 catPos = Vector2.One;
        List<Card> cards = new List<Card>();

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

            menu = new Menu(spriteBatch, bigFont, texture);

            map = new Map(spriteBatch, bigFont, smallTextFont, texture);
            map.GenerateNodes();
            selectedNode = map.GetFirstNode();

            writer = new Writer(spriteBatch, textFont, diffIndexes, writtenText);
            neededText = RandomTextGenerate(wordsGenerated);

            enhancements = new Enhancements();
            treasure = new Treasure(spriteBatch, bigFont, smallFont, texture, enhancements);
            shop = new Shop(spriteBatch, smallTextFont, texture, enhancements);
        }

        public void Update(GameTime gameTime, GameWindow window){
            if (gameState == GameState.PLAY){
                if((int)timeInSeconds % 8 == 0 && timeInSeconds != 0){
                    if(!GlyphManager.IsActive(Glyph.House)){
                        writer.ReadKeyboardInput(gameTime);
                        writer.UpdateDiffIndexes(neededText);
                    }
                }
                else{
                    writer.ReadKeyboardInput(gameTime);
                    writer.UpdateDiffIndexes(neededText);
                }
                if (!startedTyping && writtenText.Count > 0){
                    startedTyping = true;
                    timeInSeconds = 0;
                }
                

                if (startedTyping) {
                    if(!finished){
                        timeInSeconds += gameTime.ElapsedGameTime.TotalSeconds;
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
                                xTextOffset = random.Next(-100,101);
                                yTextOffset = random.Next(-100,101);
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
                        if(keyboardState.GetPressedKeyCount() > 0){
                            window.Position = new Point(windowPos.X+random.Next(-5,6), windowPos.Y+random.Next(-5,6));
                        }
                        else window.Position = windowPos;
                    }
                }
            }
            
        }

        public void Draw(GraphicsDeviceManager graphicsDevice){
            spriteBatch.Begin();
            spriteBatch.GraphicsDevice.Clear(ThemeColors.Background);
            if (gameState == GameState.MENU){
                gameState = (GameState)menu.DrawMainMenu(graphicsDevice);
                firstEnter = true;
                waitingForEnter = false;
                coins = startCoins;
            }
            else if (gameState == GameState.PLAY){
                Play();
            }
            else if (gameState == GameState.OPTIONS){
                if (Keyboard.GetState().IsKeyDown(Keys.Escape)){
                    gameState = GameState.MENU;
                    
                    SettingsManager.Save(menu.selectedTheme.ToString(), menu.volume);
                }

                if(menu.selectedTheme == Menu.Themes.pink) ThemeColors.Apply("pink");
                else if(menu.selectedTheme == Menu.Themes.red) ThemeColors.Apply("red");
                else if(menu.selectedTheme == Menu.Themes.black) ThemeColors.Apply("black");
                    
                menu.DrawOptionsMenu(graphicsDevice);
            }
            else if (gameState == GameState.EXIT){
                Environment.Exit(0);
            }
            spriteBatch.End();
        }

        public void Play(){
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape)){
                map.GenerateNodes();
                selectedNode = map.GetFirstNode();
                gameState = GameState.MENU;
                dead = false;
                return;
            }

            if(dead){
                spriteBatch.DrawString(bigFont, "You are dead", new Vector2(100), ThemeColors.Text);
                return;
            }

            if (waitingForEnter){
                if (!enterReleased && state.IsKeyUp(Keys.Enter)){
                    timeInSeconds = 0;
                    enterReleased = true;
                }

                if (enterReleased){
                    if(IsFight(selectedNode.type) && !afterFightScreen){

                        writer.WriteText(neededText, ThemeColors.NotSelected, isHintText: true, rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset);
                        Vector2 lastCharPos = writer.UserInputText(writtenText.ToArray(), rotation: textRotation, xExtraOffset: xTextOffset, yExtraOffset: yTextOffset);
                        TopBannerDisplay(false);
                        CalculateScore(lastCharPos);
                        HealthBar();
                        
                        if(eyeOfHorusActive) spriteBatch.Draw(texture, new Rectangle(0,0,1200,600), Color.Black);
                        if(!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.Cat)) spriteBatch.Draw(catPic, new Rectangle((int)catPos.X,(int)catPos.Y,80,60), ThemeColors.Background);
                        if(writtenText.Count == neededText.Length || currentScore > fight.scoreNeeded) finished = true;
                    }
                    else if(state.IsKeyDown(Keys.Tab)){
                        Inventory(state);
                    }
                    else if(selectedNode.type == NodeType.TREASURE){
                        finished = treasure.DisplayTreasure(ref coins);
                        TopBannerDisplay(true);
                    }
                    else if(selectedNode.type == NodeType.SHOP){
                        finished = shop.DisplayShop(ref coins);
                        TopBannerDisplay(true);
                    }
                    
                
                    if (finished){
                        if(IsFight(selectedNode.type)){
                            if(!afterFightScreen){
                                currentScore = currentScore * (long)((GlyphManager.IsActive(Glyph.Flower)?(1+0.1*GlyphManager.GetGlyphCount()):1) *
                                    (GlyphManager.IsActive(Glyph.Water)?2:1) * (GlyphManager.IsActive(Glyph.Heart)?(diffIndexes.Count>0?3:0.5):1));
                                if(currentScore >= fight.scoreNeeded){
                                    double cashMultiply = (GlyphManager.IsActive(Glyph.Woman) ? 0.8 : 1) * (GlyphManager.IsActive(Glyph.Man) ? 1.5 : 1);
                                    coins += (int)(fight.cashGain * cashMultiply);
                                    if(GlyphManager.IsActive(Glyph.B)) enhancements.startingScore += 5;
                                    if(GlyphManager.IsActive(Glyph.Woman)) enhancements.MultiplyLetterScore((char)(random.Next(0,26)+'a'),2);
                                } 
                                else{
                                    if(GlyphManager.IsActive(Glyph.Osiris)){
                                        enhancements.AllLettersMultiplyScore(0.8);
                                    }
                                    else dead = true;
                                }
                                if(selectedNode.type == NodeType.BOSS){
                                    map.GenerateNodes();
                                    selectedNode = map.GetFirstNode();
                                    level++;
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
                                    cards.Add(new Card((char)(random.Next(0, 26)+'a'), mult, random.Next(valMin,valMax), 0));
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
                                Color cardColor;
                                for(int i = 0; i < cards.Count; i++){
                                    
                                    cardColor = (i == afterFightSelect)? ThemeColors.Selected:ThemeColors.Foreground;
                                    spriteBatch.Draw(texture, new Rectangle(100+300*i, 200, 160, 120), cardColor);
                                    spriteBatch.DrawString(smallFont,  cards[i].letter + "  +" + cards[i].value, new Vector2(100+300*i+10, 200+10), ThemeColors.Text);
                                }
                                spriteBatch.DrawString(bigFont, "Fight won " + coins, new Vector2(100), ThemeColors.Text);
                                if(state.IsKeyDown(Keys.Enter)){
                                    if(cards[afterFightSelect].mult){
                                        enhancements.MultiplyLetterScore(cards[afterFightSelect].letter, cards[afterFightSelect].value);
                                    } else{
                                        enhancements.AddLetterScore(cards[afterFightSelect].letter, cards[afterFightSelect].value);
                                    }
                                    waitingForEnter = false;
                                    enterReleased = false;
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
                if (!firstEnter || state.IsKeyUp(Keys.Enter)){
                    firstEnter = false;
                    if(state.IsKeyUp(Keys.Tab)){
                        MapNode newNode = map.NodeSelect(selectedNode);

                        if (newNode != selectedNode){
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
                            if(GlyphManager.IsActive(Glyph.Cat)) catPos = new Vector2(random.Next(200,300),random.Next(200,300));
                            if(GlyphManager.IsActive(Glyph.Sun)) ThemeColors.NotSelected = new Color(Color.Gray, 0.6f);
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
                            damagePrint = (long)((GlyphManager.IsActive(Glyph.House) ? 0.25:1)* (fight.speed - enhancements.damageResist));
                            writtenText.Clear();
                            startedTyping = false;
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

        private string RandomTextGenerate(int length){
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < length; i++){
                string word = jsonStrings[random.Next(0, jsonStrings.Count)];
                if(GlyphManager.IsActive(Glyph.Snake) && random.Next(0,16) == 12){
                    char[] wordToChar = word.ToCharArray();
                    wordToChar[random.Next(0,word.Length)] = (char)(random.Next(0,26) + 'a');
                    word = new string(wordToChar);
                }
                stringBuilder.Append(word + " ");
            }
            stringBuilder.Append(jsonStrings[random.Next(0, jsonStrings.Count)]);
            return stringBuilder.ToString();
        }

        private void TopBannerDisplay(bool map){
            spriteBatch.Draw(texture, topBanner, ThemeColors.Foreground);
            string mapBanner = tabPressed?$"coins:{coins}":$"coins:{coins}      tab -> inventory";
            if(map) spriteBatch.DrawString(smallFont, mapBanner, topBannerTextOffset, ThemeColors.Text);
            else spriteBatch.DrawString(smallFont, $"coins:{coins} {currentScore}/{fight.scoreNeeded} speed:-{damagePrint} reward:{fight.cashGain}", topBannerTextOffset, ThemeColors.Text);
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
                bool under3Sec = timeInSeconds - timeSinceLastWord < 3;
                if(!under3Sec && GlyphManager.IsActive(Glyph.N)) wordStreak = 0;
                timeSinceLastWord = timeInSeconds;
                lastWordCount = userWords.Length;
                wordCounter++;
                if(correctWords > lastCorrectWord){
                    lastCorrectWord = correctWords;
                    wordStreak += GlyphManager.IsActive(Glyph.Scarab)?3:1;
                    extraScore += wordStreak * (GlyphManager.IsActive(Glyph.N) && under3Sec?2:1);
                    correctWordTimer = timeInSeconds;
                }
                else if(correctWords == lastCorrectWord){
                    lastCorrectWord = correctWords;
                    wordStreak = 0;
                }
            }
            if(timeInSeconds - correctWordTimer < 1 && correctWordTimer != 0) spriteBatch.DrawString(smallFont, $"Correct word: +{enhancements.wordScore}".ToString(), new Vector2(50,150), ThemeColors.Correct);

            if(GlyphManager.IsActive(Glyph.Anubis) && wordCounter % 5 == 0){
                if(!anubisActive && wordCounter > 0) coins++;
                anubisActive = true;
            } else anubisActive = false;

            if(startedTyping){
                playerScore = extraScore + enhancements.startingScore + letterScore;
                long enemyDamage = (long)((GlyphManager.IsActive(Glyph.House) ? 0.25:1)*(int)timeInSeconds) * fight.speed - enhancements.damageResist;
                long mistakeDamage = (GlyphManager.IsActive(Glyph.Snake)?0:1)*(GlyphManager.IsActive(Glyph.R)?5:1)*mistakeCount;
                currentScore = playerScore + correctWords * enhancements.wordScore - (enemyDamage<0?1*(int)timeInSeconds:enemyDamage) - mistakeDamage;
                if(writtenText.Count == 1) lastScore = 0;
                if(playerScore - lastScore != 0 && timeInSeconds - letterTimer < 0.25 && writtenText.Count > 0) spriteBatch.DrawString(bigFont, "+" + (playerScore - lastScore).ToString(), lastCharPos, ThemeColors.Correct);
            }
            else currentScore = enhancements.startingScore;
        }

        private void Inventory(KeyboardState state){
            tabPressed = true;
            int columns = 4, rows = 7;

            for(int column = 0; column < columns; column++){
                for(int row = 0; row < rows; row++){
                    if(column*rows+row >= 26) break;
                    spriteBatch.DrawString(smallFont, (char)(column*rows+row+'a') + ": " + enhancements.letters[column*rows+row], new Vector2(100+column*220,70+row*40), ThemeColors.Text);
                    long change = enhancements.lettersChange[column*rows+row];
                    if(change != 0) spriteBatch.DrawString(smallFont, (change<0?"":"+") + change, new Vector2(210+column*220,70+row*40), change<0?ThemeColors.Wrong:ThemeColors.Correct);
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
                    spriteBatch.Draw(GlyphManager.GetGlyphImage(glyph), new Rectangle(xOffset*columns++,yOffset,imageSize,imageSize), ThemeColors.Background);
                }
            }
            
            
        }

        private void HealthBar(){
            spriteBatch.Draw(texture, new Rectangle(40, 60, 930, 30), ThemeColors.Text);
            int redBarLength = (int)((double)Math.Min(fight.scoreNeeded, fight.scoreNeeded - currentScore) / fight.scoreNeeded * 920);
            spriteBatch.Draw(texture, new Rectangle(45, 65, redBarLength, 20), ThemeColors.Extra);
        }
    }
}
