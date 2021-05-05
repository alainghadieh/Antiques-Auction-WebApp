using System;
using System.ComponentModel.DataAnnotations;

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
        public DateTime CreatedAt { get; set; }
    }
}
