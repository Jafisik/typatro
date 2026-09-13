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
            if (seed == 10) map.GenerateTutorialNodes();
            else map.GenerateNodes();

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
            bool firstRun = !UnlockManager.IsUnlockUnlocked(UnlockManager.UnlockType.MapTutorial);
            seed = firstRun ? 10 : unseededRandom.Next();
            SetContext(-1, 0);
            map = new Map();
            if (firstRun) map.GenerateTutorialNodes();
            else map.GenerateNodes();
            selectedNode = map.GetFirstNode();
            lastSelectedNode = selectedNode;
            enhancements = new Enhancements();
            shop = new Shop(enhancements);
            treasure = new Treasure(enhancements);
            curseRoom = new CurseRoom(enhancements);
            if (firstRun)
            {
                // Skip character/rune select on the very first run - start it with Uruz
                // (rune 0, easiest difficulty), same bonus CharacterSelect would apply.
                // The player reaches the real character select (and its tutorial) for the
                // first time right after beating the tutorial boss - see
                // DrawTutorialCompleteScreen, which calls back into NewGame() once MapTutorial
                // is already unlocked, so this branch won't run and RUNES won't be skipped.
                selectedRune = 0;
                difficulty = 0;
                enhancements.AllLettersAddScore(1);
            }
            coins = difficulty >= 1 ? 15 : startCoins;
            if (difficulty >= 3) enhancements.streakMult -= 1;
            GlyphManager.RemoveAllGlyphs();
            GlyphManager.Add(Glyph.NoGlyphsLeft);
            visitedNodes = new List<int[]>();
            mistake = false;
            deadCounted = false;
            mousePressed = true;
            tutorial = false;
            gameState = firstRun ? GameState.LOADGAME : GameState.RUNES;
        }

        private void Reset()
        {
            EnemyManager.ClearActive();
            jumpscareActive = false;
            jumpscareNextTime = -1;
            molochActive = false;
            kHeperShieldActive = false;
            polemanRespawned = false;
            ictusFlashTimer = -1f;
            ictusFlashActive = 0f;
            wendigoBugs.Clear();
            textRotation = 0;
            enhancements.ResetChange();
            isFightFinished = false;
            afterFightScreen = false;
            fightWinPending = false;
            wordStreak = 1;
            inventoryGlyphSelect = 1;
            afterFightSelect = 0;
            pitch = 0;
            prevMistakes = 0;
            cards.Clear();
            shinyWords.Clear();
            stoneWords.Clear();
            bloomWords.Clear();
            scoreCalculator.Reset();
        }
    }
}