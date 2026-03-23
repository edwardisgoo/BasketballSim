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

        // ──────────────────────────────────────────────────────────────
        // Core simulation
        // ──────────────────────────────────────────────────────────────

        public void SimulateNextDay()
        {
            var games = schedule.Where(g => g.Day == CurrentDay).ToList();
            foreach (var g in games)
            {
                var home = teams[g.HomeTeamIndex];
                var away = teams[g.AwayTeamIndex];

                var (hs, as_) = SimulateGame(home, away);
                g.HomeScore = hs;
                g.AwayScore = as_;

                GeneratePlayerStats(home, hs);
                GeneratePlayerStats(away, as_);
            }
            CurrentDay++;
        }

        // NBA2K-style: team overall ratings drive the expected score, with
        // realistic home-court advantage and Gaussian variance for upsets.
        private static (int homeScore, int awayScore) SimulateGame(Team home, Team away)
        {
            double homeOvr = TeamRating(home);
            double awayOvr = TeamRating(away);

            // Expected score for an average (Ovr=70) team vs equal opponent is ~105.
            // Each overall point above 70 adds ~0.45 pts of offence, removes ~0.20 pts
            // due to improved opponent defence. Home court = +3.
            double hScore = 97 + (homeOvr - 70) * 0.45 - (awayOvr - 70) * 0.20 + 3 + Gaussian(0, 7);
            double aScore = 97 + (awayOvr - 70) * 0.45 - (homeOvr - 70) * 0.20 + Gaussian(0, 7);

            int h = Math.Clamp((int)Math.Round(hScore), 85, 140);
            int a = Math.Clamp((int)Math.Round(aScore), 85, 140);
            if (h == a) { if (rng.Next(2) == 0) h++; else a++; }
            return (h, a);
        }

        // Weighted average of top-8 players' overall (deeper bench matters less).
        private static double TeamRating(Team team)
        {
            var top = team.Players.Take(8).ToList();
            if (top.Count == 0) return 70;
            double weighted = 0, totalW = 0;
            for (int i = 0; i < top.Count; i++)
            {
                double w = 8 - i;  // starter gets weight 8, 8th man gets weight 1
                weighted += top[i].Overall * w;
                totalW += w;
            }
            return weighted / totalW;
        }

        // Distribute the team's final score among individual players
        // proportional to their scoring ability × estimated minutes.
        private static void GeneratePlayerStats(Team team, int teamScore)
        {
            var players = team.Players.Take(15).ToList();
            if (players.Count == 0) return;

            // Minutes: starters ~32, rotation ~18, deep bench ~6
            var minutes = new double[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                double raw = i < 5  ? 28 + rng.NextDouble() * 10
                           : i < 9  ? 10 + rng.NextDouble() * 14
                                     : rng.NextDouble() * 8;
                minutes[i] = raw;
            }
            double totalMin = minutes.Sum();
            double scale = 240.0 / (totalMin > 0 ? totalMin : 1);
            for (int i = 0; i < minutes.Length; i++)
                minutes[i] *= scale;

            // Scoring weight = individual shooting ability × minutes
            double[] sw = new double[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                double ability = (p.Shooting * 0.45 + p.ThreePoint * 0.30 + p.Speed * 0.10 + p.IQ * 0.15) / 100.0;
                sw[i] = ability * minutes[i];
            }
            double twTotal = sw.Sum();

            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                var s = p.SeasonStats;
                s.GamesPlayed++;

                double share = twTotal > 0 ? sw[i] / twTotal : 1.0 / players.Count;

                // Points with personal variance
                int pts = Math.Max(0, (int)Math.Round(teamScore * share * (0.75 + rng.NextDouble() * 0.50)));
                s.TotalPoints += pts;

                // FG attempts / makes
                double fgPct = Math.Clamp(0.36 + (p.Shooting - 60) * 0.002, 0.30, 0.62);
                int fga = pts > 0 ? Math.Max(1, (int)Math.Round(pts / (fgPct * 2.1))) : 0;
                int fgm = (int)Math.Round(fga * (fgPct + (rng.NextDouble() - 0.5) * 0.04));
                s.FGMade      += Math.Max(0, fgm);
                s.FGAttempted += fga;

                // 3-point attempts
                double threePct = Math.Clamp(0.26 + (p.ThreePoint - 60) * 0.0015, 0.20, 0.50);
                int threea = (int)Math.Round(fga * Math.Clamp(p.ThreePoint / 80.0 * 0.40, 0.05, 0.55));
                int threem = (int)Math.Round(threea * (threePct + (rng.NextDouble() - 0.5) * 0.04));
                s.ThreeMade      += Math.Max(0, threem);
                s.ThreeAttempted += Math.Max(0, threea);

                // Rebounds: big men rebound far more
                double rebAbility = (p.Rebounding * 0.60 + p.Interior * 0.40) / 100.0;
                int rebs = Math.Max(0, (int)Math.Round(minutes[i] * 0.28 * rebAbility * (0.6 + rng.NextDouble() * 0.80)));
                s.TotalRebounds += rebs;

                // Assists: guards and playmakers dish more
                double astAbility = (p.Passing * 0.65 + p.IQ * 0.35) / 100.0;
                int asts = Math.Max(0, (int)Math.Round(minutes[i] * 0.18 * astAbility * (0.6 + rng.NextDouble() * 0.80)));
                s.TotalAssists += asts;

                // Steals: quick, smart defenders
                double stlAbility = (p.Defense * 0.50 + p.Speed * 0.30 + p.IQ * 0.20) / 100.0;
                int stls = Math.Max(0, (int)Math.Round(minutes[i] * 0.04 * stlAbility * (0.5 + rng.NextDouble())));
                s.TotalSteals += stls;

                // Blocks: shot-blockers at the rim
                double blkAbility = (p.Interior * 0.55 + p.Defense * 0.30 + p.Rebounding * 0.15) / 100.0;
                int blks = Math.Max(0, (int)Math.Round(minutes[i] * 0.04 * blkAbility * (0.5 + rng.NextDouble())));
                s.TotalBlocks += blks;
            }
        }

        // ──────────────────────────────────────────────────────────────
        // Stats helpers
        // ──────────────────────────────────────────────────────────────

        /// Returns up to <paramref name="count"/> (player, team, value) tuples sorted desc.
        public List<(Player Player, Team Team, double Value)> GetLeaders(
            Func<Player, double> statSelector, int count = 5)
        {
            return teams
                .SelectMany(t => t.Players.Select(p => (Player: p, Team: t)))
                .Where(x => x.Player.SeasonStats.GamesPlayed > 0)
                .OrderByDescending(x => statSelector(x.Player))
                .Take(count)
                .Select(x => (x.Player, x.Team, statSelector(x.Player)))
                .ToList();
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
