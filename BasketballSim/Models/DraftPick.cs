namespace BasketballSim.Models
{
    public class DraftPick
    {
        public Player Player { get; }
        public int TeamIndex { get; }
        public int PickNumber { get; }

        public DraftPick(Player player, int teamIndex, int pickNumber)
        {
            Player = player;
            TeamIndex = teamIndex;
            PickNumber = pickNumber;
        }
    }
}