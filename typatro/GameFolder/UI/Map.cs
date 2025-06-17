using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using typatro.GameFolder.UI;

namespace typatro.GameFolder{
    public enum NodeType{
        FIGHT,
        ELITE,
        TREASURE,
        SHOP,
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
        MainGame.Gfx gfx;
        readonly int rowSpacing = 75, columnSpacing = 55, randomChange = 12;
        readonly int topOffset = 80, leftOffset = 40;
        readonly int paths = 4;
        
        MapNode[,] mapNodes;
        int nodeSelectIndex = 0;
        bool nodeMove = true, enterUp = true;
        

        public Map(MainGame.Gfx gfx){
            this.gfx = gfx;
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
                    Console.WriteLine(nodePos[0] +" , "+ nodePos[1]);
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
            for (int path = 0; path < paths; path++){
                while (mapNodes[pos, 1] != null){
                    pos = GameLogic.seededRandom.Next(0, mapNodes.GetLength(0));
                }

                mapNodes[pos, 1] = new MapNode(new List<MapNode>(), NodeType.FIGHT,
                    new Vector2(leftOffset, GameLogic.seededRandom.Next(-randomChange, randomChange) + topOffset + pos * columnSpacing), pos, 1);

                for (int column = 2; column < mapNodes.GetLength(1)-1; column++){
                    pos += GameLogic.seededRandom.Next(-1, 2);

                    if (pos >= mapNodes.GetLength(0)) pos = mapNodes.GetLength(0) - 1;
                    if (pos < 0) pos = 0;

                    if (mapNodes[pos, column] == null){

                        mapNodes[pos, column] = new MapNode(new List<MapNode>(), column == 7?NodeType.TREASURE:GenerateNodeType(),
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

                    if (node != null){
                        foreach (MapNode next in node.forward){
                            if (next != null){
                                DrawDottedPath(node.point, next.point);
                            }
                        }

                        gfx.spriteBatch.DrawString(gfx.menuFont, MapIcon(node.type), node.point, ThemeColors.Text);
                        if (node.visited && node.type != NodeType.NOTHING)
                        {
                            gfx.spriteBatch.Draw(gfx.texture, new Microsoft.Xna.Framework.Rectangle((int)node.point.X - 5, (int)node.point.Y - 5, 30, 40), ThemeColors.Selected);
                        }
                    }
                }
            }
            string info = "F - Fight   E - Elite fight   ? - Random  $ - Shop   X - Treasure   B - Boss";
            gfx.spriteBatch.DrawString(gfx.smallTextFont, info, new Vector2(MainGame.screenWidth/2-gfx.smallTextFont.MeasureString(info).X/2,MainGame.screenHeight-40), ThemeColors.Text);
        }

        private void DrawDottedPath(Vector2 start, Vector2 end)
        {
            int dotSpacing = 15;
            Vector2 direction = Vector2.Normalize(end - start);
            float distance = Vector2.Distance(start, end);
            
            for (float d = dotSpacing; d < distance-dotSpacing; d += dotSpacing){
                Vector2 dotPosition = start + direction * d;
                gfx.spriteBatch.DrawString(gfx.menuFont, ".", dotPosition, ThemeColors.NotSelected);
            }
        }

        public NodeType GenerateNodeType(){
            if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("GenerateNodeType",""));
            int roll = GameLogic.seededRandom.Next(0,101);
            if(roll < 50) return NodeType.FIGHT;
            if(roll < 80) return NodeType.RANDOM;
            if(roll < 87) return NodeType.SHOP;
            if(roll < 98) return NodeType.ELITE;
            return NodeType.TREASURE;
        }

        public NodeType GenerateNodeTypeFromRandom(){
            if(!GameLogic.isReplay) GameLogic.actions.Add(new UserAction("GenerateNodeTypeFromRandom",""));
            int roll = GameLogic.seededRandom.Next(0, 101);
            if (roll < 75) return NodeType.FIGHT;
            if (roll < 85) return NodeType.SHOP;
            if (roll < 98) return NodeType.ELITE;
            return NodeType.TREASURE;
        }

        public MapNode NodeSelect(MapNode node){
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

                foreach(MapNode fwdNode in node.forward){
                    gfx.spriteBatch.Draw(gfx.texture, new Microsoft.Xna.Framework.Rectangle((int)fwdNode.point.X-5, (int)fwdNode.point.Y-5, 30, 40), ThemeColors.Background);
                    gfx.spriteBatch.Draw(gfx.texture, new Microsoft.Xna.Framework.Rectangle((int)fwdNode.point.X-5, (int)fwdNode.point.Y-5, 30, 40), ThemeColors.Background);
                }
                
                MapNode selectedNode = node.forward[nodeSelectIndex];
                if(state.IsKeyDown(Keys.Enter) && enterUp){
                    enterUp = false;
                    nodeSelectIndex = 0;
                    selectedNode.visited = true;
                    return selectedNode;
                }
                else if(state.IsKeyUp(Keys.Enter)) enterUp = true;
                
                gfx.spriteBatch.Draw(gfx.texture, new Microsoft.Xna.Framework.Rectangle((int)selectedNode.point.X-5, (int)selectedNode.point.Y-5, 30, 40), ThemeColors.NotSelected);
            }
            if(node.type != NodeType.NOTHING){
                gfx.spriteBatch.Draw(gfx.texture, new Microsoft.Xna.Framework.Rectangle((int)node.point.X-5, (int)node.point.Y-5, 30, 40), ThemeColors.NotSelected);
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
                _ => "A",
            };
        }
    }
}