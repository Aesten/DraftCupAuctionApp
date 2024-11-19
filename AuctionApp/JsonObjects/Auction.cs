using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace AuctionApp.JsonObjects
{
    public class Auction
    {
        [JsonProperty("type")]
        public string Type { get; set; } = @"Auction";
        
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("teamSize")]
        public int TeamSize { get; set; } = 6;

        [JsonProperty("players")]
        public List<Player> Players { get; set; } = new List<Player>();

        [JsonProperty("captains")]
        public List<Captain> Captains { get; set; } = new List<Captain>();
        
        public class Captain
        {
            [JsonProperty("name")]
            public string Name { get; set; } = string.Empty;

            [JsonProperty("budget")]
            public decimal Budget { get; set; } = 20;
        }
        
        public class Player
        {
            [JsonProperty("name")]
            public string Name { get; set; } = string.Empty;

            private List<string> _classes = new List<string> { "inf" };

            [JsonProperty("classes")]
            public List<string> Classes
            {
                get => _classes;
                set => _classes = value ?? new List<string> { "inf" };
            }
        }

        public static Auction Deserialize(string path)
        {
            try
            {
                var auction = JsonConvert.DeserializeObject<Auction>(File.ReadAllText(path));
                return auction.Type == "Auction" ? auction : null;
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
    }
}