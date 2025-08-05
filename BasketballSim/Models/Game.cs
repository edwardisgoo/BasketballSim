namespace BasketballSim.Models
{
    public class Game
    {
        public int Day { get; set; }
        public int HomeTeamIndex { get; set; }
        public int AwayTeamIndex { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
    }
}