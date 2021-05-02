using System.Collections.Generic;
using Antiques_Auction_WebApp.Models;
using Antiques_Auction_WebApp.Data;
using MongoDB.Driver;

namespace Antiques_Auction_WebApp.Services
{
    public class BidService
    {
        private readonly IMongoCollection<Bid> _bids;

        public BidService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _bids = database.GetCollection<Bid>("Bids");
        }

        public Bid Create(Bid item)
        {
            _bids.InsertOne(item);
            return item;
        }

        public List<Bid> GetBidsForItem(string antiqueItemId) =>
            _bids.Find(b => b.AntiqueItemId == antiqueItemId).ToList();

        public Bid Find(string id) =>
            _bids.Find(b => b.Id == id).SingleOrDefault();

        public void Update(Bid bid) =>
            _bids.ReplaceOne(b => b.Id == bid.Id, bid);
    }
}