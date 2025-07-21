using System;
using System.Collections.Generic;
using BasketballSim.Models;

namespace BasketballSim.Logic
{
    public static class FranchiseGenerator
    {
        private static Random rng = new Random();

        public static List<Team> GenerateLeague()
        {
            var teams = new List<Team>();

            for (int i = 1; i <= 30; i++)
            {
                string teamName = $"Team {i}";
                var players = GeneratePlayers(15);
                var team = new Team(teamName, players);
                teams.Add(team);
            }

            return teams;
        }

        private static List<Player> GeneratePlayers(int count)
        {
            var players = new List<Player>();

            for (int i = 0; i < count; i++)
            {
                int overall = rng.Next(60, 100); // Overalls between 60–99
                players.Add(new Player(overall));
            }

            return players;
        }
    }
}
