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
                        SetContext(newNode.column, newNode.row, level);
                        Reset();
                        if (GlyphManager.IsActive(Glyph.Cat)) catPos = new Vector2(unseededRandom.Next(100, MainGame.screenWidth - 100), unseededRandom.Next(100, MainGame.screenHeight - 100));
                        if (newNode.type == NodeType.RANDOM) newNode.type = map.GenerateNodeTypeFromRandom();
                        switch (newNode.type)
                        {
                            case NodeType.FIGHT:
                            case NodeType.ELITE:
                            case NodeType.BOSS:
                                int fightDifficulty = newNode.type == NodeType.FIGHT ? 1 : newNode.type == NodeType.ELITE ? 2 : 3;
                                fight = Fight.Create(fightDifficulty, level, newNode.column);
                                if (newNode.type == NodeType.FIGHT)
                                {
                                    int enemyIndex = unseededRandom.Next(MainGame.Gfx.enemyNormal.Length);
                                    currentEnemy = MainGame.Gfx.enemyNormal[enemyIndex];
                                    currentEnemyDesc = EnemyDescriptions[enemyIndex];
                                }
                                else if (newNode.type == NodeType.ELITE)
                                {
                                    currentEnemy = MainGame.Gfx.enemyElite;
                                    currentEnemyDesc = "Elite enemy";
                                }
                                else
                                {
                                    currentEnemy = MainGame.Gfx.enemyBoss;
                                    currentEnemyDesc = "Boss enemy";
                                }
                                break;
                            case NodeType.TREASURE:
                                treasure.NewGlyph();
                                break;
                            case NodeType.SHOP:
                                shop.NewShop();
                                if (GlyphManager.IsActive(Glyph.Life))
                                {
                                    enhancements.AddLetterScore((char)(contextRandom.Next(0, 26) + 'a'), 5);
                                }
                                break;
                            case NodeType.CURSE:
                                curseRoom.NewCurse();
                                break;
                        }
                        if (IsFight(newNode.type))
                        {
                            neededText = RandomTextGenerate(fight.words + (GlyphManager.IsActive(Glyph.Papyrus) ? 20 : 0) - (difficulty >= 5 ? 5 : 0));
                            if (difficulty >= 4) fight.speed *= 2;
                            if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.FightTutorial))
                                TutorialManager.Start(TutorialManager.FightSteps(), waitForRelease: true);
                        }
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
                if (TutorialManager.Draw(state, mouseState))
                    UnlockManager.UnlockUnlock(UnlockManager.UnlockType.MapTutorial);
            }
        }
    }
}
