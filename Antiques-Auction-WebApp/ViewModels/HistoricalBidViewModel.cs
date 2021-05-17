using System;
using Antiques_Auction_WebApp.Models;

namespace Antiques_Auction_WebApp.ViewModels
{
    public class HistoricalBidViewModel
    {
        public int Amount { get; set; }
        public State State{ get; set; }
        public DateTime CreatedAt { get; set; }
        public string ItemName { get;set; }
        public string ImageUrl { get;set; }
    }
}
