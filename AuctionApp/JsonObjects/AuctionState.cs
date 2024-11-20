using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace AuctionApp.JsonObjects
{
    public class AuctionState
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "AuctionState";

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("teamSize")]
        public int TeamSize { get; set; } = 6;

        [JsonProperty("halfBudgetDisplay")] 
        public bool HalfBudgetDisplay { get; set; } = true;

        public int AuctionedNumber => Teams.Sum(team => team.Members.Count);
        public int SkippedNumber => Skipped.Count;
        public int QueueNumber => PlayerQueue.Count;

        [JsonProperty("initialNumber")]
        public int InitialNumber { get; set; }

        [JsonProperty("playerQueue")]
        public List<Auction.Player> PlayerQueue { get; set; } = new List<Auction.Player>();

        [JsonProperty("skipped")]
        public List<Auction.Player> Skipped { get; set; } = new List<Auction.Player>();

        [JsonProperty("teams")]
        public List<Team> Teams { get; set; } = new List<Team>();

        public class Team
        {
            [JsonProperty("captain")]
            public string Captain { get; set; } = string.Empty;

            [JsonProperty("initialBudget")] 
            public decimal InitialBudget { get; set; }

            public decimal CurrentBudget => InitialBudget - Members.Sum(member => member.Cost);
            public decimal CurrentBudgetHalf => InitialBudget/2 - Members.Sum(member => member.Cost);

            [JsonProperty("members")]
            public List<PlayerMember> Members { get; set; } = new List<PlayerMember>();
        }

        public class PlayerMember
        {
            [JsonProperty("name")] 
            public string Name { get; set; } = string.Empty;

            [JsonProperty("cost")] 
            public decimal Cost { get; set; }

            [JsonProperty("classes")] 
            public List<string> Classes { get; set; } = new List<string>();
        }
        
        public static AuctionState Deserialize(string path)
        {
            try
            {
                var auctionState = JsonConvert.DeserializeObject<AuctionState>(File.ReadAllText(path));
                return auctionState.Type == "AuctionState" ? auctionState : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"An error occurred: {ex.Message}");
                return null;
            }
        }

        public void Serialize(string path)
        {
            try
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"An error occurred: {ex.Message}");
            }
        }

        public static AuctionState FromAuction(Auction auction)
        {
            var auctionState = new AuctionState
            {
                Title = auction.Title,
                TeamSize = auction.TeamSize,
                InitialNumber = auction.Players.Count,
                PlayerQueue = auction.Players
            };
            
            foreach (var captain in auction.Captains)
            {
                auctionState.Teams.Add(new Team
                {
                    Captain = captain.Name,
                    InitialBudget = captain.Budget
                });
            }
            
            Shuffle(auctionState.PlayerQueue);

            return auctionState;
        }
        
        private static void Shuffle<T>(List<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}