using System;

namespace Antiques_Auction_WebApp.ViewModels
{
    public class BillViewModel
    {
        public string Id { get; set; }
        public string Winner { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ItemName { get;set; }
        public string ItemDescription { get;set; }
        public string ImageUrl { get;set; }
        public int ItemPrice { get;set; }
    }
}
