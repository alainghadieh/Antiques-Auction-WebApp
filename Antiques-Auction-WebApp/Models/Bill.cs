using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Antiques_Auction_WebApp.Models
{
    public class Bill
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Winner { get; set; }
        public string AntiqueItemId { get; set; }
        public int Amount { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedAt { get; set; }
    }
}
