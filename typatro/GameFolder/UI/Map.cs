using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.UI;
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

                bool hasShop = false;
                int shopColumn = GameLogic.seededRandom.Next(2, mapNodes.GetLength(1) - 2);

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
                        else if (!hasShop && column == shopColumn)
                        {
                            nodeType = NodeType.SHOP;
                            hasShop = true;
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

                if (!hasShop)
                {
                    for (int column = 2; column < mapNodes.GetLength(1) - 2; column++)
                    {
                        for (int row = 0; row < mapNodes.GetLength(0); row++)
                        {
                            if (mapNodes[row, column] != null && mapNodes[row, column].type == NodeType.FIGHT)
                            {
                                mapNodes[row, column].type = NodeType.SHOP;
                                hasShop = true;
                                break;
                            }
                        }
                        if (hasShop) break;
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
                            MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle((int)node.point.X - 8, (int)node.point.Y - 8, 40, 50), ThemeColors.ExitShop);
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
            if(roll < 70) return NodeType.ELITE;
            if (roll < 93) return NodeType.RANDOM;
            if (roll < 99) return NodeType.CURSE;

            return NodeType.TREASURE;
        }

        public NodeType GenerateNodeTypeFromRandom(){
            if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("GenerateNodeTypeFromRandom",""));
            int roll = GameLogic.seededRandom.Next(0, 101);
            if (roll < 50) return NodeType.FIGHT;
            if (roll < 75) return NodeType.ELITE;
            if (roll < 90) return NodeType.SHOP;
            if (roll < 99) return NodeType.CURSE;
            
            return NodeType.TREASURE;
        }

        public MapNode NodeSelect(MapNode node, ref bool mousePressed){
            MouseState mouseState = Mouse.GetState();
            if(mouseState.LeftButton == ButtonState.Released)
            {
                mousePressed = false;
            }
            int forwardCount = node.forward.Count;
            if(forwardCount > 0){KeyboardState state = Keyboard.GetState();
                if (nodeMove){
                    if (state.IsKeyDown(Keys.Down) && nodeSelectIndex != forwardCount-1) nodeSelectIndex++;
                    else if (state.IsKeyDown(Keys.Up) && nodeSelectIndex != 0) nodeSelectIndex--;
                    nodeMove = false;
                }

                if (state.IsKeyUp(Keys.Up) && state.IsKeyUp(Keys.Down)){
                    nodeMove = true;
                }

                int nodeIndex = 0;
                MapNode selectedNode;
                foreach (MapNode fwdNode in node.forward){
                    Rectangle nodeRect = new Rectangle((int)fwdNode.point.X - 8, (int)fwdNode.point.Y - 8, 40, 50);
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
                        MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle((int)node.forward[nodeSelectIndex].point.X - 8, (int)node.forward[nodeSelectIndex].point.Y - 8, 40, 50), ThemeColors.Selected);
                    }
                    else
                    {
                        MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, nodeRect, ThemeColors.NotSelected);
                    }
                    
                    nodeIndex++;
                }

                selectedNode = node.forward[nodeSelectIndex];
                if(state.IsKeyDown(Keys.Enter) && enterUp){
                    enterUp = false;
                    nodeSelectIndex = 0;
                    selectedNode.visited = true;
                    return selectedNode;
                }
                else if(state.IsKeyUp(Keys.Enter)) enterUp = true;
                
                
            }
            if(node.type != NodeType.NOTHING){
                MainGame.Gfx.spriteBatch.Draw(MainGame.Gfx.texture, new Rectangle((int)node.point.X-8, (int)node.point.Y-8, 35, 50), ThemeColors.ExitShop);
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