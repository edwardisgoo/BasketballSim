using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using BasketballSim.Logic;
using BasketballSim.Models;

namespace BasketballSim.Views
{
    public partial class DraftView : Window
    {
        private readonly DraftManager draftManager;
        private List<Player> players = new();
        private string sortColumn = "Overall";
        private bool sortAscending = false;

        public DraftView(DraftManager draftManager)
        {
            this.draftManager = draftManager;
            this.draftManager.DraftCompleted += DraftManager_DraftCompleted;
            InitializeComponent();
            this.PreviewKeyDown += DraftView_KeyDown;
            LoadPlayers();
        }

        private void LoadPlayers()
        {
            players = draftManager.AvailablePlayers.ToList();
            ApplySorting();
            UpdatePickInfo();
            AvailablePlayersListView.Focus();
        }

        private void ApplySorting()
        {
            IEnumerable<Player> sorted;
            string[] posOrder = { "PG", "SG", "SF", "PF", "C" };

            sorted = sortColumn switch
            {
                "Name"      => sortAscending ? players.OrderBy(p => p.ShortName)
                                             : players.OrderByDescending(p => p.ShortName),
                "Position"  => sortAscending
                                ? players.OrderBy(p => System.Array.IndexOf(posOrder, p.Position)).ThenByDescending(p => p.Overall)
                                : players.OrderByDescending(p => System.Array.IndexOf(posOrder, p.Position)).ThenByDescending(p => p.Overall),
                "Speed"      => sortAscending ? players.OrderBy(p => p.Speed)      : players.OrderByDescending(p => p.Speed),
                "Shooting"   => sortAscending ? players.OrderBy(p => p.Shooting)   : players.OrderByDescending(p => p.Shooting),
                "ThreePoint" => sortAscending ? players.OrderBy(p => p.ThreePoint) : players.OrderByDescending(p => p.ThreePoint),
                "Defense"    => sortAscending ? players.OrderBy(p => p.Defense)    : players.OrderByDescending(p => p.Defense),
                "Rebounding" => sortAscending ? players.OrderBy(p => p.Rebounding) : players.OrderByDescending(p => p.Rebounding),
                "Passing"    => sortAscending ? players.OrderBy(p => p.Passing)    : players.OrderByDescending(p => p.Passing),
                "Interior"   => sortAscending ? players.OrderBy(p => p.Interior)   : players.OrderByDescending(p => p.Interior),
                "IQ"         => sortAscending ? players.OrderBy(p => p.IQ)         : players.OrderByDescending(p => p.IQ),
                _            => sortAscending ? players.OrderBy(p => p.Overall)    : players.OrderByDescending(p => p.Overall),
            };

            var list = sorted.ToList();
            AvailablePlayersListView.ItemsSource = list;
            if (list.Count > 0)
                AvailablePlayersListView.SelectedIndex = 0;
        }

        private static string FormatPlayer(Player p) =>
            $"{p.FullName}  [{p.Nationality}]  Age {p.Age}  {p.Position}  Ovr {p.Overall}\n" +
            $"Spd {p.Speed}  Sht {p.Shooting}  3PT {p.ThreePoint}  " +
            $"Def {p.Defense}  Reb {p.Rebounding}  Pas {p.Passing}  " +
            $"Int {p.Interior}  IQ {p.IQ}";

        private void UpdatePickInfo()
        {
            int teamIdx = draftManager.CurrentTeamIndex;
            string teamName = FranchiseGenerator.TeamNames.Length > teamIdx
                ? FranchiseGenerator.TeamNames[teamIdx]
                : $"Team {teamIdx + 1}";
            PickInfoText.Text = $"Pick {draftManager.CurrentPickNumber}  —  {teamName}";
        }

        private void AvailablePlayersListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AvailablePlayersListView.SelectedItem is Player p)
                SelectedPlayerText.Text = FormatPlayer(p);
        }

        private void AvailablePlayersHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is System.Windows.Controls.GridViewColumnHeader header && header.Column != null)
            {
                string col = header.Column.Header?.ToString() ?? string.Empty;
                col = col switch
                {
                    "Name" => "Name",
                    "Pos"  => "Position",
                    "Ovr"  => "Overall",
                    "Spd"  => "Speed",
                    "Sht"  => "Shooting",
                    "3PT"  => "ThreePoint",
                    "Def"  => "Defense",
                    "Reb"  => "Rebounding",
                    "Pas"  => "Passing",
                    "Int"  => "Interior",
                    "IQ"   => "IQ",
                    _      => sortColumn,
                };
                if (sortColumn == col) sortAscending = !sortAscending;
                else { sortColumn = col; sortAscending = false; }
                ApplySorting();
            }
        }

        private void DraftManager_DraftCompleted(object? sender, System.EventArgs e)
        {
            var summary = new DraftSummary(draftManager.DraftHistory.ToList());
            summary.Show();
            this.Close();
        }

        private void ViewTeamButton_Click(object sender, RoutedEventArgs e)
        {
            var teamView = new TeamView(draftManager);
            teamView.Show();
        }

        private void DraftPlayerButton_Click(object sender, RoutedEventArgs e) => DraftSelected();

        private void DraftView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D)
                DraftSelected();
            else if (e.Key == Key.Escape)
            {
                if (MessageBox.Show("Exit?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    Application.Current.Shutdown();
            }
        }

        private void DraftSelected()
        {
            if (AvailablePlayersListView.SelectedItem is Player player)
            {
                draftManager.PickPlayer(player);
                AutoDraftCpuPicks();
                LoadPlayers();
            }
        }

        private void AutoDraftCpuPicks()
        {
            while (!draftManager.IsDraftComplete && draftManager.CurrentTeamIndex != 0)
            {
                var cpuPlayer = ChooseCpuPlayer(draftManager.CurrentTeamIndex);
                draftManager.PickPlayer(cpuPlayer);
            }
        }

        private Player ChooseCpuPlayer(int teamIndex)
        {
            var roster = draftManager.GetTeamRoster(teamIndex);
            int round = (draftManager.CurrentPickNumber - 1) / 30 + 1;
            var bestAvailable = draftManager.AvailablePlayers.OrderByDescending(p => p.Overall);

            // Early rounds: always take best available
            if (round <= 5)
                return bestAvailable.First();

            // Later rounds: fill positional needs (at least 2 per position)
            string[] positions = { "PG", "SG", "SF", "PF", "C" };
            var needed = positions.Where(pos => roster.Count(p => p.Position == pos) < 2).ToList();
            if (needed.Count > 0)
            {
                var pick = draftManager.AvailablePlayers
                    .Where(p => needed.Contains(p.Position))
                    .OrderByDescending(p => p.Overall)
                    .FirstOrDefault();
                if (pick != null) return pick;
            }
            return bestAvailable.First();
        }
    }
}
