using System;

namespace BasketballSim.Models
{
    public class PlayerSeasonStats
    {
        public int GamesPlayed { get; set; }
        public int TotalPoints { get; set; }
        public int TotalRebounds { get; set; }
        public int TotalAssists { get; set; }
        public int TotalSteals { get; set; }
        public int TotalBlocks { get; set; }
        public int FGMade { get; set; }
        public int FGAttempted { get; set; }
        public int ThreeMade { get; set; }
        public int ThreeAttempted { get; set; }

        public double PPG => GamesPlayed > 0 ? Math.Round((double)TotalPoints / GamesPlayed, 1) : 0;
        public double RPG => GamesPlayed > 0 ? Math.Round((double)TotalRebounds / GamesPlayed, 1) : 0;
        public double APG => GamesPlayed > 0 ? Math.Round((double)TotalAssists / GamesPlayed, 1) : 0;
        public double SPG => GamesPlayed > 0 ? Math.Round((double)TotalSteals / GamesPlayed, 1) : 0;
        public double BPG => GamesPlayed > 0 ? Math.Round((double)TotalBlocks / GamesPlayed, 1) : 0;
        public double FGPct => FGAttempted > 0 ? Math.Round((double)FGMade / FGAttempted * 100, 1) : 0;
        public double ThreePct => ThreeAttempted > 0 ? Math.Round((double)ThreeMade / ThreeAttempted * 100, 1) : 0;

        public void Reset()
        {
            GamesPlayed = 0;
            TotalPoints = 0;
            TotalRebounds = 0;
            TotalAssists = 0;
            TotalSteals = 0;
            TotalBlocks = 0;
            FGMade = 0;
            FGAttempted = 0;
            ThreeMade = 0;
            ThreeAttempted = 0;
        }
    }
}
