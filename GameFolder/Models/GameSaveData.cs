using System.Collections.Generic;

namespace typatro.GameFolder.Models
{
    public class GameSaveData
    {
        public int[] mapNode { get; set; }
        public long[] letterScores { get; set; }
        public int[] enhancements { get; set; }
        public double[] enhChances { get; set; }
        public int[] glyphs { get; set; }
        public long coins { get; set; }
        public int level { get; set; }
        public int seed { get; set; }
        public int difficulty { get; set; }
        public int rune { get; set; }
        public List<int[]> visitedNodes { get; set; }
    }
}
