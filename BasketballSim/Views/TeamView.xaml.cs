using System.Windows;
using System.Windows.Input;
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
            Keyboard.Focus(this);
            this.KeyDown += TeamView_KeyDown;
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

        private void TeamView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var result = MessageBox.Show("Exit?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            }
            else if (e.Key == Key.Right)
            {
                FranchiseContext.NextTeam();
                LoadTeam();
            }
            else if (e.Key == Key.Left)
            {
                FranchiseContext.PreviousTeam();
                LoadTeam();
            }
        }
    }
}
