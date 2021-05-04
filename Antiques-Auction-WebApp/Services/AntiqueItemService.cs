using System.Collections.Generic;
using Antiques_Auction_WebApp.Models;
using Antiques_Auction_WebApp.Data;
using MongoDB.Driver;
using System;
using Antiques_Auction_WebApp.Models.Entities;

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

        public void Update(AntiqueItemModel item)
        {
            AntiqueItem antiqueItem = new AntiqueItem()
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                ImageUrl = item.ImageUrl,
                AuctionOpenDateTime = item.AuctionOpenDateTime,
                AuctionCloseDateTime = item.AuctionCloseDateTime,
                LastUpdatedAt = DateTime.UtcNow
            };
            _antiques.ReplaceOne(a => a.Id == item.Id, antiqueItem);
        }

        public string Create(AntiqueItemModel item)
        {
            AntiqueItem antiqueItem = new AntiqueItem()
            {
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                ImageUrl = item.ImageUrl,
                AuctionOpenDateTime = item.AuctionOpenDateTime,
                AuctionCloseDateTime = item.AuctionCloseDateTime,
                CreatedAt = DateTime.UtcNow
            };
            _antiques.InsertOne(antiqueItem);
            return antiqueItem.Id;
        }
        public IList<AntiqueItem> Read() =>
            _antiques.Find(item => true).ToList();

        public List<AntiqueItem> GetItemsForSale() =>
            _antiques.Find(item => item.AuctionOpenDateTime <= DateTime.Now && item.AuctionCloseDateTime > DateTime.Now).ToList();

        public AntiqueItem Find(string id) =>
            _antiques.Find(item => item.Id == id).SingleOrDefault();

        public void Delete(string id) =>
            _antiques.DeleteOne(item => item.Id == id);
    }
}