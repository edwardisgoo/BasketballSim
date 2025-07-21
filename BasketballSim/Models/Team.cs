using System.Collections.Generic;
using System.Linq;

namespace BasketballSim.Models
{
    public class Team
    {
        public string Name { get; set; }
        public List<Player> Players { get; set; }

        public Team(string name, List<Player> players)
        {
            Name = name;
            Players = players.OrderByDescending(p => p.Overall).ToList();
        }

        public Player GetBestPlayer() => Players.FirstOrDefault();
    }
}
