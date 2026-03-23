using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BasketballSim.Logic;
using BasketballSim.Models;

namespace BasketballSim.Views
{
    public partial class StandingsView : Window
    {
        private readonly List<(int TeamIndex, int Wins)> standings;
        private readonly SeasonSimulator? season;

        public StandingsView(List<(int TeamIndex, int Wins)> standings)
        {
            InitializeComponent();
            this.standings = standings;
            season = FranchiseContext.CurrentSeason;
            PopulateStandings();
            PopulateStatsLeaders();
        }

        private void PopulateStandings()
        {
            // West = teams 0–14, East = teams 15–29 (sorted by wins within conference)
            var west = standings.Where(s => s.TeamIndex < 15)
                                .OrderByDescending(s => s.Wins).ToList();
            var east = standings.Where(s => s.TeamIndex >= 15)
                                .OrderByDescending(s => s.Wins).ToList();

            var league = FranchiseContext.CurrentLeague;

            for (int i = 0; i < west.Count; i++)
            {
                int idx = west[i].TeamIndex;
                string name = league != null && idx < league.Count ? league[idx].Name : $"Team {idx + 1}";
                int w = west[i].Wins, l = 82 - w;
                string seed = i < 8 ? $"*{i + 1}" : $" {i + 1}";
                WestList.Items.Add($"{seed}. {name,-28} {w,2}-{l,2}");
            }

            for (int i = 0; i < east.Count; i++)
            {
                int idx = east[i].TeamIndex;
                string name = league != null && idx < league.Count ? league[idx].Name : $"Team {idx + 1}";
                int w = east[i].Wins, l = 82 - w;
                string seed = i < 8 ? $"*{i + 1}" : $" {i + 1}";
                EastList.Items.Add($"{seed}. {name,-28} {w,2}-{l,2}");
            }
        }

        private void PopulateStatsLeaders()
        {
            if (season == null) return;

            var ppg = season.GetLeaders(p => p.SeasonStats.PPG, 1).FirstOrDefault();
            var rpg = season.GetLeaders(p => p.SeasonStats.RPG, 1).FirstOrDefault();
            var apg = season.GetLeaders(p => p.SeasonStats.APG, 1).FirstOrDefault();
            var spg = season.GetLeaders(p => p.SeasonStats.SPG, 1).FirstOrDefault();
            var bpg = season.GetLeaders(p => p.SeasonStats.BPG, 1).FirstOrDefault();

            if (ppg.Player != null)
                PpgLeaderText.Text = $"PPG\n{ppg.Player.ShortName}\n{ppg.Value:F1}";
            if (rpg.Player != null)
                RpgLeaderText.Text = $"RPG\n{rpg.Player.ShortName}\n{rpg.Value:F1}";
            if (apg.Player != null)
                ApgLeaderText.Text = $"APG\n{apg.Player.ShortName}\n{apg.Value:F1}";
            if (spg.Player != null)
                SpgLeaderText.Text = $"SPG\n{spg.Player.ShortName}\n{spg.Value:F1}";
            if (bpg.Player != null)
                BpgLeaderText.Text = $"BPG\n{bpg.Player.ShortName}\n{bpg.Value:F1}";
        }

        private void ViewPlayoffs_Click(object sender, RoutedEventArgs e)
        {
            // Seed the top 8 teams by overall record (combining both conferences)
            // This fixes the bug where PlayoffSimulator previously received
            // the unordered FranchiseContext.CurrentLeague.Take(8).
            var league = FranchiseContext.CurrentLeague;
            if (league == null) return;

            var top8 = standings.Take(8)
                                .Select(s => league[s.TeamIndex])
                                .ToList();

            var bracket = new PlayoffView(top8);
            bracket.Show();
        }
    }
}
