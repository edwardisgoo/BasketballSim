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

        // 30 NBA-style team names: indices 0–14 = Western Conference, 15–29 = Eastern Conference
        public static readonly string[] TeamNames = new[]
        {
            // Western Conference
            "Los Angeles Lakers",
            "Golden State Warriors",
            "LA Clippers",
            "Phoenix Suns",
            "Sacramento Kings",
            "Portland Trail Blazers",
            "Utah Jazz",
            "Denver Nuggets",
            "Oklahoma City Thunder",
            "Dallas Mavericks",
            "Houston Rockets",
            "San Antonio Spurs",
            "Memphis Grizzlies",
            "New Orleans Pelicans",
            "Minnesota Timberwolves",
            // Eastern Conference
            "Boston Celtics",
            "Miami Heat",
            "New York Knicks",
            "Philadelphia 76ers",
            "Toronto Raptors",
            "Brooklyn Nets",
            "Cleveland Cavaliers",
            "Chicago Bulls",
            "Milwaukee Bucks",
            "Detroit Pistons",
            "Atlanta Hawks",
            "Washington Wizards",
            "Charlotte Hornets",
            "Orlando Magic",
            "Indiana Pacers",
        };

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
            for (int i = 0; i < 30; i++)
            {
                string teamName = TeamNames[i];
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
            if (age < 20) return 60 + (age - 18) * 2;   // 60–64
            if (age <= 22) return 65 + (age - 20) * 3;  // 65–71
            if (age <= 27) return 71 + (age - 23) * 2.5; // 71–81
            if (age <= 30) return 82 - (age - 28) * 1.5; // 82–79
            if (age <= 33) return 78 - (age - 31) * 2;   // 78–74
            if (age <= 37) return 72 - (age - 34) * 2.5; // 72–64
            return 60;
        }

        private static int GenerateOverall(int age)
        {
            double baseValue = BaseRatingByAge(age);
            double chaos = age is >= 24 and <= 30 ? 3 : 6;

            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                   Math.Sin(2.0 * Math.PI * u2);

            double raw = baseValue + chaos * randStdNormal;
            int overall = (int)Math.Round(Math.Clamp(raw, 55, 95));

            if (rng.NextDouble() < 0.01 && age <= 22)
                overall = rng.Next(82, 91);
            else if (rng.NextDouble() < 0.01 && age >= 34)
                overall = rng.Next(82, 91);

            return overall;
        }

        private static int GenerateAge()
        {
            double mean = 26;
            double stdDev = 8;

            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                   Math.Sin(2.0 * Math.PI * u2);

            double rawValue = mean + stdDev * randStdNormal;
            return (int)Math.Round(Math.Clamp(rawValue, 18, 40));
        }

        // Generate position-specific attributes so that:
        //   - Each position has a clear archetype (PG = speed/passing, C = rebounding/interior)
        //   - Attributes roughly centre on Overall with position-biased variance
        private static void AssignAttributes(Player player)
        {
            int ovr = player.Overall;

            int A(int bias, int spread)
            {
                double u1 = 1.0 - rng.NextDouble();
                double u2 = 1.0 - rng.NextDouble();
                double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                return Math.Clamp((int)Math.Round(ovr + bias + spread * z), 40, 99);
            }

            switch (player.Position)
            {
                case "PG":
                    player.Speed      = A(+8,  4);
                    player.Passing    = A(+10, 4);
                    player.IQ         = A(+6,  4);
                    player.Shooting   = A(+2,  5);
                    player.ThreePoint = A(0,   6);
                    player.Defense    = A(-8,  5);
                    player.Rebounding = A(-18, 4);
                    player.Interior   = A(-22, 4);
                    break;
                case "SG":
                    player.Speed      = A(+5,  4);
                    player.Shooting   = A(+10, 4);
                    player.ThreePoint = A(+8,  5);
                    player.IQ         = A(+2,  4);
                    player.Passing    = A(-5,  5);
                    player.Defense    = A(-5,  5);
                    player.Rebounding = A(-14, 4);
                    player.Interior   = A(-20, 4);
                    break;
                case "SF":
                    player.Speed      = A(+2,  5);
                    player.Shooting   = A(+3,  5);
                    player.ThreePoint = A(0,   6);
                    player.Defense    = A(+3,  5);
                    player.Rebounding = A(-4,  5);
                    player.Passing    = A(-4,  5);
                    player.Interior   = A(-8,  5);
                    player.IQ         = A(0,   5);
                    break;
                case "PF":
                    player.Speed      = A(-8,  4);
                    player.Shooting   = A(-6,  5);
                    player.ThreePoint = A(-18, 5);
                    player.Defense    = A(+8,  4);
                    player.Rebounding = A(+12, 4);
                    player.Interior   = A(+10, 4);
                    player.Passing    = A(-10, 4);
                    player.IQ         = A(0,   5);
                    break;
                case "C":
                default:
                    player.Speed      = A(-14, 4);
                    player.Shooting   = A(-18, 4);
                    player.ThreePoint = A(-28, 5);
                    player.Defense    = A(+10, 4);
                    player.Rebounding = A(+16, 4);
                    player.Interior   = A(+16, 4);
                    player.Passing    = A(-16, 4);
                    player.IQ         = A(+2,  5);
                    break;
            }
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
                var player = new Player(first, last, country.name, overall, age, pos);
                AssignAttributes(player);
                players.Add(player);
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
