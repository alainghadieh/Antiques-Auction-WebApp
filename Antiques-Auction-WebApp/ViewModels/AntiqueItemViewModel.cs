using System.Collections.Generic;
using Antiques_Auction_WebApp.Models;
using Antiques_Auction_WebApp.Models.Entities;

namespace Antiques_Auction_WebApp.ViewModels
{
    public class AntiqueItemViewModel
    {
        public AntiqueItem AntiqueItem { get;set; }
        public List<Bid> Bids { get;set; }
    }
}