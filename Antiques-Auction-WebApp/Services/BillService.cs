using System.Collections.Generic;
using Antiques_Auction_WebApp.Data;
using Antiques_Auction_WebApp.Models;
using MongoDB.Driver;
namespace Antiques_Auction_WebApp.Services
{
    public class BillService
    {
        private readonly IMongoCollection<Bill> _bills;

        public BillService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _bills = database.GetCollection<Bill>("Bills");
        }

        public Bill Create(Bill item)
        {
            _bills.InsertOne(item);
            return item;
        }

        public List<Bill> GetUserBills(string username) =>
            _bills.Find(b => b.Winner == username).SortByDescending(b => b.CreatedAt).ToList();
        public void DeleteByItemId(string itemId)
        {
            _bills.DeleteMany(b => b.AntiqueItemId == itemId);
        }
    }
}