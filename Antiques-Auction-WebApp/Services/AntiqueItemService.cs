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

        public AntiqueItem Create(AntiqueItem item)
        {
            _antiques.InsertOne(item);
            return item;
        }

        public IList<AntiqueItem> Read() =>
            _antiques.Find(item => true).ToList();

        public IList<AntiqueItem> GetItemsForSale() =>
            _antiques.Find(item => item.AuctionOpenDateTime <= DateTime.Now && item.AuctionCloseDateTime > DateTime.Now).ToList();

        public AntiqueItem Find(string id) =>
            _antiques.Find(item => item.Id == id).SingleOrDefault();

        public void Update(AntiqueItem item) =>
            _antiques.ReplaceOne(item => item.Id == item.Id, item);

        public void Delete(string id) =>
            _antiques.DeleteOne(item => item.Id == id);
    }
}