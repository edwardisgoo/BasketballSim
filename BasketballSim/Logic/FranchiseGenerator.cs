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

        public static List<Team> GenerateLeague(bool withPlayers = true)
        {
            var teams = new List<Team>();
            for (int i = 1; i <= 30; i++)
            {
                string teamName = $"Team {i}";
                var players = withPlayers ? GeneratePlayers(15) : new List<Player>();
                var team = new Team(teamName, players);
                teams.Add(team);
            }
            return teams;
        }

        public static List<Player> GeneratePlayersPool(int count)
        {
            var positions = new[] { "PG", "SG", "SF", "PF", "C" };
            int perPos = count / positions.Length;
            var players = new List<Player>();
            foreach (var pos in positions)
            {
                players.AddRange(GeneratePlayers(perPos, pos));
            }
            return players;
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

        private static double BaseRatingByAge(int age)
        {
            if (age < 20) return 60 + (age - 18) * 2;  // 60–64
            if (age <= 22) return 65 + (age - 20) * 3; // 65–71
            if (age <= 27) return 71 + (age - 23) * 2.5; // 71–81
            if (age <= 30) return 82 - (age - 28) * 1.5; // 82–79
            if (age <= 33) return 78 - (age - 31) * 2; // 78–74
            if (age <= 37) return 72 - (age - 34) * 2.5; // 72–64
            return 60; // past 38
        }

        private static int GenerateOverall(int age)
        {
            double baseValue = BaseRatingByAge(age);

            // Chaos factor: lower for middle-aged players, higher for young/old
            double chaos = age is >= 24 and <= 30 ? 3 : 6;

            // Normal distribution wiggle
            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                   Math.Sin(2.0 * Math.PI * u2);

            double raw = baseValue + chaos * randStdNormal;

            // Clamp to league rules
            int overall = (int)Math.Round(Math.Clamp(raw, 55, 95));

            // Chance for rare phenoms or ironmen
            if (rng.NextDouble() < 0.01 && age <= 22) // 1% chance for prodigy
                overall = rng.Next(82, 90);
            else if (rng.NextDouble() < 0.01 && age >= 34) // 1% for durability freak
                overall = rng.Next(82, 90);

            return overall;
        }

        private static int GenerateAge()
        {
            double mean = 26;
            double stdDev = 8;

            // Box-Muller transform
            double u1 = 1.0 - rng.NextDouble(); // avoid 0
            double u2 = 1.0 - rng.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                   Math.Sin(2.0 * Math.PI * u2);

            double rawValue = mean + stdDev * randStdNormal;

            // Clamp between 18 and 40
            int age = (int)Math.Round(Math.Clamp(rawValue, 18, 40));
            return age;
        }

        private static List<Player> GeneratePlayers(int count, string? position = null)
        {
            var players = new List<Player>();
            var pool = LoadNamePool();
            var weightSum = pool.countries.Sum(c => c.weight);

            for (int i = 0; i < count; i++)
            {
                var country = PickCountry(pool.countries, weightSum);
                var first = country.firstNames[rng.Next(country.firstNames.Count)];
                var last = country.lastNames[rng.Next(country.lastNames.Count)];
                int age = GenerateAge();
                int overall = GenerateOverall(age);
                var pos = position ?? GetRandomPosition();
                players.Add(new Player(first, last, country.name, overall, age, pos));
            }
            return players;
        }

        private static string GetRandomPosition()
        {
            string[] positions = { "PG", "SG", "SF", "PF", "C" };
            return positions[rng.Next(positions.Length)];
        }
    }
}
