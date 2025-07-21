using System.Windows;
using System.Linq;
using BasketballSim.Logic;
using BasketballSim.Models;

namespace BasketballSim.Views
{
    public partial class TeamView : Window
    {
        private Team currentTeam;

        public TeamView()
        {
            InitializeComponent();
            LoadTeam();
        }

        private void LoadTeam()
        {
            currentTeam = FranchiseContext.GetCurrentTeam();
            if (currentTeam == null) return;

            TeamNameText.Text = currentTeam.Name;

            var sortedPlayers = currentTeam.Players.OrderByDescending(p => p.Overall).ToList();
            PlayerListBox.ItemsSource = sortedPlayers;
            PlayerListBox.SelectedIndex = 0; // Default to best player
        }

        private void PlayerListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selected = PlayerListBox.SelectedItem as Player;
            if (selected != null)
            {
                SelectedPlayerText.Text = $"Overall: {selected.Overall}";
            }
        }

        private void NextTeam_Click(object sender, RoutedEventArgs e)
        {
            FranchiseContext.NextTeam();
            LoadTeam();
        }

        private void PreviousTeam_Click(object sender, RoutedEventArgs e)
        {
            FranchiseContext.PreviousTeam();
            LoadTeam();
        }
    }
}
