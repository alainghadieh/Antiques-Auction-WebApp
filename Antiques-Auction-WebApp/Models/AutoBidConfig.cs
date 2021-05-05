using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Antiques_Auction_WebApp.Models
{
    public class AutoBidConfig
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int MaxBidAmount { get; set; }
        public int AlertThreshold { get; set; }
        public string UserName { get; set; }
    }
}
