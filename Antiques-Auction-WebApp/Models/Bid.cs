using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Antiques_Auction_WebApp.Models
{
    public class Bid
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Bidder { get; set; }
        public string AntiqueItemId { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
