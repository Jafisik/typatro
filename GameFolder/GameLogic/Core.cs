using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using typatro.GameFolder.Logic;
using typatro.GameFolder.Models;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.Services;
using typatro.GameFolder.UI;
using typatro.GameFolder.Upgrades;

namespace typatro.GameFolder
{
    public partial class GameLogic
    {
        public enum GameState
        {
            LOADGAME,
            NEWGAME,
            OPTIONS,
            EXIT,
            DEBUG,
            MENU,
            RUNES,
        }

        public GameState gameState = GameState.MENU;

        // UI / window
        readonly Menu menu;
        public double timeInSeconds = 0, lastTime = -1;
        long totalGameTimeMinutes = 0;
        Point windowPos, dragOffset;
        bool eyeOfHorusActive, isDragging, gameFinished, deadCounted, introPlayed;
        public bool mistake, anubisActive, mousePressed, tutorial;
        MainGame.SoundEffects sfx;

        // Player
        public Enhancements enhancements;
        public long coins = 0, startCoins = 30;
        public int selectedRune = 0, difficulty = 0, inventoryGlyphSelect = 1;
        public bool dead, inventoryMousePressed, inventoryMove = true;
        KeyboardState prevKBState;
        public static bool keyboardUsed, windowActive;
        Point mousePosition = new Point();
        CharacterSelect characterSelect;
        GameUi gameUi;

        // Score
        public double timeSinceLastWord = 0, wordStreak = 1;
        ScoreCalculator scoreCalculator;

        // Final stats
        public long totalScore, maxScore, lettersWritten, mistakesWritten, wordsWritten;
        public long highestStreak, coinsGained, maxCoins;

        // Rooms
        Fight fight;
        public bool canStartFight, startedTyping, roomSelected, isFightFinished;
        bool afterFightScreen, afterFightMove;
        int afterFightSelect = 0;
        List<LetterUpgrade> cards = new List<LetterUpgrade>();
        Treasure treasure;
        Shop shop;
        CurseRoom curseRoom;

        // Save
        public GameSaveData gameSaveData;
        public static int seed;
        public static Random contextRandom = new Random();
        public static Random unseededRandom = new Random();

        // Writer / typing
        readonly Writer writer;
        public string neededText;
        readonly List<string> jsonStrings;
        int xTextOffset = 0, yTextOffset = 0;
        public List<int> shinyWords = new List<int>(), stoneWords = new List<int>(), bloomWords = new List<int>();
        double textRotation = 0;
        Vector2 catPos = Vector2.One;

        // Enemy
        public Models.Enemy currentEnemy;
        public bool isDebugFight;
        bool jumpscareActive, molochActive;
        public bool kHeperShieldActive;
        public bool polemanRespawned;
        float ictusFlashTimer = -1f, ictusFlashActive;
        double jumpscareEndTime, jumpscareNextTime = -1;
        public List<(Vector2 pos, Vector2 vel)> wendigoBugs = new();

        // Enemy intro: shown centered/big at the start of a fight, then slides down into its
        // usual corner spot. Outside the tutorial this plays for every fight; inside the
        // tutorial only the boss gets it (the earlier fights are teaching the UI itself, so
        // the corner position needs to already be settled and the banner/scorebar visible).
        bool enemyIntroActive;
        double enemyIntroTimer;
        // Enter can also skip the intro, but only once it's been seen released at least once
        // since the intro started - otherwise the same held-down Enter used to confirm the
        // fight's map node would instantly skip it before the player ever let go.
        bool introEnterReady;

        // Safety net for tutorial fights - losing one doesn't kill the run (which would wipe
        // the save and eject a brand new player mid-tutorial, since MapTutorial unlocks the
        // moment the map bubble is dismissed, well before the first fight). It just resets the
        // same fight for another attempt, with a brief reassuring message.
        double tutorialRetryMsgUntil = -10;

        // Short victory hold once the enemy is defeated, before cutting to the reward screen -
        // otherwise the win was instant and jarring, straight from typing to Reward.
        bool fightWinPending;
        double fightWinPauseUntil = -10;

        // Map
        Map map;
        List<int[]> visitedNodes = new List<int[]>();
        MapNode selectedNode, lastSelectedNode;
        public int level = 1;
        bool firstEnter = true;
        public bool inventoryUp;

        public GameLogic(List<string> jsonStrings, Point windowPos, MainGame.SoundEffects sfx)
        {
            this.jsonStrings = jsonStrings;
            this.windowPos = windowPos;
            this.sfx = sfx;

            menu = new Menu();
            gameSaveData = SaveManager.LoadGame();
            
            enhancements = new Enhancements();

            writer = new Writer();
            neededText = RandomTextGenerate(10);

            treasure = new Treasure(enhancements);
            shop = new Shop(enhancements);
            curseRoom = new CurseRoom(enhancements);
            GlyphManager.SetUnlockedGlyphs();
            characterSelect = new CharacterSelect(this);
            gameUi = new GameUi(this);
            scoreCalculator = new ScoreCalculator(this);
        }
    }
}