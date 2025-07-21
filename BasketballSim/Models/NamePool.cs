namespace BasketballSim.Models
{
    public class NamePool
    {
        public List<CountryNamePool> countries { get; set; } = new();
    }

    public class CountryNamePool
    {
        public string name { get; set; }
        public int weight { get; set; }
        public List<string> firstNames { get; set; } = new();
        public List<string> lastNames { get; set; } = new();
    }
}