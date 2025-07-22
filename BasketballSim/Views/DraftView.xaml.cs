using System.Windows;
using BasketballSim.Logic;

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
        }

        private void DraftManager_DraftCompleted(object? sender, EventArgs e)
        {
            var teams = draftManager.GetTeams();
            FranchiseContext.CurrentLeague = teams;
            FranchiseContext.CurrentTeamIndex = 0;
            var teamView = new TeamView(teams[0]);
            teamView.Show();
            this.Close();
        }

        private void ViewTeamButton_Click(object sender, RoutedEventArgs e)
        {
            var teamView = new TeamView(draftManager.CurrentTeam);
            teamView.Show();
        }
    }
}