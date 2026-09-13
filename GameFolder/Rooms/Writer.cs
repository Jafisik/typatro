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

        // Draws text character-by-character with a vertical sine wave offset (Apnea).
        // Ignores rotation - A never runs alongside another rotating enemy.
        private static void DrawWavyText(SpriteFont font, string text, Vector2 position, Color color)
        {
            double time = MainGame.time.TotalGameTime.TotalSeconds;
            float lineHeight = font.LineSpacing;
            Vector2 cursor = Vector2.Zero;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\n')
                {
                    cursor.X = 0;
                    cursor.Y += lineHeight;
                    continue;
                }
                float wave = (float)Math.Sin(time * 5 + i * 0.4) * 4f;
                MainGame.Gfx.spriteBatch.DrawString(font, c.ToString(), position + cursor + new Vector2(0, wave), color);
                cursor.X += font.MeasureString(c.ToString()).X;
            }
        }

        //Visualizes user input and highlights mistakes (prints user input and then prints wrongString,
        // which has ' ' for correct letters and the actual letters for wrong letters)
        public void UserInputText(char[] printCharArray, int mistakeBlock, long wordBase = 0, double rotation = 0, int xExtraOffset = 0, int yExtraOffset = 0){
            
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
                    if (Services.EnemyManager.Is(Services.EnemyType.U))
                    {
                        wrongString.Append(' ');
                        blockedString.Append(' ');
                        writeLine.Append(printCharArray[i]);
                    }
                    else if (mistakeBlock == 0)
                    {
                        wrongString.Append(writtenText[i]);
                        blockedString.Append(' ');
                        writeLine.Append(' ');
                    }
                    else
                    {
                        mistakeBlock--;
                        wrongString.Append(' ');
                        blockedString.Append(writtenText[i]);
                        writeLine.Append(' ');
                    }
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
            float cursorW = charSize.X - 3;
            float cursorH = 3;
            float cursorX = charIndex * charSize.X + position.X;
            float cursorY = (indexLine + 1) * charSize.Y + position.Y - 1;
            if (Services.EnemyManager.Is(Services.EnemyType.A))
                cursorY += (float)Math.Sin(MainGame.time.TotalGameTime.TotalSeconds * 5 + writtenText.Count * 0.4) * 4f;
            Vector2 rotCenter = position + rotationPoint;
            Vector2 rel = new Vector2(cursorX, cursorY) - rotCenter;
            float cos = (float)Math.Cos(rotation), sin = (float)Math.Sin(rotation);
            Vector2 rotatedCursorPos = rotCenter + new Vector2(rel.X * cos - rel.Y * sin, rel.X * sin + rel.Y * cos);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, rotatedCursorPos, null, ThemeColors.Selected, (float)rotation, Vector2.Zero, new Vector2(cursorW, cursorH), SpriteEffects.None, 0f);
            if (Services.EnemyManager.Is(Services.EnemyType.A))
            {
                DrawWavyText(MainGame.Gfx.textFont, correctText, position, ThemeColors.Text);
                DrawWavyText(MainGame.Gfx.textFont, incorrectText, position, ThemeColors.Wrong);
                DrawWavyText(MainGame.Gfx.textFont, blockedText, position, ThemeColors.Blocked);
            }
            else
            {
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, correctText, position + rotationPoint, ThemeColors.Text, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, incorrectText, position + rotationPoint, ThemeColors.Wrong, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, blockedText, position + rotationPoint, ThemeColors.Blocked, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
            }

            // The current word's live base, drawn above the cursor using the same rotation (and
            // enemy A's wavy cursorY) as the cursor itself, so it stays glued to the caret
            // instead of drifting off when the text rotates or wobbles. Always sits above the
            // first line, even once typing has wrapped onto a later line.
            if (wordBase != 0)
            {
                string baseText = wordBase.ToString();
                Vector2 baseSize = MainGame.Gfx.textFont.MeasureString(baseText);
                float firstLineY = charSize.Y + position.Y - 1;
                if (Services.EnemyManager.Is(Services.EnemyType.A))
                    firstLineY += (float)Math.Sin(MainGame.time.TotalGameTime.TotalSeconds * 5 + writtenText.Count * 0.4) * 4f;
                Vector2 baseLocal = new Vector2(cursorX - baseSize.X / 2 - 13 + charSize.X * 0.25f, firstLineY - 55);
                Vector2 baseRel = baseLocal - rotCenter;
                Vector2 rotatedBasePos = rotCenter + new Vector2(baseRel.X * cos - baseRel.Y * sin, baseRel.X * sin + baseRel.Y * cos);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, baseText, rotatedBasePos, ThemeColors.Correct, (float)rotation, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }
        Vector2 rotationPoint;

        public void WriteText(string printString, Color color, List<int> shinyWords = null, List<int> stoneWords = null, List<int> bloomWords = null,
            int line = 0, bool isHintText = false, double rotation = 0, int xExtraOffset = 0, int yExtraOffset = 0, bool treasure = false,
            int flashWordIndex = -1, Color flashColor = default, float flashAlpha = 0f)
        {
            int word = 0;
            maxCharsPerLine = 50;

            char[] printCharArray = printString.ToCharArray();
            StringBuilder writeLine = new StringBuilder(printString.Length);
            StringBuilder shinyWriteLine = new StringBuilder(printString.Length);
            StringBuilder stoneWriteLine = new StringBuilder(printString.Length);
            StringBuilder bloomWriteLine = new StringBuilder(printString.Length);
            // Background box drawn behind a special word the moment it's completed (hit or
            // miss) - a single fading flash, never a lasting mark.
            (int lineNum, int col, int len, Color color, float alpha)? highlightBox = null;
            int beginingOfWord = 0, currentLineLength = 0, currentLineNumber = 0;
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
                        currentLineNumber++;
                    }
                    if(shinyWords != null && stoneWords != null && bloomWords != null)
                    {
                        if (shinyWords.Contains(word)) shinyWriteLine.Append(new string(printCharArray, beginingOfWord, wordLength));
                        else shinyWriteLine.Append(new string(' ', wordLength));
                        if (stoneWords.Contains(word)) stoneWriteLine.Append(new string(printCharArray, beginingOfWord, wordLength));
                        else stoneWriteLine.Append(new string(' ', wordLength));
                        if (bloomWords.Contains(word)) bloomWriteLine.Append(new string(printCharArray, beginingOfWord, wordLength));
                        else bloomWriteLine.Append(new string(' ', wordLength));
                        if (word == flashWordIndex && flashAlpha > 0f)
                        {
                            // wordLength includes the trailing space (or, for the very last
                            // word, doesn't) - the box itself should only cover the letters.
                            int boxLen = Math.Max(1, printCharArray[i] == ' ' ? wordLength - 1 : wordLength);
                            highlightBox = (currentLineNumber, currentLineLength, boxLen, flashColor, flashAlpha * 0.5f);
                        }
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

            if (highlightBox is { } box)
            {
                Vector2 charSize = MainGame.Gfx.textFont.MeasureString(" ");
                bool wavy = Services.EnemyManager.Is(Services.EnemyType.A);
                Vector2 boxSize = new Vector2(box.len * charSize.X, charSize.Y - 2f);
                Vector2 boxLocal = new Vector2(position.X + box.col * charSize.X, position.Y + box.lineNum * charSize.Y + 1f);
                Vector2 drawPos;
                if (wavy)
                    drawPos = boxLocal;
                else
                {
                    Vector2 rotCenter = position + rotationPoint;
                    float cos = (float)Math.Cos(rotation), sin = (float)Math.Sin(rotation);
                    Vector2 rel = boxLocal - rotCenter;
                    drawPos = rotCenter + new Vector2(rel.X * cos - rel.Y * sin, rel.X * sin + rel.Y * cos);
                }
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, drawPos, null, box.color * box.alpha, wavy ? 0f : (float)rotation, Vector2.Zero, boxSize, SpriteEffects.None, 0f);
            }

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

            if (Services.EnemyManager.Is(Services.EnemyType.A))
            {
                DrawWavyText(MainGame.Gfx.textFont, finalText, position, color);
                DrawWavyText(MainGame.Gfx.textFont, shinyWriteLine.ToString(), position, Color.Gold);
                DrawWavyText(MainGame.Gfx.textFont, stoneWriteLine.ToString(), position, Color.Gray);
                DrawWavyText(MainGame.Gfx.textFont, bloomWriteLine.ToString(), position, Color.DarkGreen);
            }
            else
            {
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, finalText, position + rotationPoint, color, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, shinyWriteLine.ToString(), position + rotationPoint, Color.Gold, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, stoneWriteLine.ToString(), position + rotationPoint, Color.Gray, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.textFont, bloomWriteLine.ToString(), position + rotationPoint, Color.DarkGreen, (float)rotation, rotationPoint, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}
