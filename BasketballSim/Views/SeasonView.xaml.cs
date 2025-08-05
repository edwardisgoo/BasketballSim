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
            teamGames = simulator.Schedule.Where(g => g.HomeTeamIndex == teamIndex || g.AwayTeamIndex == teamIndex)
                                          .ToDictionary(g => g.Day);
            BuildCalendar();
        }

        private void BuildCalendar()
        {
            for (int day = 0; day < 82; day++)
            {
                var border = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    Padding = new Thickness(5),
                    Margin = new Thickness(2)
                };
                var tb = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (teamGames.TryGetValue(day, out var game))
                {
                    int opp = game.HomeTeamIndex == teamIndex ? game.AwayTeamIndex : game.HomeTeamIndex;
                    tb.Text = $"Team {opp + 1}";
                }

                border.Child = tb;
                CalendarGrid.Children.Add(border);
            }
            HighlightDay(0);
        }

        private void HighlightDay(int day)
        {
            if (currentHighlight != null)
                currentHighlight.Background = Brushes.White;
            if (day < CalendarGrid.Children.Count)
            {
                if (CalendarGrid.Children[day] is Border border)
                {
                    border.Background = Brushes.LightBlue;
                    currentHighlight = border;
                }
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
                if (simulator.IsSeasonComplete)
                {
                    ShowStandings();
                }
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
                border.Background = Brushes.LightGray;
                if (teamGames.TryGetValue(day, out var game) && game.HomeScore.HasValue)
                {
                    int teamScore, oppScore;
                    bool win;
                    if (game.HomeTeamIndex == teamIndex)
                    {
                        teamScore = game.HomeScore.Value;
                        oppScore = game.AwayScore.Value;
                    }
                    else
                    {
                        teamScore = game.AwayScore.Value;
                        oppScore = game.HomeScore.Value;
                    }
                    win = teamScore > oppScore;
                    tb.Text = $"{teamScore}-{oppScore} {(win ? "W" : "L")}";
                    tb.Foreground = win ? Brushes.Green : Brushes.Red;
                }
            }
        }

        private void ShowStandings()
        {
            var standings = simulator.GetStandings();
            var west = standings.Where(t => t.TeamIndex < 15).ToList();
            var east = standings.Where(t => t.TeamIndex >= 15).ToList();
            string msg = "West\n";
            for (int i = 0; i < west.Count; i++)
            {
                msg += $"{i + 1}. Team {west[i].TeamIndex + 1} {west[i].Wins}-{82 - west[i].Wins}\n";
            }
            msg += "\nEast\n";
            for (int i = 0; i < east.Count; i++)
            {
                msg += $"{i + 1}. Team {east[i].TeamIndex + 1} {east[i].Wins}-{82 - east[i].Wins}\n";
            }
            MessageBox.Show(msg, "Season Complete");
        }
    }
}