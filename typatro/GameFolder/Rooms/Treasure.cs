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
        bool pickUp = true, keyDown;
        readonly int topOffset = 80, leftOffset = 50, descOffset = 50, rectTopOffset = 200, rectWidth = 100, rectHeight = 100;

        public Treasure(SpriteBatch spriteBatch, SpriteFont bigFont, SpriteFont smallFont, Texture2D texture){
            this.spriteBatch = spriteBatch;
            this.bigFont = bigFont;
            this.smallFont = smallFont;
            this.texture = texture;
            GlyphManager.Add(Glyph.NoGlyphsLeft);
        }

        public void DisplayTreasure(){
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
                if(state.IsKeyDown(Keys.Enter) && pickUp) GlyphManager.Add(glyph);

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
    }
}