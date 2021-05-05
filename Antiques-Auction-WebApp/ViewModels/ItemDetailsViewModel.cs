using System.Collections.Generic;

namespace Antiques_Auction_WebApp.ViewModels
{
    public class ItemDetailsViewModel
    {
        public AntiqueItemViewModel AntiqueItemViewModel { get;set; }
        public List<BidViewModel> BidViewModels { get;set; }
    }
}