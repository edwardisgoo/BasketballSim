using System;
using System.Collections.Generic;
using System.Linq;
using BasketballSim.Models;

namespace BasketballSim.Logic
{
    /// <summary>
    /// Manages the serpentine draft process for a league.
    /// </summary>
    public class DraftManager
    {
        public event EventHandler? DraftCompleted;

        private readonly List<Player> availablePlayers;
        private readonly List<List<Player>> teamRosters = new();
        private readonly List<DraftPick> draftHistory = new();
        private int currentPick;
        private int currentTeamIndex;
        private int direction = 1; // 1 = forward, -1 = reverse (serpentine)

        public bool IsDraftComplete => availablePlayers.Count == 0;

        public DraftManager(IEnumerable<Player> players)
        {
            availablePlayers = players.ToList();
            for (int i = 0; i < 30; i++)
                teamRosters.Add(new List<Player>());
        }

        public IReadOnlyList<Player> AvailablePlayers => availablePlayers;
        public int CurrentPickNumber  => currentPick + 1;
        public int CurrentTeamIndex   => currentTeamIndex;

        public IReadOnlyList<Player> CurrentTeamRoster => teamRosters[currentTeamIndex];

        public Team CurrentTeam =>
            new Team(TeamNameFor(currentTeamIndex), teamRosters[currentTeamIndex]);

        public IReadOnlyList<Player> GetTeamRoster(int teamIndex)
        {
            if (teamIndex < 0 || teamIndex >= teamRosters.Count)
                throw new ArgumentOutOfRangeException(nameof(teamIndex));
            return teamRosters[teamIndex];
        }

        public List<Team> GetTeams()
        {
            var teams = new List<Team>();
            for (int i = 0; i < teamRosters.Count; i++)
                teams.Add(new Team(TeamNameFor(i), teamRosters[i].ToList()));
            return teams;
        }

        public IReadOnlyList<DraftPick> DraftHistory => draftHistory;

        public void PickPlayer(Player player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (!availablePlayers.Remove(player))
                throw new InvalidOperationException("Player not in available pool");

            int pickNumber = currentPick + 1;
            teamRosters[currentTeamIndex].Add(player);
            draftHistory.Add(new DraftPick(player, currentTeamIndex, pickNumber));

            if (availablePlayers.Count == 0)
                DraftCompleted?.Invoke(this, EventArgs.Empty);
            else
                AdvancePick();
        }

        public void AdvancePick()
        {
            currentPick++;

            if (currentTeamIndex == teamRosters.Count - 1 && direction == 1)
                direction = -1;
            else if (currentTeamIndex == 0 && direction == -1)
                direction = 1;
            else
                currentTeamIndex += direction;
        }

        // ──────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────

        private static string TeamNameFor(int index)
        {
            // Prefer real team names from the generator; fall back if out of range
            if (index >= 0 && index < FranchiseGenerator.TeamNames.Length)
                return FranchiseGenerator.TeamNames[index];
            return $"Team {index + 1}";
        }
    }
}
