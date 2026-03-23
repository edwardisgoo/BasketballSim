namespace BasketballSim.Models
{
    public class Player
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nationality { get; set; }
        public int Overall { get; set; }
        public int Age { get; set; }
        public string Position { get; set; }

        // Position-specific attributes (40–99)
        public int Speed { get; set; }
        public int Shooting { get; set; }
        public int ThreePoint { get; set; }
        public int Defense { get; set; }
        public int Rebounding { get; set; }
        public int Passing { get; set; }
        public int Interior { get; set; }
        public int IQ { get; set; }

        // Season statistics (accumulated over the current season)
        public PlayerSeasonStats SeasonStats { get; } = new PlayerSeasonStats();

        public string FullName => $"{FirstName} {LastName}";
        public string ShortName => $"{FirstName[0]}. {LastName}";

        public Player(string firstName, string lastName, string nationality, int overall, int age, string position)
        {
            FirstName = firstName;
            LastName = lastName;
            Nationality = nationality;
            Overall = overall;
            Age = age;
            Position = position;
        }
    }
}
