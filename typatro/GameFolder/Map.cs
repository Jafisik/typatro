using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace typatro.GameFolder{
    enum NodeType{
        FIGHT,
        ELITE,
        TREASURE,
        SHOP,
        RANDOM,
        BOSS,
        NORMAL
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
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        MapNode[,] mapNodes;
        int rowSpacing = 75, columnSpacing = 70, randomChange = 12;
        int topOffset = 30, leftOffset = 30;
        int paths = 4;
        Random random = new Random();
        public Map(SpriteBatch spriteBatch, SpriteFont font){
            this.spriteBatch = spriteBatch;
            this.font = font;

            GenerateNodes();
        }

        public void GenerateNodes()
        {
            mapNodes = new MapNode[8, 13];
            int pos = random.Next(0, mapNodes.GetLength(0));

            //Generates first node and then makes a path by going to i+1 or i or i-1 node
            for (int path = 0; path < paths; path++){
                while (mapNodes[pos, 0] != null){
                    pos = random.Next(0, mapNodes.GetLength(0));
                }

                mapNodes[pos, 0] = new MapNode(new List<MapNode>(), NodeType.NORMAL,
                    new Vector2(leftOffset, random.Next(-randomChange, randomChange) + topOffset + pos * columnSpacing));

                for (int column = 1; column < mapNodes.GetLength(1)-1; column++){
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
            List<MapNode> lastRow = new List<MapNode>();
            for(int node = 0; node < mapNodes.GetLength(0); node++){
                if(mapNodes[node,mapNodes.GetLength(1)-2] != null){
                    lastRow.Add(mapNodes[node,mapNodes.GetLength(1)-2]);
                }
            }
            mapNodes[0, mapNodes.GetLength(1)-1] = new MapNode(lastRow, NodeType.BOSS,
                    new Vector2( + leftOffset + (mapNodes.GetLength(1)-1) * rowSpacing, mapNodes.GetLength(0)/2*columnSpacing));

            //Connects to previous nodes
            for (int column = 1; column < mapNodes.GetLength(1); column++){
                for (int row = 0; row < mapNodes.GetLength(0); row++){
                    MapNode currentNode = mapNodes[row, column];

                    if (currentNode != null){
                        if (mapNodes[row, column - 1] != null)
                            currentNode.forward.Add(mapNodes[row, column - 1]);

                        if (row - 1 >= 0 && mapNodes[row - 1, column - 1] != null)
                            currentNode.forward.Add(mapNodes[row - 1, column - 1]);

                        if (row + 1 < mapNodes.GetLength(0) && mapNodes[row + 1, column - 1] != null)
                            currentNode.forward.Add(mapNodes[row + 1, column - 1]);
                    }
                }
            }
        }

        public void DrawNodes()
        {
            for (int i = 0; i < mapNodes.GetLength(0); i++){
                for (int j = 0; j < mapNodes.GetLength(1); j++){
                    MapNode node = mapNodes[i, j];

                    if (node != null){
                        spriteBatch.DrawString(font, MapIcon(node.type), node.point, Color.Black);

                        foreach (MapNode next in node.forward){
                            if (next != null){
                                DrawDottedPath(node.point, next.point);
                            }
                        }
                    }
                }
            }
        }

        private void DrawDottedPath(Vector2 start, Vector2 end)
        {
            int dotSpacing = 15;
            Vector2 direction = Vector2.Normalize(end - start);
            float distance = Vector2.Distance(start, end);
            
            for (float d = dotSpacing; d < distance-dotSpacing; d += dotSpacing){
                Vector2 dotPosition = start + direction * d;
                spriteBatch.DrawString(font, ".", dotPosition, Color.Gray);
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

        private string MapIcon(NodeType type){
            switch(type){
                case NodeType.FIGHT:
                    return "F";
                case NodeType.ELITE:
                    return "E";
                case NodeType.BOSS:
                    return "B";
                case NodeType.RANDOM:
                    return "?";
                case NodeType.SHOP:
                    return "$";
                case NodeType.TREASURE:
                    return "X";
                case NodeType.NORMAL:
                    return "N";
            }
            return "A";
        }
    }
}