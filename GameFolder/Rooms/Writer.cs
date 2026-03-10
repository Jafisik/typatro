using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using typatro.GameFolder.Services;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder
{
    class Writer
    {
        int maxCharsPerLine = 40;
        readonly int yOffset = 200, leftOffset = 100;
        public static List<char> writtenText = new List<char>();
        public static List<int> diffIndexes = new List<int>();
        List<int> endLineIndexes = new List<int>(); 
        Keys? lastKey = null;
        double keyPressTime = 0;
        readonly double repeatInterval = 0.5;
        int lastCheckedIndex = 0;


        //Compare written text to needed text and then put indexes of the wrong characters into diffIndexes
        public void UpdateDiffIndexes(string compareText){
            if (writtenText.Count < lastCheckedIndex) lastCheckedIndex = writtenText.Count;
            for (int i = lastCheckedIndex; i < Math.Min(writtenText.Count, compareText.Length); i++){
                if (writtenText[i] != compareText[i] || (GlyphManager.IsActive(Glyph.Man) && writtenText[i] == 'x')){
                    if (!diffIndexes.Contains(i)){
                        diffIndexes.Add(i);
                    }
                    else diffIndexes.Remove(i);
                }
            }
            diffIndexes.RemoveAll(index => index >= writtenText.Count);
            lastCheckedIndex = writtenText.Count;
        }

        public void ReadKeyboardInput(GameTime gameTime){
            KeyboardState currentState = Keyboard.GetState();
            Keys[] pressedKeys = currentState.GetPressedKeys();

            if (pressedKeys.Length != 0){
                Keys currentKey = pressedKeys[0];
                if (currentKey != lastKey){
                    keyPressTime = 0;

                    char currentChar = ConvertKeyToChar(currentKey);
                    if (currentChar != '~' && currentChar != '\0') writtenText.Add(currentChar);
                    else{
                        if (writtenText.Count > 0 && currentChar != '\0' && GlyphManager.IsActive(Glyph.R)) writtenText.RemoveAt(writtenText.Count - 1);
                    }

                    lastKey = currentKey;
                }
                else{
                    keyPressTime += gameTime.ElapsedGameTime.TotalSeconds;

                    if (keyPressTime >= repeatInterval){
                        char currentChar = ConvertKeyToChar(currentKey);
                        if (currentChar != '~' && currentChar != '\0') writtenText.Add(currentChar);
                        else{
                            if (writtenText.Count > 0 && currentChar != '\0' && GlyphManager.IsActive(Glyph.R)) writtenText.RemoveAt(writtenText.Count - 1);
                        }

                    }
                }
            }
            else{
                lastKey = null;
                keyPressTime = 0;
            }
        }

        private static char ConvertKeyToChar(Keys? key){
            if (key == null) return '\0';
            if (key >= Keys.A && key <= Keys.Z) return (char)('a' + key - Keys.A);
            if (key == Keys.Space) return ' ';
            if (key == Keys.Back) return '~';
            return '\0';
        }

        //Visualizes user input and highlights mistakes (prints user input and then prints wrongString,
        // which has ' ' for correct letters and the actual letters for wrong letters)
        public Vector2 UserInputText(char[] printCharArray, int mistakeBlock, double rotation = 0, int xExtraOffset = 0, int yExtraOffset = 0){
            
            StringBuilder writeLine = new StringBuilder();
            StringBuilder wrongString = new StringBuilder();
            StringBuilder blockedString = new StringBuilder();
            int indexLine = 0, charIndex = 0;
            for (int i = 0; i < writtenText.Count; i++){
                charIndex++;
                if (indexLine < endLineIndexes.Count && i == endLineIndexes[indexLine] - indexLine){
                    writeLine.Append('\n');
                    wrongString.Append('\n');
                    blockedString.Append('\n');
                    indexLine++;
                    charIndex = 1;
                }

                if (diffIndexes.Contains(i))
                {
                    if (mistakeBlock == 0)
                    {
                        wrongString.Append(writtenText[i]);
                        blockedString.Append(' ');
                    }
                    else
                    {
                        mistakeBlock--;
                        wrongString.Append(' ');
                        blockedString.Append(writtenText[i]);
                    }
                    writeLine.Append(' ');
                }
                else
                {
                    wrongString.Append(' ');
                    blockedString.Append(' ');
                    writeLine.Append(printCharArray[i]);
                }
                
            }

            string correctText = writeLine.ToString();
            string incorrectText = wrongString.ToString();
            string blockedText = blockedString.ToString();

            Vector2 position = new Vector2(leftOffset + xExtraOffset, yOffset + yExtraOffset);

            Vector2 charSize = MainGame.Gfx.textFont.MeasureString(" ");
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(charIndex*(int)charSize.X + (int)position.X, (indexLine+1)*(int)charSize.Y + (int)position.Y-1,(int)charSize.X-3,3), ThemeColors.Selected);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, correctText, position + rotationPoint, ThemeColors.Text, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, incorrectText, position + rotationPoint, ThemeColors.Wrong, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, blockedText, position + rotationPoint, ThemeColors.Blocked, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
            return new Vector2(charIndex * charSize.X - 50, -50) + position;
        }
        Vector2 rotationPoint;

        public void WriteText(string printString, Color color, List<int> shinyWords = null, List<int> stoneWords = null, List<int> bloomWords = null,
            int line = 0, bool isHintText = false, double rotation = 0, int xExtraOffset = 0, int yExtraOffset = 0, bool treasure = false)
        {
            int word = 0;
            switch (SaveManager.size)
            {
                case 0:
                    maxCharsPerLine = 35;
                    break;
                case 1:
                    maxCharsPerLine = 50;
                    break;
                case 2:
                    maxCharsPerLine = 60;
                    break;
                case 3:
                    maxCharsPerLine = 90;
                    break;

            }

            char[] printCharArray = printString.ToCharArray();
            StringBuilder writeLine = new StringBuilder(printString.Length);
            StringBuilder shinyWriteLine = new StringBuilder(printString.Length);
            StringBuilder stoneWriteLine = new StringBuilder(printString.Length);
            StringBuilder bloomWriteLine = new StringBuilder(printString.Length);
            int beginingOfWord = 0, currentLineLength = 0;
            if (isHintText) endLineIndexes.Clear();
            for (int i = 0; i < printCharArray.Length; i++)
            {
                if (printCharArray[i] == ' ' || i == printCharArray.Length - 1)
                {
                    int wordLength = i - beginingOfWord + 1;

                    if (currentLineLength + wordLength > maxCharsPerLine)
                    {
                        endLineIndexes.Add(writeLine.Length);
                        shinyWriteLine.Append('\n');
                        stoneWriteLine.Append('\n');
                        bloomWriteLine.Append('\n');
                        if (isHintText) writeLine.Append('\n');
                        currentLineLength = 0;
                    }
                    if(shinyWords != null && stoneWords != null && bloomWords != null)
                    {
                        if (shinyWords.Contains(word)) shinyWriteLine.Append(new string(printCharArray, beginingOfWord, wordLength));
                        else shinyWriteLine.Append(new string(' ', wordLength));
                        if (stoneWords.Contains(word)) stoneWriteLine.Append(new string(printCharArray, beginingOfWord, wordLength));
                        else stoneWriteLine.Append(new string(' ', wordLength));
                        if (bloomWords.Contains(word)) bloomWriteLine.Append(new string(printCharArray, beginingOfWord, wordLength));
                        else bloomWriteLine.Append(new string(' ', wordLength));
                    }
                    
                    word++;
                    writeLine.Append(new string(printCharArray, beginingOfWord, wordLength));
                    currentLineLength += wordLength;
                    beginingOfWord = i + 1;
                }
            }
            string finalText = writeLine.ToString();
            Vector2 position = new Vector2(leftOffset + xExtraOffset, yOffset + (line * 30) + yExtraOffset);
            Vector2 size = MainGame.Gfx.textFont.MeasureString(finalText);
            rotationPoint = size / 2f;

            if (GlyphManager.IsActive(Glyph.Sun))
            {
                switch (SaveManager.theme)
                {
                    case 0:
                        color = new Color(30, 30, 30);
                        break;
                    case 1:
                        color = new Color(230, 170, 230);
                        break;
                    case 2:
                        color = new Color(0x303030);
                        break;
                    case 3:
                        color = new Color(0x303030);
                        break;
                }
            }

            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, finalText, position + rotationPoint, color, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, shinyWriteLine.ToString(), position + rotationPoint, color, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, stoneWriteLine.ToString(), position + rotationPoint, Color.Gray, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, bloomWriteLine.ToString(), position + rotationPoint, Color.DarkGreen, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
        }
    }
}
