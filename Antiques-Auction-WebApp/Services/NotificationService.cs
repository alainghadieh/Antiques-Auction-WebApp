using System.Collections.Generic;
using Antiques_Auction_WebApp.Models;
using Antiques_Auction_WebApp.Data;
using MongoDB.Driver;

namespace Antiques_Auction_WebApp.Services
{
    public class NotificationService
    {
        private readonly IMongoCollection<Notification> _notifications;

        public NotificationService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _notifications = database.GetCollection<Notification>("Notifications");
        }

        public string Create(Notification notification)
        {
            _notifications.InsertOne(notification);
            return notification.Id;
        }

        public void UpdateRead(Notification notification)
        {
            notification.IsRead = true;
            _notifications.ReplaceOne(n => n.Id == notification.Id, notification);
        }

        public List<Notification> Read(string userName) =>
            _notifications.Find(n => n.UserName == userName && n.IsRead == false).SortByDescending(n => n.CreatedAt).ToList();

        public Notification Find(string id) =>
            _notifications.Find(n => n.Id == id).SingleOrDefault();
    }
}