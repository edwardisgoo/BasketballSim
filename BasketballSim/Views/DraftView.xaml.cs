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
            InitializeComponent();
        }

        private void ViewTeamButton_Click(object sender, RoutedEventArgs e)
        {
            var teamView = new TeamView(draftManager.CurrentTeam);
            teamView.Show();
        }
    }
}