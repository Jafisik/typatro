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
                bool choiceTutorialPending = selectedNode.forward.Count > 1 && !UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.MapChoiceTutorial);
                if (state.IsKeyUp(Keys.Tab) && !inventoryUp && UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.MapTutorial) && tutorial && !choiceTutorialPending && !TutorialManager.IsShowing())
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
                                fight = Fight.Create(fightDifficulty, level, newNode.column, newNode.row);
                                // Tutorial map: fixed enemies instead of random ones, so the
                                // same curated fight shows up for every player - Oculus first,
                                // then Apnea (normal) / Kudlanka (elite) at the branch.
                                if (map.isTutorialMap && newNode.type == NodeType.FIGHT && newNode.column == 1)
                                {
                                    currentEnemy = Services.EnemyManager.Get(Services.EnemyType.O);
                                }
                                else if (map.isTutorialMap && newNode.type == NodeType.FIGHT && newNode.column == 2)
                                {
                                    currentEnemy = Services.EnemyManager.Get(Services.EnemyType.A);
                                }
                                else if (map.isTutorialMap && newNode.type == NodeType.ELITE)
                                {
                                    currentEnemy = Services.EnemyManager.Get(Services.EnemyType.K);
                                }
                                else if (newNode.type == NodeType.FIGHT)
                                {
                                    currentEnemy = Services.EnemyManager.Normal[contextRandom.Next(Services.EnemyManager.Normal.Length)];
                                }
                                else if (newNode.type == NodeType.ELITE)
                                {
                                    currentEnemy = Services.EnemyManager.Elite[contextRandom.Next(Services.EnemyManager.Elite.Length)];
                                }
                                else
                                {
                                    currentEnemy = Services.EnemyManager.Boss[contextRandom.Next(Services.EnemyManager.Boss.Length)];
                                }
                                Services.EnemyManager.SetActive(currentEnemy.Type);
                                enemyIntroActive = !map.isTutorialMap || newNode.type == NodeType.BOSS;
                                enemyIntroTimer = 0;
                                introEnterReady = false;
                                if (enemyIntroActive) sfx.enemyIntro.Play((float)SaveManager.volume / 10, 0f, 0f);
                                break;
                            case NodeType.TREASURE:
                                treasure.NewGlyph(map.isTutorialMap);
                                if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.TreasureTutorial))
                                    TutorialManager.Start(TutorialManager.TreasureSteps());
                                break;
                            case NodeType.SHOP:
                                shop.NewShop();
                                if (GlyphManager.IsActive(Glyph.Life))
                                {
                                    enhancements.AddLetterScore((char)(contextRandom.Next(0, 26) + 'a'), 5);
                                }
                                if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.ShopTutorial))
                                    TutorialManager.Start(TutorialManager.ShopSteps());
                                break;
                            case NodeType.CURSE:
                                curseRoom.NewCurse(map.isTutorialMap);
                                if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.CurseTutorial))
                                    TutorialManager.Start(TutorialManager.CurseSteps());
                                break;
                        }
                        if (IsFight(newNode.type))
                        {
                            // No shiny/stone/bloom words yet in the tutorial's very first fight -
                            // SpecialWordsSteps introduces them at the second fight instead.
                            bool allowSpecialWords = UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.FightTutorial);
                            neededText = RandomTextGenerate(fight.words + (GlyphManager.IsActive(Glyph.Papyrus) ? 20 : 0) - (difficulty >= 5 ? 5 : 0), allowSpecialWords);
                            if (difficulty >= 4) fight.speed *= 2;
                            if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.FightTutorial))
                                TutorialManager.Start(TutorialManager.FightSteps(), waitForRelease: true);
                            else if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.SpecialWordsTutorial))
                                TutorialManager.Start(TutorialManager.SpecialWordsSteps(), waitForRelease: true);
                        }
                        Writer.writtenText.Clear();
                        Writer.diffIndexes.Clear();
                        startedTyping = false;
                        lastSelectedNode = selectedNode;
                        selectedNode = newNode;
                        roomSelected = true;
                    }
                }
            }
            if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.MapTutorial))
            {
                if (!TutorialManager.IsShowing())
                    TutorialManager.Start(TutorialManager.MapSteps(map.GetFirstNode().forward[0].point));
                if (TutorialManager.Draw(state, mouseState))
                {
                    UnlockManager.UnlockUnlock(UnlockManager.UnlockType.MapTutorial);
                    map.ResetEnterGate();
                }
            }
            else if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.MapChoiceTutorial) && selectedNode.forward.Count > 1)
            {
                if (!TutorialManager.IsShowing())
                    TutorialManager.Start(TutorialManager.MapChoiceSteps(selectedNode.forward[0].point, selectedNode.forward[selectedNode.forward.Count - 1].point));
                if (TutorialManager.Draw(state, mouseState))
                {
                    UnlockManager.UnlockUnlock(UnlockManager.UnlockType.MapChoiceTutorial);
                    map.ResetEnterGate();
                }
            }
            // Fires back on the map the moment the player has been through the Treasure
            // tutorial - by then they've actually picked something up worth checking.
            else if (!UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.InventoryTutorial) && UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.TreasureTutorial))
            {
                if (!TutorialManager.IsShowing())
                    TutorialManager.Start(TutorialManager.InventorySteps());
                if (TutorialManager.Draw(state, mouseState))
                {
                    UnlockManager.UnlockUnlock(UnlockManager.UnlockType.InventoryTutorial);
                    map.ResetEnterGate();
                }
            }
        }
    }
}
