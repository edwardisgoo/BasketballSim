using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BasketballSim.Logic;
using BasketballSim.Models;

namespace BasketballSim.Views
{
    public partial class SeasonView : Window
    {
        private readonly SeasonSimulator simulator;
        private Dictionary<int, Game> teamGames = new();
        private Border? currentHighlight;
        private readonly int teamIndex;

        public SeasonView()
        {
            InitializeComponent();
            this.PreviewKeyDown += SeasonView_KeyDown;

            simulator = new SeasonSimulator(FranchiseContext.CurrentLeague ?? new List<Team>());
            FranchiseContext.CurrentSeason = simulator;
            teamIndex = FranchiseContext.CurrentTeamIndex;

            teamGames = simulator.Schedule
                .Where(g => g.HomeTeamIndex == teamIndex || g.AwayTeamIndex == teamIndex)
                .ToDictionary(g => g.Day);

            // Show team name in header
            var league = FranchiseContext.CurrentLeague;
            if (league != null && teamIndex < league.Count)
                TeamNameText.Text = league[teamIndex].Name;

            BuildCalendar();
            UpdateRecord();
        }

        private void BuildCalendar()
        {
            var league = FranchiseContext.CurrentLeague;
            for (int day = 0; day < 82; day++)
            {
                var border = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(50, 50, 80)),
                    Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)),
                    Padding = new Thickness(5),
                    Margin = new Thickness(2),
                };
                var tb = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White,
                    FontSize = 11,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                };

                if (teamGames.TryGetValue(day, out var game))
                {
                    int opp = game.HomeTeamIndex == teamIndex ? game.AwayTeamIndex : game.HomeTeamIndex;
                    bool isHome = game.HomeTeamIndex == teamIndex;
                    string oppName = league != null && opp < league.Count
                        ? league[opp].Name
                        : $"Team {opp + 1}";
                    // Abbreviate: show first word of team name
                    string abbrev = oppName.Split(' ')[0];
                    tb.Text = $"Day {day + 1}\n{(isHome ? "vs" : "@")} {abbrev}";
                }
                else
                {
                    tb.Text = $"Day {day + 1}";
                    tb.Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 100));
                }

                border.Child = tb;
                CalendarGrid.Children.Add(border);
            }
            HighlightDay(0);
        }

        private void HighlightDay(int day)
        {
            if (currentHighlight != null)
            {
                // Restore to either played or upcoming color
                int idx = CalendarGrid.Children.IndexOf(currentHighlight);
                bool isPlayedGame = teamGames.TryGetValue(idx, out var g) && g.HomeScore.HasValue;
                currentHighlight.Background = isPlayedGame
                    ? new SolidColorBrush(Color.FromRgb(30, 30, 50))
                    : new SolidColorBrush(Color.FromRgb(15, 52, 96));
            }
            if (day < CalendarGrid.Children.Count && CalendarGrid.Children[day] is Border border)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(226, 183, 20));
                currentHighlight = border;
            }
        }

        private void SeasonView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !simulator.IsSeasonComplete)
            {
                simulator.SimulateNextDay();
                int playedDay = simulator.CurrentDay - 1;
                UpdateDay(playedDay);
                HighlightDay(simulator.CurrentDay);
                UpdateRecord();
                UpdateStatsLeaders();

                if (simulator.IsSeasonComplete)
                    ShowStandings();
            }
            else if (e.Key == Key.Escape)
            {
                var result = MessageBox.Show("Exit?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                    Application.Current.Shutdown();
            }
        }

        private void UpdateDay(int day)
        {
            if (day >= CalendarGrid.Children.Count) return;
            if (CalendarGrid.Children[day] is Border border && border.Child is TextBlock tb)
            {
                if (teamGames.TryGetValue(day, out var game) && game.HomeScore.HasValue)
                {
                    int teamScore, oppScore;
                    if (game.HomeTeamIndex == teamIndex)
                    {
                        teamScore = game.HomeScore!.Value;
                        oppScore  = game.AwayScore!.Value;
                    }
                    else
                    {
                        teamScore = game.AwayScore!.Value;
                        oppScore  = game.HomeScore!.Value;
                    }
                    bool win = teamScore > oppScore;
                    tb.Text = $"{teamScore}-{oppScore}\n{(win ? "W" : "L")}";
                    tb.Foreground = win ? new SolidColorBrush(Color.FromRgb(80, 220, 80))
                                       : new SolidColorBrush(Color.FromRgb(220, 80, 80));
                    border.Background = new SolidColorBrush(Color.FromRgb(30, 30, 50));
                }
                else
                {
                    border.Background = new SolidColorBrush(Color.FromRgb(30, 30, 50));
                }
            }
        }

        private void UpdateRecord()
        {
            var (w, l) = simulator.GetRecord(teamIndex);
            RecordText.Text = $"Record: {w}-{l}  |  Day {simulator.CurrentDay} / 82";
        }

        private void UpdateStatsLeaders()
        {
            var ppg = simulator.GetLeaders(p => p.SeasonStats.PPG, 1).FirstOrDefault();
            var rpg = simulator.GetLeaders(p => p.SeasonStats.RPG, 1).FirstOrDefault();
            var apg = simulator.GetLeaders(p => p.SeasonStats.APG, 1).FirstOrDefault();

            if (ppg.Player != null)
                PpgLeaderText.Text = $"Points leader: {ppg.Player.ShortName}  {ppg.Value:F1} PPG";
            if (rpg.Player != null)
                RpgLeaderText.Text = $"Rebounds leader: {rpg.Player.ShortName}  {rpg.Value:F1} RPG";
            if (apg.Player != null)
                ApgLeaderText.Text = $"Assists leader: {apg.Player.ShortName}  {apg.Value:F1} APG";
        }

        private void ShowStandings()
        {
            var standings = simulator.GetStandings();
            var sv = new StandingsView(standings);
            sv.Show();
        }
    }
}
