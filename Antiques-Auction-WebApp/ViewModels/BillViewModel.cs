using System;

namespace Antiques_Auction_WebApp.ViewModels
{
    public class BillViewModel
    {
        public string Id { get; set; }
        public string Winner { get; set; }
        public string AntiqueItemId { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
