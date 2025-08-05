using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using BasketballSim.Logic;
using BasketballSim.Models;

namespace BasketballSim.Views
{
    public partial class TeamView : Window
    {
        private Team currentTeam;
        private readonly DraftManager? draftManager;
        private int teamIndex;
        private List<Player> players = new();
        private string sortColumn = "Overall";
        private bool sortAscending = false;

        public TeamView() : this(FranchiseContext.GetCurrentTeam())
        {
        }

        public TeamView(DraftManager manager)
        {
            draftManager = manager;
            teamIndex = manager.CurrentTeamIndex;
            InitializeComponent();
            this.PreviewKeyDown += TeamView_KeyDown;
            BackToDraftButton.Visibility = Visibility.Visible;
            AdvanceToSeasonButton.Visibility = Visibility.Collapsed;
            LoadTeamFromDraft();
        }

        public TeamView(Team team)
        {
            InitializeComponent();
            this.PreviewKeyDown += TeamView_KeyDown;
            BackToDraftButton.Visibility = Visibility.Collapsed;
            AdvanceToSeasonButton.Visibility = Visibility.Visible;
            LoadTeam(team);
        }

        private void ApplySorting()
        {
            IEnumerable<Player> sorted;
            if (sortColumn == "Name")
            {
                sorted = sortAscending
                    ? players.OrderBy(p => p.ShortName)
                    : players.OrderByDescending(p => p.ShortName);
            }
            else if (sortColumn == "Position")
            {
                string[] order = { "PG", "SG", "SF", "PF", "C" };
                sorted = sortAscending
                    ? players.OrderBy(p => System.Array.IndexOf(order, p.Position)).ThenByDescending(p => p.Overall)
                    : players.OrderByDescending(p => System.Array.IndexOf(order, p.Position)).ThenByDescending(p => p.Overall);
            }
            else // Overall
            {
                sorted = sortAscending
                    ? players.OrderBy(p => p.Overall)
                    : players.OrderByDescending(p => p.Overall);
            }

            var list = sorted.ToList();
            PlayerListView.ItemsSource = list;
            if (list.Count > 0)
                PlayerListView.SelectedIndex = 0;
        }

        private void LoadTeam(Team team)
        {
            currentTeam = team;
            if (currentTeam == null) return;

            TeamNameText.Text = currentTeam.Name;

            players = currentTeam.Players.ToList();
            ApplySorting();
            PlayerListView.Focus();
            var first = players.FirstOrDefault();
            if (first != null)
            {
                SelectedPlayerText.Text = $"{first.FullName} - {first.Nationality} | Age: {first.Age} | Pos: {first.Position} | Overall: {first.Overall}";
            }
        }

        private void LoadTeamFromDraft()
        {
            if (draftManager == null) return;
            var roster = draftManager.GetTeamRoster(teamIndex);
            var team = new Team($"Team {teamIndex + 1}", roster.ToList());
            LoadTeam(team);
        }

        private void PlayerListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selected = PlayerListView.SelectedItem as Player;
            if (selected != null)
            {
                SelectedPlayerText.Text = $"{selected.FullName} - {selected.Nationality} | Age: {selected.Age} | Pos: {selected.Position} | Overall: {selected.Overall}";
            }
        }

        private void PlayerHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is System.Windows.Controls.GridViewColumnHeader header && header.Column != null)
            {
                string column = header.Column.Header.ToString() ?? string.Empty;
                column = column switch
                {
                    "Name" => "Name",
                    "Pos" => "Position",
                    "Ovr" => "Overall",
                    _ => sortColumn
                };

                if (sortColumn == column)
                    sortAscending = !sortAscending;
                else
                {
                    sortColumn = column;
                    sortAscending = true;
                }
                ApplySorting();
            }
        }

        private void NextTeam_Click(object sender, RoutedEventArgs e)
        {
            if (draftManager != null)
            {
                var count = draftManager.GetTeams().Count;
                teamIndex = (teamIndex + 1) % count;
                LoadTeamFromDraft();
            }
            else
            {
                FranchiseContext.NextTeam();
                LoadTeam(FranchiseContext.GetCurrentTeam());
            }
        }

        private void PreviousTeam_Click(object sender, RoutedEventArgs e)
        {
            if (draftManager != null)
            {
                var count = draftManager.GetTeams().Count;
                teamIndex = (teamIndex - 1 + count) % count;
                LoadTeamFromDraft();
            }
            else
            {
                FranchiseContext.PreviousTeam();
                LoadTeam(FranchiseContext.GetCurrentTeam());
            }
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
                if (draftManager != null)
                {
                    var count = draftManager.GetTeams().Count;
                    teamIndex = (teamIndex + 1) % count;
                    LoadTeamFromDraft();
                }
                else
                {
                    FranchiseContext.NextTeam();
                    LoadTeam(FranchiseContext.GetCurrentTeam());
                }
            }
            else if (e.Key == Key.Left)
            {
                if (draftManager != null)
                {
                    var count = draftManager.GetTeams().Count;
                    teamIndex = (teamIndex - 1 + count) % count;
                    LoadTeamFromDraft();
                }
                else
                {
                    FranchiseContext.PreviousTeam();
                    LoadTeam(FranchiseContext.GetCurrentTeam());
                }
            }
        }

        private void BackToDraft_Click(object sender, RoutedEventArgs e)
        {
            if (draftManager != null)
            {
                this.Close();
            }
        }

        private void AdvanceToSeason_Click(object sender, RoutedEventArgs e)
        {
            var season = new SeasonView();
            season.Show();
            this.Close();
        }
    }
}
