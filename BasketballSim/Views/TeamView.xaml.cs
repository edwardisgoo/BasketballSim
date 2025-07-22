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

        public TeamView() : this(FranchiseContext.GetCurrentTeam())
        {
        }

        public TeamView(Team team)
        {
            InitializeComponent();
            this.PreviewKeyDown += TeamView_KeyDown;
            LoadTeam(team);
        }

        private void LoadTeam(Team team)
        {
            currentTeam = team;
            if (currentTeam == null) return;

            TeamNameText.Text = currentTeam.Name;

            var sortedPlayers = currentTeam.Players.OrderByDescending(p => p.Overall).ToList();
            PlayerListBox.ItemsSource = sortedPlayers;
            PlayerListBox.SelectedIndex = 0; // Default to best player
            PlayerListBox.Focus();
            var first = sortedPlayers.FirstOrDefault();
            if (first != null)
            {
                SelectedPlayerText.Text = $"{first.FullName} - {first.Nationality} | Age: {first.Age} | Overall: {first.Overall}";
            }
        }

        private void PlayerListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selected = PlayerListBox.SelectedItem as Player;
            if (selected != null)
            {
                SelectedPlayerText.Text = $"{selected.FullName} - {selected.Nationality} | Age: {selected.Age} | Overall: {selected.Overall}";
            }
        }

        private void NextTeam_Click(object sender, RoutedEventArgs e)
        {
            FranchiseContext.NextTeam();
            LoadTeam(FranchiseContext.GetCurrentTeam());
        }

        private void PreviousTeam_Click(object sender, RoutedEventArgs e)
        {
            FranchiseContext.PreviousTeam();
            LoadTeam(FranchiseContext.GetCurrentTeam());
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
                LoadTeam(FranchiseContext.GetCurrentTeam());
            }
            else if (e.Key == Key.Left)
            {
                FranchiseContext.PreviousTeam();
                LoadTeam(FranchiseContext.GetCurrentTeam());
            }
        }

        private void BackToDraft_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
