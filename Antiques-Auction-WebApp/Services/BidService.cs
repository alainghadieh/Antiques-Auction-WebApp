using System.Collections.Generic;
using Antiques_Auction_WebApp.Data;
using Antiques_Auction_WebApp.Models;
using MongoDB.Driver;
using System.Linq;
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

        public Bid GetLastBidForItem(string antiqueItemId) =>
            _bids.Find(b => b.AntiqueItemId == antiqueItemId).SortByDescending(b => b.CreatedAt).FirstOrDefault();
        public List<Bid> GetLosingBids(string antiqueItemId, string winner) =>
            _bids.Find(b => b.AntiqueItemId == antiqueItemId && b.Bidder != winner).ToList();

        public Bid GetBiddersBidOnItem(string itemId, string userName) =>
            _bids.Find(b => b.AntiqueItemId == itemId && b.Bidder == userName).SingleOrDefault();
        public int? GetHighestBidOnItem(string itemId) =>
            _bids.Find(b => b.AntiqueItemId == itemId).SortByDescending(b => b.Amount).FirstOrDefault()?.Amount;
        public Bid GetWinningBid(string itemId) =>
            _bids.AsQueryable().Where(b => b.State == Models.State.Won && b.AntiqueItemId == itemId).FirstOrDefault();

        public int GetHiggestBid() =>
            _bids.Aggregate()
                    .SortByDescending(b => b.Amount)
                    .Limit(1).ToList()[0].Amount;

        public List<Bid> GetAutoBidsOnItem(string itemId) =>
            _bids.Find(b => b.AntiqueItemId == itemId && b.AutoBiddingEnabled == true).ToList();

        public int GetReservedAmountByAutoBid(string bidder)
        {
            var bids = _bids.Find(b => b.Bidder == bidder && b.AutoBiddingEnabled == true).ToList();
            int sum = 0;
            foreach (var bid in bids)
            {
                sum += bid.Amount;
            }
            return sum;
        }
        public List<string>GetItemBidders(string itemId)
        {
            return _bids.AsQueryable().Where(x => x.AntiqueItemId == itemId).Select(x => x.Bidder).ToList();
        }
        public void Update(Bid bid) =>
            _bids.ReplaceOne(b => b.Id == bid.Id, bid);

        public void DeleteByItemId(string itemId)
        {
            _bids.DeleteMany(b => b.AntiqueItemId == itemId);
        }
    }
}