using System;
using System.Collections.Generic;
using System.Linq;
using BasketballSim.Models;

namespace BasketballSim.Logic
{
    public class SeasonSimulator
    {
        private static readonly Random rng = new();
        private readonly List<Game> schedule;
        private readonly List<Team> teams;
        public int CurrentDay { get; private set; }

        public SeasonSimulator(List<Team> league)
        {
            teams = league;
            schedule = GenerateSchedule();
        }

        private List<Game> GenerateSchedule()
        {
            var sched = new List<Game>();
            for (int day = 0; day < 82; day++)
            {
                var indexes = Enumerable.Range(0, teams.Count).ToList();
                Shuffle(indexes);
                for (int i = 0; i < indexes.Count; i += 2)
                {
                    sched.Add(new Game
                    {
                        Day = day,
                        HomeTeamIndex = indexes[i],
                        AwayTeamIndex = indexes[i + 1]
                    });
                }
            }
            return sched;
        }

        private static void Shuffle(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public IReadOnlyList<Game> Schedule => schedule;
        public bool IsSeasonComplete => CurrentDay >= 82;

        public void SimulateNextDay()
        {
            var games = schedule.Where(g => g.Day == CurrentDay);
            foreach (var g in games)
            {
                g.HomeScore = rng.Next(90, 121);
                g.AwayScore = rng.Next(90, 121);
            }
            CurrentDay++;
        }

        public (int Wins, int Losses) GetRecord(int teamIndex)
        {
            int wins = 0, losses = 0;
            foreach (var g in schedule.Where(g => g.HomeScore.HasValue))
            {
                if (g.HomeTeamIndex == teamIndex)
                {
                    if (g.HomeScore > g.AwayScore) wins++; else losses++;
                }
                else if (g.AwayTeamIndex == teamIndex)
                {
                    if (g.AwayScore > g.HomeScore) wins++; else losses++;
                }
            }
            return (wins, losses);
        }

        public List<(int TeamIndex, int Wins)> GetStandings()
        {
            var records = new List<(int, int)>();
            for (int i = 0; i < teams.Count; i++)
            {
                var r = GetRecord(i);
                records.Add((i, r.Wins));
            }
            return records.OrderByDescending(r => r.Item2).ToList();
        }
    }
}