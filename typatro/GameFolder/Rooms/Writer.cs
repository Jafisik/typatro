using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder
{
    class Writer
    {
        readonly int maxCharsPerLine = 40, yOffset = 160;
        readonly SpriteBatch _spriteBatch;
        readonly SpriteFont font;
        List<char> writtenText;
        List<int> diffIndexes;
        List<int> endLineIndexes = new List<int>(); 
        Keys? lastKey = null;
        double keyPressTime = 0;
        readonly double repeatInterval = 0.5;
        int lastCheckedIndex = 0;


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

        private static char ConvertKeyToChar(Keys? key){
            if (key == null) return '\0';
            if (key >= Keys.A && key <= Keys.Z) return (char)('a' + key - Keys.A);
            if (key == Keys.Space) return ' ';
            if (key == Keys.Back) return '~';
            return '\0';
        }

        //Visualizes user input and highlights mistakes (prints user input and then prints wrongString,
        // which has ' ' for correct letters and the actual letters for wrong letters)
        public void UserInputText(char[] printCharArray, Color color)
        {
            StringBuilder writeLine = new StringBuilder();
            StringBuilder wrongString = new StringBuilder();
            int indexLine = 0;
            for (int i = 0; i < writtenText.Count; i++){
                if (indexLine < endLineIndexes.Count && i == endLineIndexes[indexLine] - indexLine){
                    writeLine.Append('\n');
                    wrongString.Append('\n');
                    indexLine++;
                }
                writeLine.Append(printCharArray[i]);
                wrongString.Append(diffIndexes.Contains(i) ? writtenText[i] : ' ');
            }

            string correctText = writeLine.ToString();
            string incorrectText = wrongString.ToString();

            Vector2 position = new Vector2(80, yOffset);

            float rotation = GlyphManager.IsActive(Glyph.ReverseText) ? MathF.PI : 0;

            _spriteBatch.DrawString(font, correctText, position + rotationPoint, color, rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
            _spriteBatch.DrawString(font, incorrectText, position + rotationPoint, Color.Red, rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
        }
        Vector2 rotationPoint;
        
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
            string finalText = writeLine.ToString();
            Vector2 position = new Vector2(80, yOffset + (line * 30));
            Vector2 size = font.MeasureString(finalText);
            rotationPoint = size / 2f;

            float rotation = GlyphManager.IsActive(Glyph.ReverseText) ? MathF.PI : 0;

            _spriteBatch.DrawString(font, finalText, position + rotationPoint, color, rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
        }
    }
}
