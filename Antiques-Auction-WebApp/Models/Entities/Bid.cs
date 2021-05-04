using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Antiques_Auction_WebApp.Models.Entities
{
    public class Bid
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Bidder { get; set; }
        public string AntiqueItemId { get; set; }
        [Required]
        public int Amount { get; set; }
        [Display(Name = "Allow Auto-Bidding")]
        public bool AutoBiddingEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
