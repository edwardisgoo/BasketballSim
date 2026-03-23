using System;
using System.Collections.Generic;
using System.Linq;
using BasketballSim.Models;

namespace BasketballSim.Logic
{
    /// <summary>
    /// Manages an 8-team, 3-round single-elimination bracket
    /// (seeded 1v8, 4v5, 3v6, 2v7) with best-of-7 series.
    /// Game outcomes are driven by team overall ratings, matching
    /// the NBA2K simulation philosophy.
    /// </summary>
    public class PlayoffSimulator
    {
        private static readonly Random rng = new();
        private readonly List<Team> teams;  // indices match seeds 0–7

        /// <summary>
        /// Rounds[0] = First Round (4 series)
        /// Rounds[1] = Semifinals    (2 series)
        /// Rounds[2] = Finals        (1 series)
        /// </summary>
        public List<List<Series>> Rounds { get; } = new();

        public PlayoffSimulator(List<Team> seededTop8)
        {
            if (seededTop8.Count < 8)
                throw new ArgumentException("Need at least 8 teams", nameof(seededTop8));

            teams = seededTop8.Take(8).ToList();

            Rounds.Add(new List<Series>
            {
                new Series(0, 7),  // 1 vs 8
                new Series(3, 4),  // 4 vs 5
                new Series(2, 5),  // 3 vs 6
                new Series(1, 6),  // 2 vs 7
            });
            Rounds.Add(new List<Series>());
            Rounds.Add(new List<Series>());
        }

        public string GetTeamName(int index) => teams[index].Name;

        public bool IsComplete => Rounds.Last().Count == 1 && Rounds.Last()[0].IsComplete;

        // ──────────────────────────────────────────────────────────────
        // Simulation
        // ──────────────────────────────────────────────────────────────

        public void SimulateNextGame()
        {
            if (IsComplete) return;

            for (int r = 0; r < Rounds.Count; r++)
            {
                for (int i = 0; i < Rounds[r].Count; i++)
                {
                    var series = Rounds[r][i];
                    if (series.TeamA == -1 || series.TeamB == -1 || series.IsComplete)
                        continue;

                    SimulateSeriesGame(series);

                    if (series.IsComplete)
                        AdvanceTeam(r, i, series);
                    return;
                }
            }
        }

        private void SimulateSeriesGame(Series series)
        {
            var teamA = teams[series.TeamA];
            var teamB = teams[series.TeamB];

            double ovrA = WeightedRating(teamA);
            double ovrB = WeightedRating(teamB);

            // Higher-rated team gets a scoring edge; home-court modelled as 2-pt bonus
            // for the higher-seeded (lower-index) team in odd games.
            double scoreA = 97 + (ovrA - 70) * 0.45 - (ovrB - 70) * 0.20 + Gaussian(0, 7);
            double scoreB = 97 + (ovrB - 70) * 0.45 - (ovrA - 70) * 0.20 + Gaussian(0, 7);

            // Home-court advantage: the better-seeded team hosts games 1, 2, 5, 7.
            int gamesPlayed = series.WinsA + series.WinsB;
            bool homeTeamA = gamesPlayed is 0 or 1 or 4 or 6;
            if (homeTeamA) scoreA += 3; else scoreB += 3;

            int sA = Math.Clamp((int)Math.Round(scoreA), 85, 140);
            int sB = Math.Clamp((int)Math.Round(scoreB), 85, 140);
            if (sA == sB) { if (rng.Next(2) == 0) sA++; else sB++; }

            series.LastScoreA = sA;
            series.LastScoreB = sB;

            if (sA > sB) series.WinsA++;
            else         series.WinsB++;
        }

        private static double WeightedRating(Team team)
        {
            var top = team.Players.Take(8).ToList();
            if (top.Count == 0) return 70;
            double w = 0, tw = 0;
            for (int i = 0; i < top.Count; i++)
            {
                double wi = 8 - i;
                w  += top[i].Overall * wi;
                tw += wi;
            }
            return w / tw;
        }

        private void AdvanceTeam(int round, int seriesIndex, Series series)
        {
            if (round >= Rounds.Count - 1) return;
            int pairIndex = seriesIndex / 2;
            var nextRound = Rounds[round + 1];
            while (nextRound.Count <= pairIndex)
                nextRound.Add(new Series());
            var next = nextRound[pairIndex];
            if (next.TeamA == -1) next.TeamA = series.WinnerIndex;
            else                  next.TeamB = series.WinnerIndex;
        }

        // ──────────────────────────────────────────────────────────────
        // Inner class
        // ──────────────────────────────────────────────────────────────

        public class Series
        {
            public int TeamA { get; set; }
            public int TeamB { get; set; }
            public int WinsA { get; set; }
            public int WinsB { get; set; }
            public int LastScoreA { get; set; }
            public int LastScoreB { get; set; }

            public Series() { TeamA = -1; TeamB = -1; }
            public Series(int a, int b) { TeamA = a; TeamB = b; }

            public bool IsComplete => WinsA >= 4 || WinsB >= 4;
            public int WinnerIndex  => WinsA >= 4 ? TeamA : TeamB;

            /// Human-readable series score, e.g. "(3-2)".
            public string ScoreLabel => $"({WinsA}-{WinsB})";
        }

        // ──────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────

        private static double Gaussian(double mean, double stdDev)
        {
            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();
            double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * z;
        }
    }
}
