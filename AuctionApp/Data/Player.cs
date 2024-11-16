namespace AuctionApp.Data
{
    public class Player
    {
        public Player(string name, decimal cost)
        {
            Name = name;
            Cost = cost;
        }

        public string Name { get; set; }
        public decimal Cost { get; set; }
    }
}