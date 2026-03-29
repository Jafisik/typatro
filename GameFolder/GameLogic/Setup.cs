using System.Collections.Generic;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.Services;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;
using static typatro.GameFolder.Services.UnlockManager;

namespace typatro.GameFolder
{
    public partial class GameLogic
    {
        private void LoadGame()
        {
            seed = gameSaveData.seed;
            map = new Map();
            enhancements = new Enhancements();
            enhancements.letters = gameSaveData.letterScores;
            enhancements.damageResist = gameSaveData.enhancements[0];
            enhancements.mistakeBlock = gameSaveData.enhancements[1];
            enhancements.shinyChance = gameSaveData.enhChances[0];
            enhancements.stoneChance = gameSaveData.enhChances[1];
            enhancements.bloomChance = gameSaveData.enhChances[2];
            enhancements.streakMult = gameSaveData.enhChances[3];
            shop = new Shop(enhancements);
            treasure = new Treasure(enhancements);
            curseRoom = new CurseRoom(enhancements);
            coins = gameSaveData.coins;
            level = gameSaveData.level;
            selectedRune = gameSaveData.rune;
            difficulty = gameSaveData.difficulty;

            SetContext(-1, 0);
            map.GenerateNodes();

            map.NodeVisit(gameSaveData.visitedNodes);
            visitedNodes = gameSaveData.visitedNodes;
            selectedNode = map.GetNodeFromPos(gameSaveData.mapNode[0], gameSaveData.mapNode[1]);
            lastSelectedNode = selectedNode;
            mousePressed = true;
            foreach (int glyph in gameSaveData.glyphs)
                GlyphManager.Add((Glyph)glyph);
        }

        private void NewGame()
        {
            level = 1;
            seed = UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.MapTutorial) ? unseededRandom.Next() : 10;
            SetContext(-1, 0);
            map = new Map();
            map.GenerateNodes();
            selectedNode = map.GetFirstNode();
            lastSelectedNode = selectedNode;
            enhancements = new Enhancements();
            shop = new Shop(enhancements);
            treasure = new Treasure(enhancements);
            curseRoom = new CurseRoom(enhancements);
            coins = difficulty >= 1 ? 15 : startCoins;
            if (difficulty >= 3) enhancements.streakMult -= 1;
            GlyphManager.RemoveAllGlyphs();
            GlyphManager.Add(Glyph.NoGlyphsLeft);
            visitedNodes = new List<int[]>();
            mistake = false;
            deadCounted = false;
            mousePressed = true;
            tutorial = false;
            mapTutorialStarted = false;
            gameState = GameState.RUNES;
        }

        private void Reset()
        {
            EnemyManager.ClearActive();
            jumpscareActive = false;
            jumpscareNextTime = -1;
            molochActive = false;
            kHeperShieldActive = false;
            wendigoBugs.Clear();
            textRotation = 0;
            enhancements.ResetChange();
            isFightFinished = false;
            afterFightScreen = false;
            wordStreak = 1;
            inventoryGlyphSelect = 1;
            afterFightSelect = 0;
            pitch = 0;
            prevMistakes = 0;
            cards.Clear();
            shinyWords.Clear();
            stoneWords.Clear();
            scoreCalculator.Reset();
        }
    }
}