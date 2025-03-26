using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace typatro.GameFolder
{
    class Writer
    {
        private int maxCharsPerLine = 40;
        private int yOffset = 80;
        private SpriteBatch _spriteBatch;
        private SpriteFont font;
        List<char> writtenText;
        List<int> diffIndexes;
        List<int> endLineIndexes = new List<int>();
        Keys? lastKey = null;
        double keyPressTime = 0;
        double repeatInterval = 0.5;
        private int lastCheckedIndex = 0;


        public Writer(SpriteBatch _spriteBatch, SpriteFont font, List<int> diffIndexes, List<char> writtenText)
        {
            this._spriteBatch = _spriteBatch;
            this.font = font;
            this.writtenText = writtenText;
            this.diffIndexes = diffIndexes;
        }

        public void UpdateDiffIndexes(string compareText)
        {
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

        public void WriteMistakes(){
            List<char> wrongString = new List<char>(new string(' ', writtenText.Count+50)); 
            int line = 0, letterOffset = 0, offsetSum = 0;
            foreach (int index in diffIndexes){
                if(endLineIndexes.Count > line){
                    if(index > endLineIndexes[line]){
                        letterOffset += maxCharsPerLine + offsetSum - endLineIndexes[line];
                        offsetSum = endLineIndexes[line];
                        line++;
                    }
                }
                
                if (index < wrongString.Count) {
                    int offsetIndex;
                    if(line - 1 < 0){
                        offsetIndex = index+letterOffset+line;
                    } else{
                        offsetIndex = index+letterOffset+line-1;
                    }
                    wrongString[offsetIndex] = writtenText[index];
                }
            }
            
            WriteText(new string(wrongString.ToArray()), Color.Red);
        }

        public void ReadKeyboardInput(GameTime gameTime)
        {
            KeyboardState currentState = Keyboard.GetState();
            Keys[] pressedKeys = currentState.GetPressedKeys();
            bool shiftPressed = currentState.IsKeyDown(Keys.LeftShift) || currentState.IsKeyDown(Keys.RightShift);

            if (pressedKeys.Length != 0)
            {
                Keys currentKey = pressedKeys[0];
                if (currentKey != lastKey)
                {
                    keyPressTime = 0;

                    char currentChar = ConvertKeyToChar(currentKey, shiftPressed);
                    if (currentChar != '~' && currentChar != '\0') writtenText.Add(currentChar);
                    else
                    {
                        if (writtenText.Count > 0 && currentChar != '\0') writtenText.RemoveAt(writtenText.Count - 1);
                    }

                    lastKey = currentKey;
                }
                else
                {
                    keyPressTime += gameTime.ElapsedGameTime.TotalSeconds;

                    if (keyPressTime >= repeatInterval)
                    {
                        char currentChar = ConvertKeyToChar(currentKey, shiftPressed);
                        if (currentChar != '~' && currentChar != '\0') writtenText.Add(currentChar);
                        else
                        {
                            if (writtenText.Count > 0 && currentChar != '\0') writtenText.RemoveAt(writtenText.Count - 1);
                        }

                    }
                }
            }
            else
            {
                lastKey = null;
                keyPressTime = 0;
            }
        }

        private char ConvertKeyToChar(Keys? key, bool shift){
            if (key == null) return '\0';

            bool capsLock = Console.CapsLock;

            if (key >= Keys.A && key <= Keys.Z){
                bool isUpper = capsLock && !shift || !capsLock && shift;
                return (char)((isUpper ? 'A' : 'a') + (key - Keys.A));
            }

            if (key >= Keys.D0 && key <= Keys.D9){
                if (shift){
                    return key switch{
                        Keys.D1 => '!',
                        Keys.D2 => '@',
                        Keys.D3 => '#',
                        Keys.D4 => '$',
                        Keys.D5 => '%',
                        Keys.D6 => '^',
                        Keys.D7 => '&',
                        Keys.D8 => '*',
                        Keys.D9 => '(',
                        Keys.D0 => ')',
                        _ => '\0'
                    };
                }
                return (char)('0' + (key - Keys.D0));
            }
            //Any other special characters just don't work with shift :/
            if (key == Keys.Space) return ' ';
            if (key == Keys.Back) return '~';

            return '\0';
        }

        public void WriteText(char[] printCharArray, Color color, int line = 0)
        {
            //Print written text to screen
            string writeLine = "";
            int beginingOfWord = 0, currentLineLength = 0;
            endLineIndexes.Clear(); 
            for (int i = 0; i < printCharArray.Length; i++){
                if(printCharArray[i] == ' ' || i == printCharArray.Length-1){
                    int wordLength = i - beginingOfWord + 1;

                    if (currentLineLength + wordLength > maxCharsPerLine){
                        endLineIndexes.Add(writeLine.Length);
                        writeLine += "\n";
                        currentLineLength = 0;
                    }

                    writeLine += new string(printCharArray, beginingOfWord, wordLength);
                    currentLineLength += wordLength;
                    beginingOfWord = i+1;
                }
                
            }
            _spriteBatch.DrawString(font, writeLine, new Vector2(80, yOffset), color);
        }

        public void WriteText(string printString, Color color, int line = 0)
        {
            char[] printCharArray = printString.ToCharArray();
            string writeLine = "";
            int beginingOfWord = 0, currentLineLength = 0;
            for (int i = 0; i < printCharArray.Length; i++){
                if(printCharArray[i] == ' ' || i == printCharArray.Length-1){
                    int wordLength = i - beginingOfWord + 1;

                    if (currentLineLength + wordLength > maxCharsPerLine){
                        writeLine += "\n";
                        currentLineLength = 0;
                    }

                    writeLine += new string(printCharArray, beginingOfWord, wordLength);
                    currentLineLength += wordLength;
                    beginingOfWord = i+1;
                }
                
            }
            _spriteBatch.DrawString(font, writeLine, new Vector2(80, yOffset), color);
        }
    }
}
