using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Antiques_Auction_WebApp.Models
{
    public enum State 
    {
        Lost,
        InProgress,
        Won
    }
    public class Bid
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Bidder { get; set; }
        public string AntiqueItemId { get; set; }
        public int Amount { get; set; }
        public bool AutoBiddingEnabled { get; set; }
        [JsonConverter(typeof(StringEnumConverter))] 
        [BsonRepresentation(BsonType.String)]         
        public State State { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedAt { get; set; }
    }
}
