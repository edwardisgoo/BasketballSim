using System.Linq;
using System.Windows;
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
        private bool sortAscending = false; // default overall descending

        public DraftView(DraftManager draftManager)
        {
            this.draftManager = draftManager;
            this.draftManager.DraftCompleted += DraftManager_DraftCompleted;
            InitializeComponent();
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
            if (sortColumn == "Name")
            {
                sorted = sortAscending
                    ? players.OrderBy(p => p.ShortName)
                    : players.OrderByDescending(p => p.ShortName);
            }
            else if (sortColumn == "Position")
            {
                string[] order = { "PG", "SG", "SF", "PF", "C" };
                sorted = sortAscending
                    ? players.OrderBy(p => System.Array.IndexOf(order, p.Position)).ThenByDescending(p => p.Overall)
                    : players.OrderByDescending(p => System.Array.IndexOf(order, p.Position)).ThenByDescending(p => p.Overall);
            }
            else // Overall
            {
                sorted = sortAscending
                    ? players.OrderBy(p => p.Overall)
                    : players.OrderByDescending(p => p.Overall);
            }

            var list = sorted.ToList();
            AvailablePlayersListView.ItemsSource = list;
            if (list.Count > 0)
                AvailablePlayersListView.SelectedIndex = 0;
        }

        private static string FormatPlayer(Player player)
        {
            return $"{player.FullName} - {player.Nationality} | Age: {player.Age} | Pos: {player.Position} | Overall: {player.Overall}";
        }

        private void UpdatePickInfo()
        {
            PickInfoText.Text = $"Pick {draftManager.CurrentPickNumber} - Team {draftManager.CurrentTeamIndex + 1}";
        }

        private void AvailablePlayersListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var player = AvailablePlayersListView.SelectedItem as Player;
            if (player != null)
            {
                SelectedPlayerText.Text = FormatPlayer(player);
            }
        }

        private void AvailablePlayersHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is System.Windows.Controls.GridViewColumnHeader header && header.Column != null)
            {
                string column = header.Column.Header.ToString() ?? string.Empty;
                column = column switch
                {
                    "Name" => "Name",
                    "Pos" => "Position",
                    "Ovr" => "Overall",
                    _ => sortColumn
                };

                if (sortColumn == column)
                    sortAscending = !sortAscending;
                else
                {
                    sortColumn = column;
                    sortAscending = true;
                }
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
            var teamView = new TeamView(draftManager.CurrentTeam);
            teamView.Show();
        }

        private void DraftPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            var player = AvailablePlayersListView.SelectedItem as Player;
            if (player != null)
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

            if (round <= 5)
                return bestAvailable.First();

            string[] positions = { "PG", "SG", "SF", "PF", "C" };
            var needed = positions.Where(pos => roster.Count(p => p.Position == pos) < 2).ToList();
            if (needed.Count > 0)
            {
                var pick = draftManager.AvailablePlayers
                    .Where(p => needed.Contains(p.Position))
                    .OrderByDescending(p => p.Overall)
                    .FirstOrDefault();
                if (pick != null)
                    return pick;
            }
            return bestAvailable.First();
        }
    }
}