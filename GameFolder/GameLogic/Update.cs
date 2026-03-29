using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using System;
using typatro.GameFolder.Services;
using static typatro.GameFolder.Services.EnemyManager;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder
{
    public partial class GameLogic
    {
        public void Update(GameWindow window, bool isActive)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            HandleWindowDragging(window);
            if (gameState == GameState.NEWGAME || gameState == GameState.LOADGAME)
            {
                TypingSystem(window, keyboardState);
            }
            if (totalGameTimeMinutes != (long)MainGame.time.TotalGameTime.Minutes)
            {
                totalGameTimeMinutes = (long)MainGame.time.TotalGameTime.Minutes;
                SteamManager.IncrementStat(SteamManager.SteamStats.GameMinutes);
            }
            sfx.musicIntro.Volume = (float)SaveManager.volume / 10;
            sfx.musicMainTheme.Volume = (float)SaveManager.volume / 10;
            if (!introPlayed && sfx.musicIntro.State == SoundState.Stopped)
            {
                introPlayed = true;
                sfx.musicMainTheme.IsLooped = true;
                sfx.musicMainTheme.Play();
                if (!sfx.musicIntro.IsDisposed) sfx.musicIntro.Dispose();
            }
            windowActive = isActive;
        }

        private void HandleWindowDragging(GameWindow window)
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && windowActive)
            {
                if (new Rectangle(0, 0, MainGame.screenWidth, 15).Contains(mouseState.Position) ||
                    new Rectangle(0, 0, 15, MainGame.screenHeight).Contains(mouseState.Position) ||
                    new Rectangle(0, MainGame.screenHeight - 15, MainGame.screenWidth, MainGame.screenHeight).Contains(mouseState.Position) ||
                    new Rectangle(MainGame.screenWidth - 15, 0, MainGame.screenWidth, MainGame.screenHeight).Contains(mouseState.Position))
                {
                    if (!isDragging)
                    {
                        isDragging = true;
                        dragOffset = new Point(mouseState.X, mouseState.Y);
                    }
                }
            }

            if (mouseState.LeftButton == ButtonState.Released)
            {
                isDragging = false;
            }

            if (isDragging)
            {
                window.Position = new Point(mouseState.X + window.Position.X - dragOffset.X, mouseState.Y + window.Position.Y - dragOffset.Y);
            }
        }

        private void TypingSystem(GameWindow window, KeyboardState keyboardState)
        {
            bool houseBlocking = GlyphManager.IsActive(Glyph.House) && (int)timeInSeconds % 8 == 0 && timeInSeconds != 0;                                                                                                                    
            if (!houseBlocking)                                                                                                                                                                                                              
            {                                                                                                                                                                                                                                
                writer.ReadKeyboardInput(MainGame.time);                                                                                                                                                                                   
                writer.UpdateDiffIndexes(neededText);                                                                                                                                                                                        
            }   

            if (!startedTyping && Writer.writtenText.Count > 0)
            {
                startedTyping = true;
                timeInSeconds = 0;
            }

            if (startedTyping)
            {
                if (!isFightFinished)
                {
                    timeInSeconds += MainGame.time.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    letterTimer = 0;
                }

                if ((int)timeInSeconds != lastTime)
                {
                    if ((int)timeInSeconds % 10 == 0)
                    {
                        if (!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.B)) textRotation += Math.PI;
                        if (GlyphManager.IsActive(Glyph.M)) coins += 10;
                    }
                    if ((int)timeInSeconds % 5 == 0 && timeInSeconds != 0)
                    {
                        if (!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.EyeOfHorus)) eyeOfHorusActive = true;
                        if (!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.M))
                        {
                            xTextOffset = unseededRandom.Next(-100, 101);
                            yTextOffset = unseededRandom.Next(0, 101);
                        }
                        else
                        {
                            xTextOffset = 0;
                            yTextOffset = 0;
                        }
                    }
                    else eyeOfHorusActive = false;

                    if ((int)timeInSeconds % 4 == 0 && timeInSeconds != 0)
                        molochActive = Is(EnemyType.M);
                    else if ((int)timeInSeconds % 4 == 1)
                        molochActive = false;

                    if ((int)timeInSeconds % 5 == 0 && timeInSeconds != 0)
                        kHeperShieldActive = Is(EnemyType.H);
                    else if ((int)timeInSeconds % 5 == 2)
                        kHeperShieldActive = false;

                }
                lastTime = (int)timeInSeconds;

                if (Is(EnemyType.F) && !isFightFinished)
                {
                    if (jumpscareNextTime < 0)
                        jumpscareNextTime = timeInSeconds + unseededRandom.Next(10, 26);
                    if (timeInSeconds >= jumpscareNextTime)
                    {
                        jumpscareActive = true;
                        jumpscareEndTime = timeInSeconds + 0.4;
                        jumpscareNextTime = timeInSeconds + unseededRandom.Next(10, 26);
                        sfx.jumpscareSound.Play((float)SaveManager.volume / 10, 0f, 0f);
                    }
                }
                if (jumpscareActive && timeInSeconds >= jumpscareEndTime)
                    jumpscareActive = false;

                if (Is(EnemyType.G) && !isFightFinished)
                    textRotation += 0.03 * MainGame.time.ElapsedGameTime.TotalSeconds;

                if (Is(EnemyType.W) && !isFightFinished)
                {
                    if (wendigoBugs.Count == 0)
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            float angle = (float)(unseededRandom.NextDouble() * Math.PI * 2);
                            float speed = unseededRandom.Next(60, 140);
                            wendigoBugs.Add((
                                new Vector2(unseededRandom.Next(20, MainGame.screenWidth - 20), unseededRandom.Next(80, MainGame.screenHeight - 20)),
                                new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed)
                            ));
                        }
                    }
                    float dt = (float)MainGame.time.ElapsedGameTime.TotalSeconds;
                    for (int i = 0; i < wendigoBugs.Count; i++)
                    {
                        var (pos, vel) = wendigoBugs[i];
                        pos += vel * dt;
                        if (pos.X < 5 || pos.X > MainGame.screenWidth - 5)  vel.X = -vel.X;
                        if (pos.Y < 60 || pos.Y > MainGame.screenHeight - 5) vel.Y = -vel.Y;
                        wendigoBugs[i] = (pos, vel);
                    }
                }

                if (!GlyphManager.IsActive(Glyph.Sun) && GlyphManager.IsActive(Glyph.J))
                {
                    if (keyboardState.GetPressedKeyCount() > 0 && keyboardState.IsKeyUp(Keys.Tab))
                    {
                        window.Position = new Point(windowPos.X + unseededRandom.Next(-5, 6), windowPos.Y + unseededRandom.Next(-5, 6));
                    }
                    else window.Position = windowPos;
                }
            }
        }
    }
}