using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using BasketballSim.Models;
using BasketballSim.Logic;

namespace BasketballSim
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += MainWindow_KeyDown;
        }

        private void StartFranchise_Click(object sender, RoutedEventArgs e)
        {
            // Generate the league
            List<Team> league = FranchiseGenerator.GenerateLeague();

            // Store it globally for now
            FranchiseContext.CurrentLeague = league;
            FranchiseContext.CurrentTeamIndex = 0;

            // Create a draft pool and draft manager
            var pool = FranchiseGenerator.GeneratePlayersPool(450);
            var draftManager = new DraftManager(pool);
            FranchiseContext.CurrentDraft = draftManager;

            // Navigate to draft view
            var draftView = new Views.DraftView(draftManager);
            draftView.Show();

            this.Close(); // Close the main menu
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var result = MessageBox.Show("Exit?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }
}
