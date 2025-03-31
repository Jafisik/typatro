using System;
using System.Collections.Generic;
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
        public MapNode[] forward;
        public NodeType type;
        public MapNode(MapNode[] forward, NodeType type){
            this.forward = forward;
            this.type = type;
        }
    }
    class Map{
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        List<List<MapNode>> mapNodes = new List<List<MapNode>>();
        int rowSpacing = 80, columnSpacing = 100, randomChange = 20, columnsCount = 8;
        int seed;
        public Map(SpriteBatch spriteBatch, SpriteFont font, int seed){
            this.spriteBatch = spriteBatch;
            this.font = font;
            this.seed = seed;
        }

        public void GenerateNodes(){
            Random random = new Random();
            List<MapNode> nodes = new List<MapNode>{ new MapNode(null, NodeType.BOSS) };
            mapNodes.Add(nodes);
            for(int column = columnsCount; column > 0; column--){
                nodes.Clear();
                int nodeCount = random.Next(2,6);
                for(int node = 0; node < nodeCount; node++){
                    List<MapNode> forwardNodes = new List<MapNode>();
                    forwardNodes.Add(mapNodes[column+1][node]);
                    nodes.Add(new MapNode(forwardNodes.ToArray(), (NodeType)random.Next(0,5)));
                }
            }
            nodes = new List<MapNode>{ new MapNode(mapNodes[0].ToArray(), NodeType.NORMAL) };
            mapNodes.Add(nodes);
        }

        public void DrawNodes(){
            Random rand = new Random(seed);
            int rowSpace = rowSpacing + rand.Next(-randomChange,randomChange), columnSpace = columnSpacing + rand.Next(-randomChange,randomChange);
            for(int nodeRow = mapNodes.Count-1; nodeRow >= 0; nodeRow--){
                for(int nodeNum = 0; nodeNum < mapNodes[nodeRow].Count; nodeNum++){
                    spriteBatch.DrawString(font, MapIcon(mapNodes[nodeRow][nodeNum].type), new Vector2(columnSpace + rand.Next(-randomChange,randomChange),rowSpace), Color.Black);
                    rowSpace += rowSpacing + rand.Next(-randomChange,randomChange);
                }
                rowSpace = rowSpacing + rand.Next(-randomChange,randomChange);
                columnSpace += columnSpacing;
            }
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