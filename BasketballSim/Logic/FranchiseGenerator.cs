using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BasketballSim.Models;

namespace BasketballSim.Logic
{
    public static class FranchiseGenerator
    {
        private static readonly Random rng = new Random();
        private static NamePool? cachedPool;

        private static NamePool LoadNamePool()
        {
            if (cachedPool != null)
                return cachedPool;

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource", "namepool.json");
            if (!File.Exists(path))
            {
                cachedPool = new NamePool();
                return cachedPool;
            }

            var json = File.ReadAllText(path);
            cachedPool = JsonSerializer.Deserialize<NamePool>(json) ?? new NamePool();
            return cachedPool;
        }

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

        private static CountryNamePool PickCountry(List<CountryNamePool> countries, int totalWeight)
        {
            int roll = rng.Next(totalWeight);
            int cumulative = 0;
            foreach (var c in countries)
            {
                cumulative += c.weight;
                if (roll < cumulative)
                    return c;
            }
            return countries[0];
        }

        private static List<Player> GeneratePlayers(int count)
        {
            var players = new List<Player>();
            var pool = LoadNamePool();
            var weightSum = pool.countries.Sum(c => c.weight);

            for (int i = 0; i < count; i++)
            {
                var country = PickCountry(pool.countries, weightSum);
                var first = country.firstNames[rng.Next(country.firstNames.Count)];
                var last = country.lastNames[rng.Next(country.lastNames.Count)];
                int overall = rng.Next(60, 100); // Overalls between 60–99
                players.Add(new Player(first, last, country.name, overall));
            }
            return players;
        }
    }
}
