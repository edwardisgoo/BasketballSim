using System.Linq;
using System.Windows;
using BasketballSim.Logic;
using BasketballSim.Models;

namespace BasketballSim.Views
{
    public partial class DraftView : Window
    {
        private readonly DraftManager draftManager;

        public DraftView(DraftManager draftManager)
        {
            this.draftManager = draftManager;
            this.draftManager.DraftCompleted += DraftManager_DraftCompleted;
            InitializeComponent();
            LoadPlayers();
        }

        private void LoadPlayers()
        {
            var sorted = draftManager.AvailablePlayers
                .OrderByDescending(p => p.Overall)
                .ToList();
            AvailablePlayersListBox.ItemsSource = sorted;
            AvailablePlayersListBox.SelectedIndex = 0;
            if (sorted.Count > 0)
            {
                SelectedPlayerText.Text = FormatPlayer(sorted[0]);
            }
            UpdatePickInfo();
            AvailablePlayersListBox.Focus();
        }

        private static string FormatPlayer(Player player)
        {
            return $"{player.FullName} - {player.Nationality} | Age: {player.Age} | Pos: {player.Position} | Overall: {player.Overall}";
        }

        private void UpdatePickInfo()
        {
            PickInfoText.Text = $"Pick {draftManager.CurrentPickNumber} - Team {draftManager.CurrentTeamIndex + 1}";
        }

        private void AvailablePlayersListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var player = AvailablePlayersListBox.SelectedItem as Player;
            if (player != null)
            {
                SelectedPlayerText.Text = FormatPlayer(player);
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
            var player = AvailablePlayersListBox.SelectedItem as Player;
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