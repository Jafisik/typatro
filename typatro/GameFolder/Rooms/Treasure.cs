using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder.Rooms{

    class Treasure{
        SpriteBatch spriteBatch;
        SpriteFont bigFont, smallFont;
        Texture2D texture;
        Glyph currentGlyph;
        Enhancements enhancements;
        Random random = new Random();
        bool pickUp = true, keyDown;
        readonly int topOffset = 80, leftOffset = 50, descOffset = 50, rectTopOffset = 200, rectWidth = 100, rectHeight = 100;

        public Treasure(SpriteBatch spriteBatch, SpriteFont bigFont, SpriteFont smallFont, Texture2D texture, Enhancements enhancements){
            this.spriteBatch = spriteBatch;
            this.bigFont = bigFont;
            this.smallFont = smallFont;
            this.texture = texture;
            this.enhancements = enhancements;
            GlyphManager.Add(Glyph.NoGlyphsLeft);
        }

        public void DisplayTreasure(ref int coins){
            Glyph glyph = currentGlyph;
            string treasureText = glyph.ToString();
            string treasureDescriptionText = GlyphManager.GetDescription(glyph);
            spriteBatch.DrawString(bigFont, treasureText, new Vector2(leftOffset,topOffset), Color.Black);
            spriteBatch.DrawString(smallFont, treasureDescriptionText, new Vector2(leftOffset,topOffset+descOffset), Color.Black);

            if(glyph != Glyph.NoGlyphsLeft){
                var state = Keyboard.GetState();
                if(!keyDown && (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.Right))){
                    pickUp = !pickUp;
                    keyDown = true;
                }
                if(state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right)){
                    keyDown = false;
                }
                if(state.IsKeyDown(Keys.Enter) && pickUp){
                    GlyphManager.Add(glyph);
                    if(glyph == Glyph.Hundred) coins += 100;
                    AddGlyphEnhancementsUpdate(glyph);
                }

                spriteBatch.Draw(texture, new Rectangle(leftOffset, rectTopOffset, rectWidth, rectHeight), pickUp ? Color.Gray : Color.LightGray);
                spriteBatch.DrawString(bigFont, "yes", new Vector2(leftOffset, rectTopOffset), Color.Black);
                spriteBatch.Draw(texture, new Rectangle(leftOffset*2+rectHeight, rectTopOffset, rectWidth, rectHeight), pickUp ? Color.LightGray : Color.Gray);
                spriteBatch.DrawString(bigFont, "no", new Vector2(leftOffset*2+rectHeight, rectTopOffset), Color.Black);
            } 
            else{
                spriteBatch.Draw(texture, new Rectangle(leftOffset, rectTopOffset, rectWidth, rectHeight), Color.Gray);
                spriteBatch.Draw(texture, new Rectangle(leftOffset*2+rectHeight, rectTopOffset, rectWidth, rectHeight), Color.LightGray);
            }
        }

        public void NewGlyph(){
            currentGlyph = GlyphManager.GetRandomUnusedGlyph();
        }

        public void AddGlyphEnhancementsUpdate(Glyph glyph){
            switch(glyph){
                case Glyph.A:
                    enhancements.AddLetterScore('a',5);
                    enhancements.AddLetterScore('e',5);
                    enhancements.AddLetterScore('i',5);
                    enhancements.AddLetterScore('o',5);
                    enhancements.AddLetterScore('u',5);
                    break;
                case Glyph.D:   
                    enhancements.MultiplyLetterScore('d',4);
                    break;
                case Glyph.H:
                    for(int i = 0; i < 10; i++){
                        enhancements.MultiplyLetterScore((char)(random.Next(0,26)+'a'),5);
                    }
                    break;
                case Glyph.K:
                    enhancements.AllLettersAddScore(10);
                    break;
                case Glyph.Water:
                    for(int i = 0; i < 5; i++){
                        enhancements.AddLetterScore((char)(random.Next(0,26)+'a'),-5);
                    }
                    break;
                case Glyph.King:
                    long maxVal = enhancements.letters.Max();
                    long minVal = enhancements.letters.Min();
                    bool setZero = maxVal != minVal;
                    for(int i = 0; i < enhancements.letters.Length; i++){
                        if(enhancements.letters[i] == maxVal) enhancements.letters[i] *= 5;
                        if(setZero && enhancements.letters[i] == minVal) enhancements.letters[i] = 0;
                    }
                    break;
                case Glyph.Cat:
                    for(int i = 0; i < 9; i++){
                        enhancements.MultiplyLetterScore((char)(random.Next(0,26)+'a'),2);
                    }
                    break;
                case Glyph.Crocodile:
                    enhancements.MultiplyLetterScore((char)(random.Next(0,26)+'a'),20);
                    enhancements.letters[(char)(random.Next(0,26)+'a')] = 0;
                    break;
                case Glyph.One:
                    enhancements.AllLettersAddScore(1);
                    break;
                case Glyph.Ten:
                    enhancements.MultiplyLetterScore((char)(random.Next(0,26)+'a'),10);
                    break;
                case Glyph.Bread:
                    for(int i = 0; i < 6; i++){
                        enhancements.MultiplyLetterScore((char)(random.Next(0,26)+'a'),3);
                    }
                    break;
                case Glyph.Star:
                    enhancements.AllLettersMultiplyScore(20);
                    break;
            }
        }
    }
}