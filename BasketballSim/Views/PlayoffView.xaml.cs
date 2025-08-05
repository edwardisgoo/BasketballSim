using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BasketballSim.Logic;
using BasketballSim.Models;

namespace BasketballSim.Views
{
    public partial class PlayoffView : Window
    {
        private readonly PlayoffSimulator simulator;
        private readonly List<List<TextBlock>> roundBlocks = new();

        public PlayoffView()
        {
            InitializeComponent();
            this.PreviewKeyDown += PlayoffView_KeyDown;
            var league = FranchiseContext.CurrentLeague ?? new List<Team>();
            simulator = new PlayoffSimulator(league);
            roundBlocks.Add(new List<TextBlock>());
            roundBlocks.Add(new List<TextBlock>());
            roundBlocks.Add(new List<TextBlock>());
            roundBlocks.Add(new List<TextBlock>());
            BuildBracket();
        }

        private void BuildBracket()
        {
            var r1 = simulator.Rounds[0];
            foreach (var s in r1)
            {
                var tbA = new TextBlock { Text = simulator.GetTeamName(s.TeamA), Margin = new Thickness(5) };
                var tbB = new TextBlock { Text = simulator.GetTeamName(s.TeamB), Margin = new Thickness(5) };
                Round1Panel.Children.Add(tbA);
                Round1Panel.Children.Add(tbB);
                roundBlocks[0].Add(tbA);
                roundBlocks[0].Add(tbB);
            }
            for (int i = 0; i < 4; i++)
            {
                var tb = new TextBlock { Margin = new Thickness(5) };
                Round2Panel.Children.Add(tb);
                roundBlocks[1].Add(tb);
            }
            for (int i = 0; i < 2; i++)
            {
                var tb = new TextBlock { Margin = new Thickness(5) };
                Round3Panel.Children.Add(tb);
                roundBlocks[2].Add(tb);
            }
            var champ = new TextBlock { Margin = new Thickness(5), FontWeight = FontWeights.Bold };
            ChampionPanel.Children.Add(champ);
            roundBlocks[3].Add(champ);
        }

        private void PlayoffView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !simulator.IsComplete)
            {
                simulator.SimulateNextGame();
                UpdateBracket();
            }
            else if (e.Key == Key.Escape)
            {
                var result = MessageBox.Show("Exit?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                    Application.Current.Shutdown();
            }
        }

        private void UpdateBracket()
        {
            for (int r = 0; r < simulator.Rounds.Count; r++)
            {
                var round = simulator.Rounds[r];
                for (int i = 0; i < round.Count; i++)
                {
                    var series = round[i];
                    if (series.IsComplete)
                    {
                        int nextRound = r + 1;
                        if (nextRound < roundBlocks.Count)
                        {
                            int slot = i / 2;
                            var tb = roundBlocks[nextRound][slot];
                            if (string.IsNullOrEmpty(tb.Text))
                            {
                                tb.Text = simulator.GetTeamName(series.WinnerIndex);
                            }
                        }
                    }
                }
            }
        }
    }
}