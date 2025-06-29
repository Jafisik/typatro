using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder.Rooms{

    class Treasure{
        MainGame.Gfx gfx;
        Glyph currentGlyph;
        Enhancements enhancements;
        bool pickUp = true, keyDown;
        readonly int topOffset = 80, leftOffset = 50, descOffset = 150, rectTopOffset = 200, rectWidth = 100, rectHeight = 50;

        public Treasure(MainGame.Gfx gfx, Enhancements enhancements){
            this.gfx = gfx;
            this.enhancements = enhancements;
            GlyphManager.Add(Glyph.NoGlyphsLeft);
        }

        public bool DisplayTreasure(ref long coins, ref bool mousePressed){
            MouseState mouseState = Mouse.GetState();
            Glyph glyph = currentGlyph;
            string treasureDescriptionText = GlyphManager.GetDescription(glyph);
            gfx.spriteBatch.Draw(GlyphManager.GetGlyphImage(glyph), new Rectangle(leftOffset, topOffset, 128, 128), ThemeColors.Foreground);
            gfx.spriteBatch.DrawString(gfx.smallTextFont, treasureDescriptionText, new Vector2(leftOffset, topOffset+descOffset), ThemeColors.Text);

            if(glyph != Glyph.NoGlyphsLeft){
                var state = Keyboard.GetState();
                if(!keyDown && (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.Right))){
                    pickUp = !pickUp;
                    keyDown = true;
                }
                if(state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right)){
                    keyDown = false;
                }
                if(state.IsKeyDown(Keys.Enter)){
                    
                    return true;
                }
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    mousePressed = false;
                }

                Rectangle yesRect = new Rectangle(leftOffset, rectTopOffset + descOffset * 2, rectWidth, rectHeight);
                if (yesRect.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
                {
                    if (!mousePressed && mouseState.LeftButton == ButtonState.Pressed)
                    {
                        GlyphManager.Add(glyph);
                        if (glyph == Glyph.Hundred) coins += 100;
                        enhancements.AddGlyphEnhancementsUpdate(glyph);
                        mousePressed = true;
                        return true;
                    }
                    pickUp = true;
                }
                gfx.spriteBatch.Draw(gfx.texture, yesRect, pickUp ? ThemeColors.Selected : ThemeColors.NotSelected);
                gfx.spriteBatch.DrawString(gfx.gameFont, "yes", new Vector2(leftOffset + 10, rectTopOffset + descOffset * 2 + 5), ThemeColors.Text);


                Rectangle noRect = new Rectangle(leftOffset + rectWidth, rectTopOffset + descOffset * 2, rectWidth, rectHeight);
                if (noRect.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
                {
                    if (!mousePressed && mouseState.LeftButton == ButtonState.Pressed)
                    {
                        mousePressed = true;
                        return true;
                    }
                    pickUp = false;
                }
                gfx.spriteBatch.Draw(gfx.texture, noRect, pickUp ? ThemeColors.NotSelected : ThemeColors.Selected);
                gfx.spriteBatch.DrawString(gfx.gameFont, "no", new Vector2(leftOffset * 2 + rectHeight + 10, rectTopOffset + descOffset * 2 + 5), ThemeColors.Text);

            } 
            else{
                gfx.spriteBatch.Draw(gfx.texture, new Rectangle(leftOffset, rectTopOffset, rectWidth, rectHeight), ThemeColors.NotSelected);
                gfx.spriteBatch.Draw(gfx.texture, new Rectangle(leftOffset*2+rectHeight, rectTopOffset, rectWidth, rectHeight), ThemeColors.Selected);
            }
            return false;
        }

        public void NewGlyph(){
            currentGlyph = GlyphManager.GetRandomUnusedGlyph();
        }

        
    }
}