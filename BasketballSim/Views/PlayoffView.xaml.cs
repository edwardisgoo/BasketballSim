using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BasketballSim.Logic;
using BasketballSim.Models;

namespace BasketballSim.Views
{
    public partial class PlayoffView : Window
    {
        private readonly PlayoffSimulator simulator;

        // One TextBlock per series slot per round:
        // roundSeriesBlocks[r][i] = the TextBlock for Rounds[r][i]
        private readonly List<List<TextBlock>> roundSeriesBlocks = new();

        public PlayoffView(List<Team> seededPlayoffTeams)
        {
            InitializeComponent();
            this.PreviewKeyDown += PlayoffView_KeyDown;
            simulator = new PlayoffSimulator(seededPlayoffTeams);
            BuildBracket();
        }

        // ──────────────────────────────────────────────────────────────
        // Build the initial bracket UI
        // ──────────────────────────────────────────────────────────────

        private void BuildBracket()
        {
            // Round 1: 4 series
            var r1Blocks = new List<TextBlock>();
            foreach (var s in simulator.Rounds[0])
            {
                var tb = MakeSeriesBlock(simulator.GetTeamName(s.TeamA), simulator.GetTeamName(s.TeamB), s.ScoreLabel);
                Round1Panel.Children.Add(tb);
                r1Blocks.Add(tb);
            }
            roundSeriesBlocks.Add(r1Blocks);

            // Round 2: 2 series (teams TBD)
            var r2Blocks = new List<TextBlock>();
            for (int i = 0; i < 2; i++)
            {
                var tb = MakeSeriesBlock("TBD", "TBD", "(0-0)");
                Round2Panel.Children.Add(tb);
                r2Blocks.Add(tb);
            }
            roundSeriesBlocks.Add(r2Blocks);

            // Round 3 / Finals: 1 series (teams TBD)
            var r3Blocks = new List<TextBlock>();
            var finalsBlock = MakeSeriesBlock("TBD", "TBD", "(0-0)");
            Round3Panel.Children.Add(finalsBlock);
            r3Blocks.Add(finalsBlock);
            roundSeriesBlocks.Add(r3Blocks);
        }

        private static TextBlock MakeSeriesBlock(string teamA, string teamB, string score)
        {
            return new TextBlock
            {
                Text = FormatSeries(teamA, teamB, score),
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)),
                Padding = new Thickness(10, 8, 10, 8),
                Margin = new Thickness(0, 6, 0, 6),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
            };
        }

        private static string FormatSeries(string a, string b, string score) =>
            $"{a}  vs  {b}  {score}";

        // ──────────────────────────────────────────────────────────────
        // Keyboard handling
        // ──────────────────────────────────────────────────────────────

        private void PlayoffView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!simulator.IsComplete)
                {
                    simulator.SimulateNextGame();
                    UpdateBracket();
                }
                // Allow Enter to dismiss the "champion" state too
            }
            else if (e.Key == Key.Escape)
            {
                var result = MessageBox.Show("Exit?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                    Application.Current.Shutdown();
            }
        }

        // ──────────────────────────────────────────────────────────────
        // Update bracket after each game
        // ──────────────────────────────────────────────────────────────

        private void UpdateBracket()
        {
            for (int r = 0; r < simulator.Rounds.Count; r++)
            {
                var round = simulator.Rounds[r];
                for (int i = 0; i < round.Count; i++)
                {
                    var series = round[i];
                    if (series.TeamA == -1 || series.TeamB == -1) continue;

                    // Refresh current round's series display
                    string nameA = simulator.GetTeamName(series.TeamA);
                    string nameB = simulator.GetTeamName(series.TeamB);
                    var tb = roundSeriesBlocks[r][i];
                    tb.Text = FormatSeries(nameA, nameB, series.ScoreLabel);

                    // Highlight completed series winner
                    if (series.IsComplete)
                    {
                        string winner = simulator.GetTeamName(series.WinnerIndex);
                        tb.Text = $"✓ {winner} wins {series.ScoreLabel}";
                        tb.Foreground = new SolidColorBrush(Color.FromRgb(226, 183, 20));

                        // Advance to next round's UI block
                        int nextR = r + 1;
                        if (nextR < roundSeriesBlocks.Count)
                        {
                            // Which series in the next round does this winner belong to?
                            int nextSeriesIdx = i / 2;
                            var nextRound = simulator.Rounds[nextR];
                            if (nextSeriesIdx < nextRound.Count)
                            {
                                var nextSeries = nextRound[nextSeriesIdx];
                                if (nextSeries.TeamA != -1 && nextSeries.TeamB != -1)
                                {
                                    // Both teams now known – update the display
                                    string nA = simulator.GetTeamName(nextSeries.TeamA);
                                    string nB = simulator.GetTeamName(nextSeries.TeamB);
                                    roundSeriesBlocks[nextR][nextSeriesIdx].Text =
                                        FormatSeries(nA, nB, nextSeries.ScoreLabel);
                                    roundSeriesBlocks[nextR][nextSeriesIdx].Foreground = Brushes.White;
                                }
                            }
                        }

                        // Finals winner → champion box
                        if (r == simulator.Rounds.Count - 1)
                        {
                            ChampionNameText.Text = winner;
                            ChampionBanner.Text = $"🏆  {winner} are Champions!";
                            InstructionText.Text = "Season complete.";
                        }
                    }
                }
            }
        }
    }
}
