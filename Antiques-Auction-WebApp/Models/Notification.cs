using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Antiques_Auction_WebApp.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
        public bool IsRead { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedAt { get; set; }
    }
}
