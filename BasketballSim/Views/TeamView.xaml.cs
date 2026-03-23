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

        public TeamView() : this(FranchiseContext.GetCurrentTeam()) { }

        public TeamView(DraftManager manager)
        {
            draftManager = manager;
            teamIndex = manager.CurrentTeamIndex;
            InitializeComponent();
            this.PreviewKeyDown += TeamView_KeyDown;
            BackToDraftButton.Visibility     = Visibility.Visible;
            AdvanceToSeasonButton.Visibility = Visibility.Collapsed;
            LoadTeamFromDraft();
        }

        public TeamView(Team team)
        {
            InitializeComponent();
            this.PreviewKeyDown += TeamView_KeyDown;
            BackToDraftButton.Visibility     = Visibility.Collapsed;
            AdvanceToSeasonButton.Visibility = Visibility.Visible;
            LoadTeam(team);
        }

        // ──────────────────────────────────────────────────────────────
        // Sorting
        // ──────────────────────────────────────────────────────────────

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
            else
            {
                // Sort by an integer attribute property; default = Overall
                sorted = sortAscending
                    ? players.OrderBy(p => GetAttrValue(p, sortColumn))
                    : players.OrderByDescending(p => GetAttrValue(p, sortColumn));
            }

            var list = sorted.ToList();
            PlayerListView.ItemsSource = list;
            if (list.Count > 0)
                PlayerListView.SelectedIndex = 0;
        }

        private static int GetAttrValue(Player p, string col) => col switch
        {
            "Speed"      => p.Speed,
            "Shooting"   => p.Shooting,
            "ThreePoint" => p.ThreePoint,
            "Defense"    => p.Defense,
            "Rebounding" => p.Rebounding,
            "Passing"    => p.Passing,
            "Interior"   => p.Interior,
            "IQ"         => p.IQ,
            _            => p.Overall,
        };

        // ──────────────────────────────────────────────────────────────
        // Load helpers
        // ──────────────────────────────────────────────────────────────

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
                SelectedPlayerText.Text = FormatPlayer(first);
        }

        private void LoadTeamFromDraft()
        {
            if (draftManager == null) return;
            var roster = draftManager.GetTeamRoster(teamIndex);
            var name   = FranchiseGenerator.TeamNames.Length > teamIndex
                ? FranchiseGenerator.TeamNames[teamIndex]
                : $"Team {teamIndex + 1}";
            LoadTeam(new Team(name, roster.ToList()));
        }

        private static string FormatPlayer(Player p)
        {
            var s = p.SeasonStats;
            bool hasStats = s.GamesPlayed > 0;
            string attrLine = $"Spd {p.Speed}  Sht {p.Shooting}  3PT {p.ThreePoint}  " +
                              $"Def {p.Defense}  Reb {p.Rebounding}  Pas {p.Passing}  " +
                              $"Int {p.Interior}  IQ {p.IQ}";
            string statsLine = hasStats
                ? $"  |  {s.PPG} PPG  {s.RPG} RPG  {s.APG} APG  {s.SPG} SPG  {s.BPG} BPG  " +
                  $"FG {s.FGPct}%  3P {s.ThreePct}%  ({s.GamesPlayed} GP)"
                : string.Empty;
            return $"{p.FullName}  [{p.Nationality}]  Age {p.Age}  {p.Position}  Ovr {p.Overall}\n" +
                   attrLine + statsLine;
        }

        // ──────────────────────────────────────────────────────────────
        // Events
        // ──────────────────────────────────────────────────────────────

        private void PlayerListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PlayerListView.SelectedItem is Player p)
                SelectedPlayerText.Text = FormatPlayer(p);
        }

        private void PlayerHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is System.Windows.Controls.GridViewColumnHeader header && header.Column != null)
            {
                string col = header.Column.Header?.ToString() ?? string.Empty;
                col = col switch
                {
                    "Name" => "Name",
                    "Pos"  => "Position",
                    "Ovr"  => "Overall",
                    "Spd"  => "Speed",
                    "Sht"  => "Shooting",
                    "3PT"  => "ThreePoint",
                    "Def"  => "Defense",
                    "Reb"  => "Rebounding",
                    "Pas"  => "Passing",
                    "Int"  => "Interior",
                    "IQ"   => "IQ",
                    _      => sortColumn,
                };
                if (sortColumn == col) sortAscending = !sortAscending;
                else { sortColumn = col; sortAscending = false; }
                ApplySorting();
            }
        }

        private void NextTeam_Click(object sender, RoutedEventArgs e)   => NavigateTeam(+1);
        private void PreviousTeam_Click(object sender, RoutedEventArgs e) => NavigateTeam(-1);

        private void NavigateTeam(int dir)
        {
            if (draftManager != null)
            {
                int count = draftManager.GetTeams().Count;
                teamIndex = (teamIndex + dir + count) % count;
                LoadTeamFromDraft();
            }
            else
            {
                if (dir > 0) FranchiseContext.NextTeam(); else FranchiseContext.PreviousTeam();
                LoadTeam(FranchiseContext.GetCurrentTeam());
            }
        }

        private void TeamView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right: NavigateTeam(+1); break;
                case Key.Left:  NavigateTeam(-1); break;
                case Key.Escape:
                    if (MessageBox.Show("Exit?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        Application.Current.Shutdown();
                    break;
            }
        }

        private void BackToDraft_Click(object sender, RoutedEventArgs e)
        {
            if (draftManager != null) this.Close();
        }

        private void AdvanceToSeason_Click(object sender, RoutedEventArgs e)
        {
            var season = new SeasonView();
            season.Show();
            this.Close();
        }
    }
}
