using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BasketballSim.Logic;
using BasketballSim.Models;

namespace BasketballSim.Views
{
    public partial class DraftSummary : Window
    {
        private readonly List<DraftPick> picks;

        public DraftSummary(List<DraftPick> picks)
        {
            InitializeComponent();
            this.picks = picks.OrderBy(p => p.PickNumber).ToList();
            LoadPlayers();
        }

        private void LoadPlayers()
        {
            var display = picks.Select(p => $"{p.PickNumber,3}. {p.Player.ShortName,-20} — {TeamNameFor(p.TeamIndex)}").ToList();
            PlayerListBox.ItemsSource = display;
            if (picks.Count > 0)
            {
                PlayerListBox.SelectedIndex = 0;
                SelectedPlayerText.Text = FormatPick(picks[0]);
                PlayerListBox.Focus();
            }
        }

        private static string FormatPick(DraftPick pick)
        {
            var pl = pick.Player;
            return $"{pick.PickNumber}. {pl.FullName}  —  {TeamNameFor(pick.TeamIndex)}\n" +
                   $"Age {pl.Age}  |  {pl.Position}  |  Ovr {pl.Overall}\n" +
                   $"Spd {pl.Speed}  Sht {pl.Shooting}  3PT {pl.ThreePoint}  " +
                   $"Def {pl.Defense}  Reb {pl.Rebounding}  Pas {pl.Passing}  " +
                   $"Int {pl.Interior}  IQ {pl.IQ}";
        }

        private static string TeamNameFor(int index) =>
            index >= 0 && index < FranchiseGenerator.TeamNames.Length
                ? FranchiseGenerator.TeamNames[index]
                : $"Team {index + 1}";

        private void PlayerListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int idx = PlayerListBox.SelectedIndex;
            if (idx >= 0 && idx < picks.Count)
                SelectedPlayerText.Text = FormatPick(picks[idx]);
        }

        private void Advance_Click(object sender, RoutedEventArgs e)
        {
            var teams = FranchiseContext.CurrentDraft?.GetTeams() ?? new List<Team>();
            FranchiseContext.CurrentLeague = teams;
            FranchiseContext.CurrentTeamIndex = 0;
            var teamView = new TeamView(teams.First());
            teamView.Show();
            this.Close();
        }
    }
}
