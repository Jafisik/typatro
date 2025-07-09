using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.Rooms;
using typatro.GameFolder.UI;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace typatro.GameFolder{
    public enum NodeType{
        FIGHT,
        ELITE,
        TREASURE,
        SHOP,
        CURSE,
        RANDOM,
        BOSS,
        NOTHING
    }
    public class MapNode{
        public Vector2 point;
        public List<MapNode> forward;
        public NodeType type;
        public int row, column;
        public bool visited = false;
        public MapNode(List<MapNode> forward, NodeType type, Vector2 point, int row, int column){
            this.forward = forward;
            this.type = type;
            this.point = point;
            this.row = row;
            this.column = column;
        }

        public int[] NodePos(){
            return new int[] { row, column };
        }
    }
    class Map{
        readonly int rowSpacing = 75, columnSpacing = 55, randomChange = 12;
        readonly int topOffset = 80, leftOffset = 40;
        readonly int paths = 4;
        
        MapNode[,] mapNodes;
        int nodeSelectIndex = 0;
        bool nodeMove = true, enterUp = true;
        

        public Map(){
            columnSpacing = MainGame.screenHeight/10;
            rowSpacing = MainGame.screenWidth/14+2;
        }

        public MapNode GetFirstNode(){
            return mapNodes[0,0];
        }

        public MapNode GetNodeFromPos(int row, int column){
            return mapNodes[row, column];
        }

        public void NodeVisit(List<int[]> visitedList){
            foreach(int[] nodePos in visitedList){
                if(nodePos[0] != 0){
                    mapNodes[nodePos[1],nodePos[0]].visited = true;
                }
            }
        }

        public void GenerateNodes()
        {
            if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("GenerateNodes",""));
            mapNodes = new MapNode[8, 13];
            int pos = GameLogic.seededRandom.Next(0, mapNodes.GetLength(0));

            //Generates first node and then makes a path by going to i+1 or i or i-1 node
            for (int path = 0; path < paths; path++)
            {
                while (mapNodes[pos, 1] != null)
                {
                    pos = GameLogic.seededRandom.Next(0, mapNodes.GetLength(0));
                }

                mapNodes[pos, 1] = new MapNode(new List<MapNode>(), NodeType.FIGHT,
                    new Vector2(leftOffset, GameLogic.seededRandom.Next(-randomChange, randomChange) + topOffset + pos * columnSpacing), pos, 1);

                for (int column = 2; column < mapNodes.GetLength(1) - 1; column++)
                {
                    pos += GameLogic.seededRandom.Next(-1, 2);
                    pos = Math.Clamp(pos, 0, mapNodes.GetLength(0) - 1);

                    if (mapNodes[pos, column] == null)
                    {
                        NodeType nodeType;
                        if (column == 7)
                        {
                            nodeType = NodeType.TREASURE;
                        }
                        else if (column == 11)
                        {
                            nodeType = NodeType.SHOP;
                        }
                        else
                        {
                            nodeType = GenerateNodeType();
                        }

                        mapNodes[pos, column] = new MapNode(new List<MapNode>(), nodeType,
                            new Vector2(GameLogic.seededRandom.Next(-randomChange, randomChange) + leftOffset + column * rowSpacing,
                                        GameLogic.seededRandom.Next(-randomChange, randomChange) + topOffset + pos * columnSpacing), pos, column);
                    }
                }
            }
            //Boss node generation
            mapNodes[0, mapNodes.GetLength(1)-1] = new MapNode(new List<MapNode>(), NodeType.BOSS,
                    new Vector2(leftOffset + (mapNodes.GetLength(1)-1) * rowSpacing, mapNodes.GetLength(0)/2*columnSpacing), 0, mapNodes.GetLength(1)-1);

            //First node for traversal generation
            List<MapNode> firstRow = new List<MapNode>();
            for(int node = 0; node < mapNodes.GetLength(0); node++){
                if(mapNodes[node,1] != null){
                    firstRow.Add(mapNodes[node,1]);
                }
            }
            mapNodes[0,0] = new MapNode(firstRow, NodeType.NOTHING, new Vector2(), 0, 0);

            //Connects to previous nodes
            for (int column = 1; column < mapNodes.GetLength(1)-1; column++){
                for (int row = 0; row < mapNodes.GetLength(0); row++){
                    MapNode currentNode = mapNodes[row, column];

                    if (currentNode != null){
                        if (row - 1 >= 0 && mapNodes[row - 1, column + 1] != null)
                            currentNode.forward.Add(mapNodes[row - 1, column + 1]);

                        if (mapNodes[row, column + 1] != null)
                            currentNode.forward.Add(mapNodes[row, column + 1]);

                        if (row + 1 < mapNodes.GetLength(0) && mapNodes[row + 1, column + 1] != null)
                            currentNode.forward.Add(mapNodes[row + 1, column + 1]);
                    }
                }
            }
            for(int node = 0; node < mapNodes.GetLength(0); node++){
                if(mapNodes[node,mapNodes.GetLength(1)-2] != null){
                    mapNodes[node,mapNodes.GetLength(1)-2].forward.Add(mapNodes[0, mapNodes.GetLength(1)-1]);
                }
            }
            
        }

        public void DrawNodes()
        {
            for (int i = 0; i < mapNodes.GetLength(0); i++){
                for (int j = 1; j < mapNodes.GetLength(1); j++){
                    MapNode node = mapNodes[i, j];

                    if (node != null)
                    {
                        foreach (MapNode next in node.forward)
                        {
                            if (next != null)
                            {
                                DrawDottedPath(node.point, next.point);
                            }
                        }

                        MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, MapIcon(node.type), node.point, ThemeColors.Text);
                        if (node.visited && node.type != NodeType.NOTHING)
                        {
                            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle((int)node.point.X - 7, (int)node.point.Y - 7, 38, 48), ThemeColors.ExitShop);
                        }
                    }
                }
            }
            string info = "F - Fight   E - Elite fight   C - Curse  $ - Shop   X - Treasure   B - Boss";
            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallMapFont, info, new Vector2(MainGame.screenWidth/2-MainGame.Gfx.smallMapFont.MeasureString(info).X/2,MainGame.screenHeight-40), ThemeColors.Text);
        }

        private void DrawDottedPath(Vector2 start, Vector2 end)
        {
            int dotSpacing = 15;
            Vector2 direction = Vector2.Normalize(end - start);
            float distance = Vector2.Distance(start, end);
            
            for (float d = dotSpacing; d < distance-dotSpacing; d += dotSpacing){
                Vector2 dotPosition = start + direction * d;
                MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.menuFont, ".", dotPosition, ThemeColors.NotSelected);
            }
        }

        public NodeType GenerateNodeType(){
            if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("GenerateNodeType",""));
            int roll = GameLogic.seededRandom.Next(0,101);
            if(roll < 40) return NodeType.FIGHT;
            if(roll < 60) return NodeType.RANDOM;
            if(roll < 88) return NodeType.ELITE;
            if(roll < 94) return NodeType.SHOP;
            if(roll < 99) return NodeType.CURSE;

            return NodeType.TREASURE;
        }

        public NodeType GenerateNodeTypeFromRandom(){
            if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("GenerateNodeTypeFromRandom",""));
            int roll = GameLogic.seededRandom.Next(0, 101);
            if (roll < 50) return NodeType.FIGHT;
            if (roll < 80) return NodeType.ELITE;
            if (roll < 90) return NodeType.SHOP;
            if (roll < 99) return NodeType.CURSE;
            
            return NodeType.TREASURE;
        }

        public MapNode NodeSelect(MapNode node, ref bool mousePressed, int column, int level){
            MouseState mouseState = Mouse.GetState();
            if(mouseState.LeftButton == ButtonState.Released)
            {
                mousePressed = false;
            }
            int forwardCount = node.forward.Count;
            if(node.type != NodeType.NOTHING){
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle((int)node.point.X-7, (int)node.point.Y-7, 38, 48), ThemeColors.ExitShop);
            }
            if (forwardCount > 0)
            {
                KeyboardState state = Keyboard.GetState();
                if (nodeMove)
                {
                    if (state.IsKeyDown(Keys.Down) && nodeSelectIndex != forwardCount - 1) nodeSelectIndex++;
                    else if (state.IsKeyDown(Keys.Up) && nodeSelectIndex != 0) nodeSelectIndex--;
                    nodeMove = false;
                }

                if (state.IsKeyUp(Keys.Up) && state.IsKeyUp(Keys.Down))
                {
                    nodeMove = true;
                }

                int nodeIndex = 0;
                MapNode selectedNode;
                foreach (MapNode fwdNode in node.forward)
                {
                    Rectangle nodeRect = new Rectangle((int)fwdNode.point.X - 7, (int)fwdNode.point.Y - 7, 38, 48);
                    if (!mousePressed && nodeRect.Contains(mouseState.Position) && !GameLogic.keyboardUsed)
                    {
                        nodeSelectIndex = nodeIndex;
                        selectedNode = node.forward[nodeSelectIndex];
                        if (mouseState.LeftButton == ButtonState.Pressed)
                        {
                            enterUp = false;
                            nodeSelectIndex = 0;
                            selectedNode.visited = true;
                            mousePressed = true;
                            return selectedNode;
                        }
                    }

                    if (nodeSelectIndex == nodeIndex)
                    {
                        Vector2 nodePoint = node.forward[nodeSelectIndex].point;
                        MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle((int)nodePoint.X - 7, (int)nodePoint.Y - 7, 38, 48), ThemeColors.Selected);

                        NodeType type = node.forward[nodeSelectIndex].type;
                        Color infoBg = ThemeColors.ExitShop;
                        infoBg.A = 220;
                        Rectangle infoRect;
                        if (column <= (SaveManager.size > 0 ? 8 : 5)) infoRect = new Rectangle((int)nodePoint.X + 50, (int)nodePoint.Y - 30, 200, 100);
                        else infoRect = new Rectangle((int)nodePoint.X - 230, (int)nodePoint.Y - 30, 200, 100);

                        if (GameLogic.IsFight(type))
                        {
                            int difficulty;
                            string letters;
                            switch (node.forward[nodeSelectIndex].type)
                            {
                                case NodeType.FIGHT:
                                    difficulty = 1;
                                    letters = "+ 1-3";
                                    break;
                                case NodeType.ELITE:
                                    difficulty = 2;
                                    letters = "+ 3-5";
                                    break;
                                case NodeType.BOSS:
                                    difficulty = 3;
                                    letters = "* 2-3";
                                    break;
                                default:
                                    difficulty = 1;
                                    letters = "";
                                    break;
                            }

                            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, infoRect, infoBg);
                            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallMapFont,
                                $"Reward: {Fight.CashGainGen(level, column, difficulty)} coins\nLetter: {letters}\nLength: {Fight.WordsGen(difficulty)} words\nDamage: {Math.Max(1, Fight.SpeedGen(level, column, difficulty) + 1)}/s",
                                new Vector2(infoRect.X + 10, infoRect.Y + 10), ThemeColors.Text);
                        }
                        else if (type == NodeType.SHOP)
                        {
                            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, infoRect, infoBg);
                            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallMapFont, "Shop\n\nUse your coins to\nupgrade yourself", new Vector2(infoRect.X + 10, infoRect.Y + 10), ThemeColors.Text);
                        }
                        else if (type == NodeType.TREASURE)
                        {
                            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, infoRect, infoBg);
                            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallMapFont, "Treasure\n\nHarness the power\nof hieroglyphs", new Vector2(infoRect.X + 10, infoRect.Y + 10), ThemeColors.Text);
                        }
                        else if (type == NodeType.CURSE)
                        {
                            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, infoRect, infoBg);
                            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallMapFont, "Curse\n\nEmbrace pain\ngain an advantage", new Vector2(infoRect.X + 10, infoRect.Y + 10), ThemeColors.Text);
                        }
                        else if (type == NodeType.RANDOM)
                        {
                            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, infoRect, infoBg);
                            MainGame.Gfx.spriteBatch.DrawString(MainGame.Gfx.smallMapFont, "Random\n\nLet the chaos\ndecide your faith", new Vector2(infoRect.X + 10, infoRect.Y + 10), ThemeColors.Text);
                        }
                    }
                    
                    else
                    {
                        MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, nodeRect, ThemeColors.NotSelected);
                    }

                    nodeIndex++;
                }

                selectedNode = node.forward[nodeSelectIndex];
                if (state.IsKeyDown(Keys.Enter) && enterUp)
                {
                    enterUp = false;
                    nodeSelectIndex = 0;
                    selectedNode.visited = true;
                    return selectedNode;
                }
                else if (state.IsKeyUp(Keys.Enter)) enterUp = true;


            }
            
            
            return node;
        }

        private static string MapIcon(NodeType type){
            return type switch
            {
                NodeType.FIGHT => "F",
                NodeType.ELITE => "E",
                NodeType.BOSS => "B",
                NodeType.RANDOM => "?",
                NodeType.SHOP => "$",
                NodeType.TREASURE => "X",
                NodeType.CURSE => "C",
                _ => "A",
            };
        }
    }
}