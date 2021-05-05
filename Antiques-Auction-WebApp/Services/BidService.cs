using System.Collections.Generic;
using Antiques_Auction_WebApp.Data;
using Antiques_Auction_WebApp.Models;
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
            _bids.Find(b => b.AntiqueItemId == antiqueItemId).SortByDescending(b => b.CreatedAt).ToList();

        public Bid GetBiddersBidOnItem(string itemId, string userName) =>
            _bids.Find(b => b.AntiqueItemId == itemId && b.Bidder == userName).SingleOrDefault();
        public int? GetHighestBidOnItem(string itemId) =>
            _bids.Find(b => b.AntiqueItemId == itemId).SortByDescending(b => b.Amount).FirstOrDefault()?.Amount;

        public int GetBiggestBid() =>
            _bids.Aggregate()
                    .SortByDescending(b => b.Amount)
                    .Limit(1).ToList()[0].Amount;

        public void Update(Bid bid) =>
            _bids.ReplaceOne(b => b.Id == bid.Id, bid);

        public void DeleteByItemId(string itemId)
        {
            _bids.DeleteMany(item => item.AntiqueItemId == itemId);
        }
    }
}