using System;
using System.Collections.Generic;
using System.Linq;
using BasketballSim.Models;

namespace BasketballSim.Logic
{
    /// <summary>
    /// Manages the draft process for a league.
    /// Keeps track of available players, team rosters and pick order.
    /// </summary>
    public class DraftManager
    {
        private readonly List<Player> availablePlayers;
        private readonly List<List<Player>> teamRosters = new();
        private int currentPick;
        private int currentTeamIndex;
        private int direction = 1; // 1 for forward, -1 for reverse

        public DraftManager(IEnumerable<Player> players)
        {
            availablePlayers = players.ToList();
            for (int i = 0; i < 30; i++)
            {
                teamRosters.Add(new List<Player>());
            }
        }

        public IReadOnlyList<Player> AvailablePlayers => availablePlayers;
        public int CurrentPickNumber => currentPick + 1;
        public int CurrentTeamIndex => currentTeamIndex;

        public IReadOnlyList<Player> GetTeamRoster(int teamIndex)
        {
            if (teamIndex < 0 || teamIndex >= teamRosters.Count)
                throw new ArgumentOutOfRangeException(nameof(teamIndex));
            return teamRosters[teamIndex];
        }

        public void PickPlayer(Player player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (!availablePlayers.Remove(player))
                throw new InvalidOperationException("Player not in available pool");

            teamRosters[currentTeamIndex].Add(player);
            AdvancePick();
        }

        /// <summary>
        /// Advance to the next pick in the serpentine order.
        /// Can be called manually to skip a pick.
        /// </summary>
        public void AdvancePick()
        {
            currentPick++;

            if (currentTeamIndex == teamRosters.Count - 1 && direction == 1)
                direction = -1;
            else if (currentTeamIndex == 0 && direction == -1)
                direction = 1;

            currentTeamIndex += direction;
        }
    }
}