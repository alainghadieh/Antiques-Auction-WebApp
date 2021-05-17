using System.Collections.Generic;

namespace Antiques_Auction_WebApp.Interfaces
{
    public interface IAuctionRepository
    {
        IEnumerable<object> GetUserDetailedBids(string username);
        IEnumerable<object> GetUserAwards(string username);
    }
}