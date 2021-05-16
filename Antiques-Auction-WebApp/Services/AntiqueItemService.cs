using System.Collections.Generic;
using Antiques_Auction_WebApp.Models;
using Antiques_Auction_WebApp.Data;
using MongoDB.Driver;
using System;

namespace Antiques_Auction_WebApp.Services
{
    public class AntiqueItemService
    {
        private readonly IMongoCollection<AntiqueItem> _antiques;

        public AntiqueItemService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _antiques = database.GetCollection<AntiqueItem>("AntiqueItems");
        }

        public void Update(AntiqueItem item)
        {
            _antiques.ReplaceOne(a => a.Id == item.Id, item);
        }

        public string Create(AntiqueItem item)
        {
            _antiques.InsertOne(item);
            return item.Id;
        }
        public IList<AntiqueItem> Read() =>
            _antiques.Find(item => true).SortByDescending(a => a.AuctionCloseDateTime).ToList();

        public List<AntiqueItem> GetItemsUpForBidding() =>
            _antiques.Find(item => item.BiddingClosed == false).ToList();

        public AntiqueItem Find(string id) =>
            _antiques.Find(item => item.Id == id).SingleOrDefault();

        public void Delete(string id) =>
            _antiques.DeleteOne(item => item.Id == id);
    }
}