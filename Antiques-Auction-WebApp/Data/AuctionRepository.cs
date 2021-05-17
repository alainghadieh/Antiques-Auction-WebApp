using System.Collections.Generic;
using Antiques_Auction_WebApp.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using System.Linq;
using Antiques_Auction_WebApp.ViewModels;

namespace Antiques_Auction_WebApp.Data
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AuctionContext _context = null;
        public AuctionRepository(IOptions<DatabaseSettings> settings)
        {
            _context = new AuctionContext(settings);
        }
        public IEnumerable<object> GetUserDetailedBids(string username)
        {
            var results = from bid in _context.Bids.AsQueryable()
                        join antiqueItem in _context.AntiqueItems
                            on bid.AntiqueItemId equals antiqueItem.Id
                        into DetailedBids
                        where (bid.Bidder == username)
                        select new HistoricalBidViewModel
                        {
                            Amount = bid.Amount,
                            State = bid.State,
                            CreatedAt = bid.CreatedAt,
                            ItemName = DetailedBids.First().Name,
                            ImageUrl = DetailedBids.First().ImageUrl
                        };
            return results;
        }
        public IEnumerable<object> GetUserAwards(string username)
        {
            var results = from b in _context.Bills.AsQueryable()
                        join a in _context.AntiqueItems
                            on b.AntiqueItemId equals a.Id
                        into DetailedBills
                        where (b.Winner == username)
                        select new BillViewModel
                        {
                            Id = b.Id,
                            Amount = b.Amount,
                            Winner = b.Winner,
                            CreatedAt = b.CreatedAt,
                            ImageUrl = DetailedBills.First().ImageUrl,
                            ItemName = DetailedBills.First().Name,
                            ItemDescription = DetailedBills.First().Description,
                            ItemPrice = DetailedBills.First().Price
                        };
            return results;
        }
    }
}