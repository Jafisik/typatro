using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder.Rooms{

    class Treasure{
        Glyph currentGlyph;
        Enhancements enhancements;
        bool pickUp = true, keyDown;
        readonly int topOffset = 80, leftOffset = 50, rectTopOffset = 200, rectWidth = 170, rectHeight = 60, rectOffset;

        public Treasure(Enhancements enhancements){
            this.enhancements = enhancements;
            GlyphManager.Add(Glyph.NoGlyphsLeft);
            rectOffset = MainGame.screenWidth/4;
        }

        public bool DisplayTreasure(ref long coins, ref bool mousePressed){
            MouseState mouseState = Mouse.GetState();
            Glyph glyph = currentGlyph;
            string treasureDescriptionText = GlyphManager.GetDescription(glyph);
            MainGame.Gfx.spriteBatch.Draw(GlyphManager.GetGlyphImage(glyph), new Rectangle(leftOffset, topOffset, 128, 128), ThemeColors.Foreground);
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, treasureDescriptionText, new Vector2(leftOffset, topOffset*3), ThemeColors.Text);

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
                    GlyphManager.Add(glyph);
                    if (glyph == Glyph.Hundred) coins += 100;
                    enhancements.AddGlyphEnhancementsUpdate(glyph);
                    mousePressed = true;
                    return true;
                }
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    mousePressed = false;
                }

                Rectangle yesRect = new Rectangle(rectOffset, topOffset*5, rectWidth, rectHeight);
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

                Vector2 yesSize = MainGame.Gfx.gameFont.MeasureString("accept");
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, yesRect, pickUp ? ThemeColors.Selected : ThemeColors.NotSelected);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "accept", new Vector2(yesRect.X + yesRect.Width / 2 - yesSize.X / 2, yesRect.Y + yesRect.Height / 2 - yesSize.Y / 2+5), ThemeColors.Text);


                Rectangle noRect = new Rectangle(rectOffset*2, topOffset*5, rectWidth, rectHeight);
                if (noRect.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
                {
                    if (!mousePressed && mouseState.LeftButton == ButtonState.Pressed)
                    {
                        mousePressed = true;
                        return true;
                    }
                    pickUp = false;
                }
                Vector2 noSize = MainGame.Gfx.gameFont.MeasureString("decline");
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, noRect, pickUp ? ThemeColors.NotSelected : ThemeColors.Selected);
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.gameFont, "decline", new Vector2(noRect.X + noRect.Width/2 - noSize.X/2, noRect.Y + noRect.Height / 2 - noSize.Y / 2+5), ThemeColors.Text);

            } 
            else{
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(leftOffset, rectTopOffset, rectWidth, rectHeight), ThemeColors.NotSelected);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(leftOffset*2+rectHeight, rectTopOffset, rectWidth, rectHeight), ThemeColors.Selected);
            }
            return false;
        }

        public void NewGlyph(){
            currentGlyph = GlyphManager.GetRandomUnusedGlyph();
        }

        
    }
}