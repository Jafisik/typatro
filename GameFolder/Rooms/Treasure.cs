using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder;
using typatro.GameFolder.Services;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder.Rooms{

    class Treasure{
        Glyph currentGlyph;
        Enhancements enhancements;
        bool pickUp = true, keyDown;
        readonly int rectTopOffset = 200, rectWidth = 170, rectHeight = 60;
        readonly Rectangle panelRect;

        public Treasure(Enhancements enhancements){
            this.enhancements = enhancements;
            GlyphManager.Add(Glyph.NoGlyphsLeft);
            int panelWidth = 850;
            panelRect = new Rectangle((MainGame.screenWidth - panelWidth) / 2, 80, panelWidth, 480);
        }

        // Position of the checkmark/cross baked into Backgrounds/treasure.png, within its
        // native 128x64 canvas - yes.png/no.png are the same canvas size, so drawing either
        // over the same rect the background uses lines them up exactly on top of the
        // check/cross already in the background art.
        static readonly Rectangle yesHitboxNative = new Rectangle(48, 43, 14, 13);
        static readonly Rectangle noHitboxNative = new Rectangle(64, 42, 15, 14);

        public bool DisplayTreasure(ref long coins, ref bool mousePressed){
            MouseState mouseState = Mouse.GetState();
            Glyph glyph = currentGlyph;
            string treasureDescriptionText = GlyphManager.GetDescription(glyph);

            // Confined to the play field - below the top banner, with a matching gap at the
            // bottom - instead of stretching the art edge to edge over the whole window.
            Rectangle screenRect = new Rectangle(0, 55, MainGame.screenWidth, MainGame.screenHeight - 55 - 15);
            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.treasureBg, screenRect, Color.White);

            int leftOffset = panelRect.X + 80;
            int glyphSize = 150;
            Rectangle glyphRect = new Rectangle(panelRect.X + panelRect.Width / 2 - glyphSize / 2, panelRect.Y + 60, glyphSize, glyphSize);
            MainGame.Gfx.spriteBatch.Draw(GlyphManager.GetGlyphImage(glyph), glyphRect, ThemeColors.Foreground);
            float descScale = 1.15f;
            // Wrapped narrower than the panel itself so long descriptions (e.g. Cat's) stay
            // inside the visible circle instead of running past its edge.
            float wrapWidth = 520f;
            string wrappedDescription = GameLogic.WrapText(MainGame.Gfx.smallTextFont, treasureDescriptionText, wrapWidth);

            // Each line centered individually (rather than the whole block left-aligned) since
            // the wrapped lines vary a lot in width.
            int descCenterX = panelRect.X + panelRect.Width / 2;
            float descLineH = MainGame.Gfx.smallTextFont.LineSpacing * descScale;
            float descY = panelRect.Y + 225;
            foreach (string line in wrappedDescription.Split('\n'))
            {
                float lineWidth = MainGame.Gfx.smallTextFont.MeasureString(line).X * descScale;
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallTextFont, line, new Vector2(descCenterX - lineWidth / 2, descY),
                    ThemeColors.Text, 0f, Vector2.Zero, descScale, SpriteEffects.None, 0f);
                descY += descLineH;
            }

            if(glyph != Glyph.NoGlyphsLeft){
                var state = Keyboard.GetState();
                if(!keyDown && state.IsKeyDown(Keys.Left)){
                    pickUp = true;
                    keyDown = true;
                }
                else if(!keyDown && state.IsKeyDown(Keys.Right)){
                    pickUp = false;
                    keyDown = true;
                }
                if(state.IsKeyUp(Keys.Left) && state.IsKeyUp(Keys.Right)){
                    keyDown = false;
                }
                if(UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.TreasureTutorial) && state.IsKeyDown(Keys.Enter)){
                    if (pickUp)
                    {
                        GlyphManager.Add(glyph);
                        if (glyph == Glyph.Hundred) coins += 100;
                        enhancements.AddGlyphEnhancementsUpdate(glyph);
                    }
                    mousePressed = true;
                    return true;
                }
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    mousePressed = false;
                }

                float scaleX = screenRect.Width / 128f, scaleY = screenRect.Height / 64f;
                Rectangle yesRect = new Rectangle(screenRect.X + (int)(yesHitboxNative.X * scaleX), screenRect.Y + (int)(yesHitboxNative.Y * scaleY), (int)(yesHitboxNative.Width * scaleX), (int)(yesHitboxNative.Height * scaleY));
                Rectangle noRect = new Rectangle(screenRect.X + (int)(noHitboxNative.X * scaleX), screenRect.Y + (int)(noHitboxNative.Y * scaleY), (int)(noHitboxNative.Width * scaleX), (int)(noHitboxNative.Height * scaleY));

                if (!TutorialManager.IsShowing() && yesRect.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
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
                if (!TutorialManager.IsShowing() && noRect.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
                {
                    if (!mousePressed && mouseState.LeftButton == ButtonState.Pressed)
                    {
                        mousePressed = true;
                        return true;
                    }
                    pickUp = false;
                }

                MainGame.Gfx.spriteBatch.Draw(pickUp ? MainGame.Gfx.yesOverlay : MainGame.Gfx.noOverlay, screenRect, Color.White);
            }
            else{
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(leftOffset, rectTopOffset, rectWidth, rectHeight), ThemeColors.NotSelected);
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle(leftOffset*2+rectHeight, rectTopOffset, rectWidth, rectHeight), ThemeColors.Selected);
            }
            return false;
        }

        public void NewGlyph(bool forceCat = false){
            currentGlyph = forceCat ? Glyph.Cat : GlyphManager.GetRandomUnusedGlyph();
        }

        
    }
}