using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Antiques_Auction_WebApp.Models;
namespace Antiques_Auction_WebApp.Data
{
    public class AuctionContext
    {
        private readonly IMongoDatabase _database = null;

        public AuctionContext(IOptions<DatabaseSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.DatabaseName);
        }
        public IMongoCollection<Bill> Bills
        {
            get
            {
                return _database.GetCollection<Bill>("Bills");
            }
        }
        public IMongoCollection<Bid> Bids
        {
            get
            {
                return _database.GetCollection<Bid>("Bids");
            }
        }
        public IMongoCollection<AntiqueItem> AntiqueItems
        {
            get
            {
                return _database.GetCollection<AntiqueItem>("AntiqueItems");
            }
        }
    }
}