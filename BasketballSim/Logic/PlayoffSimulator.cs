using System;
using System.Collections.Generic;
using System.Linq;
using BasketballSim.Models;

namespace BasketballSim.Logic
{
    public class PlayoffSimulator
    {
        private static readonly Random rng = new();
        private readonly List<Team> teams;
        public List<List<Series>> Rounds { get; } = new();

        public PlayoffSimulator(List<Team> league)
        {
            teams = league.Take(8).ToList();
            if (teams.Count < 8)
                throw new ArgumentException("Need at least 8 teams", nameof(league));

            Rounds.Add(new List<Series>
            {
                new Series(0,7),
                new Series(3,4),
                new Series(2,5),
                new Series(1,6)
            });
            Rounds.Add(new List<Series>());
            Rounds.Add(new List<Series>());
        }

        public string GetTeamName(int index) => teams[index].Name;

        public bool IsComplete => Rounds.Last().Count == 1 && Rounds.Last()[0].IsComplete;

        public void SimulateNextGame()
        {
            if (IsComplete) return;
            for (int r = 0; r < Rounds.Count; r++)
            {
                for (int i = 0; i < Rounds[r].Count; i++)
                {
                    var series = Rounds[r][i];
                    if (series.TeamA != -1 && series.TeamB != -1 && !series.IsComplete)
                    {
                        int scoreA = rng.Next(90, 121);
                        int scoreB = rng.Next(90, 121);
                        if (scoreA >= scoreB) series.WinsA++; else series.WinsB++;
                        if (series.IsComplete)
                            AdvanceTeam(r, i, series);
                        return;
                    }
                }
            }
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
            else next.TeamB = series.WinnerIndex;
        }

        public class Series
        {
            public int TeamA { get; set; }
            public int TeamB { get; set; }
            public int WinsA { get; set; }
            public int WinsB { get; set; }
            public Series() { TeamA = -1; TeamB = -1; }
            public Series(int a, int b) { TeamA = a; TeamB = b; }
            public bool IsComplete => WinsA >= 4 || WinsB >= 4;
            public int WinnerIndex => WinsA > WinsB ? TeamA : TeamB;
        }
    }
}