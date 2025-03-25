using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            for (int i = lastCheckedIndex; i < Math.Min(writtenText.Count, compareText.Length); i++)
            {
                if (writtenText[i] != compareText [i]) if (!diffIndexes.Contains(i)) diffIndexes.Add(i);
                    else diffIndexes.Remove(i);
            }
            diffIndexes.RemoveAll(index => index >= writtenText.Count);
            lastCheckedIndex = writtenText.Count;
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

        private char ConvertKeyToChar(Keys? key, bool shift)
        {
            if (key == null) return '\0';

            bool capsLock = Console.CapsLock;

            if (key >= Keys.A && key <= Keys.Z)
            {
                bool isUpper = capsLock && !shift || !capsLock && shift;
                return (char)((isUpper ? 'A' : 'a') + (key - Keys.A));
            }

            if (key >= Keys.D0 && key <= Keys.D9)
            {
                if (shift)
                {
                    return key switch
                    {
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
            for (int i = 0; i < printCharArray.Length; i += maxCharsPerLine)
            {
                int length = Math.Min(maxCharsPerLine, printCharArray.Length - i);
                char[] lineChars = new char[length];
                Array.Copy(printCharArray, i, lineChars, 0, length);

                string writeLine = new string(lineChars);
                _spriteBatch.DrawString(font, writeLine + "\n", new Vector2(80, yOffset + (font.LineSpacing * line)), color);
                line++;
            }
            //Highlight wrong characters
            foreach (int diffCharIndex in diffIndexes)
            {
                WriteText(writtenText.ElementAt(diffCharIndex), Color.Red, diffCharIndex);
            }
        }

        public void WriteText(string printString, Color color, int line = 0)
        {
            char[] printCharArray = printString.ToCharArray();
            //Print written text to screen
            for (int i = 0; i < printCharArray.Length; i += maxCharsPerLine)
            {
                int length = Math.Min(maxCharsPerLine, printCharArray.Length - i);
                char[] lineChars = new char[length];
                Array.Copy(printCharArray, i, lineChars, 0, length);

                string writeLine = new string(lineChars);
                _spriteBatch.DrawString(font, writeLine + "\n" + "aaaaaaaa", new Vector2(80, yOffset + (font.LineSpacing * line)), color);
                line++;
            }
        }


        public void WriteText(char printChar, Color color, int charPos)
        {
            int fontSize = 16;
            _spriteBatch.DrawString(
                font,
                printChar.ToString(),
                new Vector2(
                    yOffset + charPos * fontSize % (fontSize * maxCharsPerLine),
                    yOffset + font.LineSpacing * (float)Math.Floor(charPos * fontSize / (float)(fontSize * maxCharsPerLine))
                ),
                Color.Red
            );
        }
    }
}
