using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.Services;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder
{
    public partial class GameLogic
    {
        //Handles the map logic and the selection of new rooms
        private void MapHandler(KeyboardState state)
        {
            MouseState mouseState = Mouse.GetState();
            gameUi.TopBannerDisplay(true);
            if (state.IsKeyDown(Keys.Tab))
            {
                gameUi.Inventory(state);
            }
            else if (!inventoryUp)
            {
                map.DrawNodes();
            }
            if (!firstEnter || state.IsKeyUp(Keys.Enter))
            {
                firstEnter = false;
                if (state.IsKeyUp(Keys.Enter) && mouseState.LeftButton == ButtonState.Released) tutorial = true;
                if (state.IsKeyUp(Keys.Tab) && !inventoryUp && UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.MapTutorial) && tutorial)
                {
                    MapNode newNode = map.NodeSelect(selectedNode, ref mousePressed, selectedNode.column, level);

                    if (newNode != selectedNode)
                    {
                        visitedNodes.Add(new int[] { selectedNode.column, selectedNode.row });
                        Reset();
                        if (GlyphManager.IsActive(Glyph.Cat)) catPos = new Vector2(unseededRandom.Next(100, MainGame.screenWidth - 100), unseededRandom.Next(100, MainGame.screenHeight - 100));
                        if (newNode.type == NodeType.RANDOM) newNode.type = map.GenerateNodeTypeFromRandom();
                        switch (newNode.type)
                        {
                            case NodeType.FIGHT:
                                fight = Fight.Create(1, level, newNode.column);
                                break;
                            case NodeType.ELITE:
                                fight = Fight.Create(2, level, newNode.column);
                                break;
                            case NodeType.BOSS:
                                fight = Fight.Create(3, level, newNode.column);
                                break;
                            case NodeType.TREASURE:
                                treasure.NewGlyph();
                                break;
                            case NodeType.SHOP:
                                shop.NewShop();
                                if (GlyphManager.IsActive(Glyph.Life))
                                {
                                    if (!isReplay) actions.Add(new UserAction("randomLetter", ""));
                                    enhancements.AddLetterScore((char)(seededRandom.Next(0, 26) + 'a'), 5);
                                }
                                break;
                            case NodeType.CURSE:
                                curseRoom.NewCurse();
                                break;
                        }
                        if (IsFight(newNode.type))
                        {
                            neededText = RandomTextGenerate(fight.words + (GlyphManager.IsActive(Glyph.Papyrus) ? 20 : 0) - (difficulty >= 5 ? 5 : 0));
                            if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.FightTutorial))
                                TutorialManager.Start(TutorialManager.FightSteps(), waitForRelease: true);
                        }
                        if (difficulty >= 4) fight.speed *= 2;
                        Writer.writtenText.Clear();
                        startedTyping = false;
                        lastSelectedNode = selectedNode;
                        selectedNode = newNode;
                        roomSelected = true;
                    }
                }
            }
            if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.MapTutorial))
            {
                if (!mapTutorialStarted)
                {
                    TutorialManager.Start(TutorialManager.MapSteps());
                    mapTutorialStarted = true;
                }
                SpriteFont font = SaveManager.size == 0 ? MainGame.Gfx.smallTextFont : MainGame.Gfx.menuFont;
                if (TutorialManager.Draw(state, mouseState, font))
                    UnlockManager.UnlockUnlock(UnlockManager.UnlockType.MapTutorial);
            }
        }
    }
}
