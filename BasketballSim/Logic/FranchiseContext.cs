using System.Collections.Generic;
using BasketballSim.Models;

namespace BasketballSim.Logic
{
    public static class FranchiseContext
    {
        public static List<Team> CurrentLeague { get; set; }
        public static int CurrentTeamIndex { get; set; }

        public static Team GetCurrentTeam()
        {
            return CurrentLeague?[CurrentTeamIndex];
        }

        public static void NextTeam()
        {
            if (CurrentLeague == null) return;
            CurrentTeamIndex = (CurrentTeamIndex + 1) % CurrentLeague.Count;
        }

        public static void PreviousTeam()
        {
            if (CurrentLeague == null) return;
            CurrentTeamIndex = (CurrentTeamIndex - 1 + CurrentLeague.Count) % CurrentLeague.Count;
        }
    }
}
