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

