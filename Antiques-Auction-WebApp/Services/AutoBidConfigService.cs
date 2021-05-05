using System;
using Antiques_Auction_WebApp.Data;
using Antiques_Auction_WebApp.Models;
using Antiques_Auction_WebApp.Models.Entities;
using MongoDB.Driver;

namespace Antiques_Auction_WebApp.Services
{
    public class AutoBidConfigService
    {
        private readonly IMongoCollection<AutoBidConfig> _configurations;

        public AutoBidConfigService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _configurations = database.GetCollection<AutoBidConfig>("AutoBidConfigs");
        }

        public void Update(AutoBidConfig config)
        {
            _configurations.ReplaceOne(c => c.Id == config.Id, config);
        }

        public AutoBidConfig Create(AutoBidConfig config)
        {
            _configurations.InsertOne(config);
            return config;
        }

        public AutoBidConfig Find(string userName) =>
            _configurations.Find(c => c.UserName == userName).SingleOrDefault();

    }
}
