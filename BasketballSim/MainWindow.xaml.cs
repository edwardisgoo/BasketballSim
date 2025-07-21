using System.Collections.Generic;
using System.Windows;
using BasketballSim.Models;
using BasketballSim.Logic;

namespace BasketballSim
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartFranchise_Click(object sender, RoutedEventArgs e)
        {
            // Generate the league
            List<Team> league = FranchiseGenerator.GenerateLeague();

            // Store it globally for now
            FranchiseContext.CurrentLeague = league;
            FranchiseContext.CurrentTeamIndex = 0;

            // Navigate to team view
            var teamView = new Views.TeamView(); // We'll create this next
            teamView.Show();

            this.Close(); // Close the main menu
        }
    }
}
