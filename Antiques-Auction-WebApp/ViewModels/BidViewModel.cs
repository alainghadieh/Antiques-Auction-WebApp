using System;
using System.ComponentModel.DataAnnotations;
using Antiques_Auction_WebApp.Models;

namespace Antiques_Auction_WebApp.ViewModels
{
    public class BidViewModel
    {
        public string Id { get; set; }
        public string Bidder { get; set; }
        public string AntiqueItemId { get; set; }
        [Required]
        public int Amount { get; set; }
        [Display(Name = "Allow Auto-Bidding")]
        public bool AutoBiddingEnabled { get; set; }
        public State State { get; set; }
        public string CreatedAt { get; set; }
    }
}
