using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BasketballSim.Views
{
    public partial class StandingsView : Window
    {
        private readonly List<(int TeamIndex, int Wins)> standings;

        public StandingsView(List<(int TeamIndex, int Wins)> standings)
        {
            InitializeComponent();
            this.standings = standings;
            PopulateStandings();
        }

        private void PopulateStandings()
        {
            var west = standings.Where(s => s.TeamIndex < 15).ToList();
            var east = standings.Where(s => s.TeamIndex >= 15).ToList();

            for (int i = 0; i < west.Count; i++)
            {
                WestList.Items.Add($"{i + 1}. Team {west[i].TeamIndex + 1} {west[i].Wins}-{82 - west[i].Wins}");
            }

            for (int i = 0; i < east.Count; i++)
            {
                EastList.Items.Add($"{i + 1}. Team {east[i].TeamIndex + 1} {east[i].Wins}-{82 - east[i].Wins}");
            }
        }

        private void ViewPlayoffs_Click(object sender, RoutedEventArgs e)
        {
            var bracket = new PlayoffView();
            bracket.Show();
        }
    }
}