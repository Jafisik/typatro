using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace typatro.GameFolder{
    enum NodeType{
        FIGHT,
        ELITE,
        TREASURE,
        SHOP,
        RANDOM,
        BOSS,
        NOTHING
    }
    class MapNode{
        public Vector2 point;
        public List<MapNode> forward;
        public NodeType type;
        public MapNode(List<MapNode> forward, NodeType type, Vector2 point){
            this.forward = forward;
            this.type = type;
            this.point = point;
        }
    }
    class Map{
        readonly SpriteBatch spriteBatch;
        readonly SpriteFont bigFont;
        readonly SpriteFont smallFont;
        readonly int rowSpacing = 75, columnSpacing = 60, randomChange = 12;
        readonly int topOffset = 50, leftOffset = 40;
        readonly int paths = 4;
        readonly Random random = new Random();
        readonly Texture2D texture;
        
        MapNode[,] mapNodes;
        int nodeSelectIndex = 0;
        bool nodeMove = true, enterUp = true;
        

        public Map(SpriteBatch spriteBatch, SpriteFont bigFont, SpriteFont smallFont, Texture2D texture){
            this.spriteBatch = spriteBatch;
            this.bigFont = bigFont;
            this.smallFont = smallFont;
            this.texture = texture;

            GenerateNodes();
        }

        public MapNode GetFirstNode(){
            return mapNodes[0,0];
        }

        public void GenerateNodes()
        {
            mapNodes = new MapNode[8, 13];
            int pos = random.Next(0, mapNodes.GetLength(0));

            //Generates first node and then makes a path by going to i+1 or i or i-1 node
            for (int path = 0; path < paths; path++){
                while (mapNodes[pos, 1] != null){
                    pos = random.Next(0, mapNodes.GetLength(0));
                }

                mapNodes[pos, 1] = new MapNode(new List<MapNode>(), NodeType.FIGHT,
                    new Vector2(leftOffset, random.Next(-randomChange, randomChange) + topOffset + pos * columnSpacing));

                for (int column = 2; column < mapNodes.GetLength(1)-1; column++){
                    pos += random.Next(-1, 2);

                    if (pos >= mapNodes.GetLength(0)) pos = mapNodes.GetLength(0) - 1;
                    if (pos < 0) pos = 0;

                    if (mapNodes[pos, column] == null){

                        mapNodes[pos, column] = new MapNode(new List<MapNode>(), GenerateNodeType(),
                            new Vector2(random.Next(-randomChange, randomChange) + leftOffset + column * rowSpacing,
                            random.Next(-randomChange, randomChange) + topOffset + pos * columnSpacing));
                    }
                }
            }
            //Boss node generation
            mapNodes[0, mapNodes.GetLength(1)-1] = new MapNode(new List<MapNode>(), NodeType.BOSS,
                    new Vector2(leftOffset + (mapNodes.GetLength(1)-1) * rowSpacing, mapNodes.GetLength(0)/2*columnSpacing));

            //First node for traversal generation
            List<MapNode> firstRow = new List<MapNode>();
            for(int node = 0; node < mapNodes.GetLength(0); node++){
                if(mapNodes[node,1] != null){
                    firstRow.Add(mapNodes[node,1]);
                }
            }
            mapNodes[0,0] = new MapNode(firstRow, NodeType.NOTHING, new Vector2());

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

                        spriteBatch.DrawString(bigFont, MapIcon(node.type), node.point, Color.Black);
                    }
                }
            }
            spriteBatch.DrawString(smallFont, "F - Fight   E - Elite fight   ? - Random\n  $ - Shop   X - Treasure   B - Boss", new Vector2(leftOffset + 100,520), Color.Black);
        }

        private void DrawDottedPath(Vector2 start, Vector2 end)
        {
            int dotSpacing = 15;
            Vector2 direction = Vector2.Normalize(end - start);
            float distance = Vector2.Distance(start, end);
            
            for (float d = dotSpacing; d < distance-dotSpacing; d += dotSpacing){
                Vector2 dotPosition = start + direction * d;
                spriteBatch.DrawString(bigFont, ".", dotPosition, Color.Gray);
            }
        }

        private NodeType GenerateNodeType(){
            int roll = random.Next(0,101);
            if(roll < 50) return NodeType.FIGHT;
            if(roll < 80) return NodeType.RANDOM;
            if(roll < 87) return NodeType.SHOP;
            if(roll < 97) return NodeType.ELITE;
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
                
                MapNode selectedNode = node.forward[nodeSelectIndex];
                if(state.IsKeyDown(Keys.Enter) && enterUp){
                    enterUp = false;
                    nodeSelectIndex = 0;
                    return selectedNode;
                }
                else if(state.IsKeyUp(Keys.Enter)) enterUp = true;
                
                spriteBatch.Draw(texture, new Rectangle((int)selectedNode.point.X-5, (int)selectedNode.point.Y-5, 30, 40), Color.Gray);
            }
            if(node.type != NodeType.NOTHING){
                spriteBatch.Draw(texture, new Rectangle((int)node.point.X-5, (int)node.point.Y-5, 30, 40), Color.MediumVioletRed);
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