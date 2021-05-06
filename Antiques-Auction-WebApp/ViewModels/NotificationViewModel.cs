using System;

namespace Antiques_Auction_WebApp.ViewModels
{
    public class NotificationViewModel
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
