using System;
using System.ComponentModel.DataAnnotations;

namespace Antiques_Auction_WebApp.ViewModels
{
    public class AutoBidConfigViewModel
    {
        public string Id { get; set; }
        [Display(Name = "Maximum amount for auto-bidding"), Required]
        public int MaxBidAmount { get; set; }
        [Display(Name = "Percentage of the maximum bid amount reserved"), Required]
        [Range(typeof(int), "0", "100", ErrorMessage = "Threshold can only be between 0 and 100")]
        public int AlertThreshold { get; set; }
        public string UserName { get; set; }
    }
}