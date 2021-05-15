using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Antiques_Auction_WebApp.Hubs
{
    public class BidHub : Hub
    {
        public async Task SendSignal()
        {
            await Clients.All.SendAsync("ReceiveSignal");
        }
    }
}