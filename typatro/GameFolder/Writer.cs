using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace typatro.GameFolder
{
    class Writer
    {
        private int maxCharsPerLine = 40;
        private int yOffset = 80;
        private SpriteBatch _spriteBatch;
        private SpriteFont font;
        private List<char> writtenText;
        private List<int> diffIndexes;
        private List<int> endLineIndexes = new List<int>(); 
        private Keys? lastKey = null;
        private double keyPressTime = 0;
        private double repeatInterval = 0.5;
        private int lastCheckedIndex = 0;


        public Writer(SpriteBatch _spriteBatch, SpriteFont font, List<int> diffIndexes, List<char> writtenText)
        {
            this._spriteBatch = _spriteBatch;
            this.font = font;
            this.writtenText = writtenText;
            this.diffIndexes = diffIndexes;
        }

        //Compare written text to needed text and then put indexes of the wrong characters into diffIndexes
        public void UpdateDiffIndexes(string compareText){
            if (writtenText.Count < lastCheckedIndex) lastCheckedIndex = writtenText.Count;
            for (int i = lastCheckedIndex; i < Math.Min(writtenText.Count, compareText.Length); i++){
                if (writtenText[i] != compareText [i]){
                    if (!diffIndexes.Contains(i)){
                        diffIndexes.Add(i);
                    }
                    else diffIndexes.Remove(i);
                }
            }
            diffIndexes.RemoveAll(index => index >= writtenText.Count);
            lastCheckedIndex = writtenText.Count;
        }

        //Build a string of ' ' and wrong characters which is devided by \n into lines 
        // and then draw it over written text
        public void WriteMistakes(List<int> endLineIndexes){
            StringBuilder wrongString = new StringBuilder();
            int indexLine = 0;

            for (int i = 0; i < writtenText.Count; i++){
                if (indexLine < endLineIndexes.Count && i == endLineIndexes[indexLine] - indexLine){
                    wrongString.Append('\n');
                    indexLine++;
                }
                wrongString.Append(diffIndexes.Contains(i) ? writtenText[i] : ' ');
            }
            _spriteBatch.DrawString(font, wrongString.ToString(), new Vector2(80, yOffset), Color.Red);
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
                        if (writtenText.Count > 0 && currentChar != '\0') writtenText.RemoveAt(writtenText.Count - 1);
                    }

                    lastKey = currentKey;
                }
                else{
                    keyPressTime += gameTime.ElapsedGameTime.TotalSeconds;

                    if (keyPressTime >= repeatInterval){
                        char currentChar = ConvertKeyToChar(currentKey);
                        if (currentChar != '~' && currentChar != '\0') writtenText.Add(currentChar);
                        else{
                            if (writtenText.Count > 0 && currentChar != '\0') writtenText.RemoveAt(writtenText.Count - 1);
                        }

                    }
                }
            }
            else{
                lastKey = null;
                keyPressTime = 0;
            }
        }

        private char ConvertKeyToChar(Keys? key){
            if (key == null) return '\0';
            if (key >= Keys.A && key <= Keys.Z) return (char)('a' + key - Keys.A);
            if (key == Keys.Space) return ' ';
            if (key == Keys.Back) return '~';
            return '\0';
        }

        public void UserInputText(char[] printCharArray, Color color)
        {
            StringBuilder writeLine = new StringBuilder();
            int beginingOfWord = 0, currentLine = 0;
            for (int i = 0; i < printCharArray.Length; i++){
                if(printCharArray[i] == ' ' || i == printCharArray.Length-1){
                    int wordLength = i - beginingOfWord + 1;

                    if(currentLine < endLineIndexes.Count && i >= endLineIndexes[currentLine]-currentLine){
                        writeLine.Append('\n');
                        currentLine++;
                    }

                    writeLine.Append(new string(printCharArray, beginingOfWord, wordLength));
                    beginingOfWord = i+1;
                }
            }
            _spriteBatch.DrawString(font, writeLine.ToString(), new Vector2(80, yOffset), color);
            WriteMistakes(endLineIndexes);
        }

        
        public void WriteText(string printString, Color color, int line = 0, bool isHintText = false)
        {
            char[] printCharArray = printString.ToCharArray();
            StringBuilder writeLine = new StringBuilder(printString.Length);
            int beginingOfWord = 0, currentLineLength = 0;
            if(isHintText) endLineIndexes.Clear();
            for (int i = 0; i < printCharArray.Length; i++){
                if(printCharArray[i] == ' ' || i == printCharArray.Length-1){
                    int wordLength = i - beginingOfWord + 1;

                    if (currentLineLength + wordLength > maxCharsPerLine){
                        endLineIndexes.Add(writeLine.Length);
                        if(isHintText) writeLine.Append('\n');
                        currentLineLength = 0;
                    }

                    writeLine.Append(new string(printCharArray, beginingOfWord, wordLength));
                    currentLineLength += wordLength;
                    beginingOfWord = i + 1;
                }
            }
            _spriteBatch.DrawString(font, writeLine.ToString(), new Vector2(80, yOffset + (line * 30)), color);
        }
    }
}
