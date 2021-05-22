using Microsoft.AspNetCore.Identity.UI.Services;
using Antiques_Auction_WebApp.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Antiques_Auction_WebApp.Models;

namespace Antiques_Auction_WebApp.Helpers
{
    public class EmailService
    {
        public IEmailSender EmailSender { get; set; }
        public IConfiguration Configuration { get; }
        private readonly BidService _bidSvc;
        private readonly AntiqueItemService _antqSvc;
        private string User1Email { get; set; }
        private string User2Email { get; set; }
        public EmailService(IEmailSender emailSender, IConfiguration configuration, BidService bidService, AntiqueItemService antiqueItemService)
        {
            EmailSender = emailSender;
            _bidSvc = bidService;
            _antqSvc = antiqueItemService;
            Configuration = configuration;
            User1Email = Configuration["Emails:user1"];
            User2Email = Configuration["Emails:user2"];
        }
        public void NotifyBidders(AntiqueItem item, int amount)
        {
            List<string> bidders = _bidSvc.GetItemBidders(item.Id);
            foreach(var bidder in bidders)
            {
                Send(GetBidderEmail(bidder), "Bid alert", $"The highest bid is now ${amount} on {item.Name}");
            }
        }
        public void NotifyAutoBidFailed(string bidder, AntiqueItem item)
        {
            Send(GetBidderEmail(bidder), "AutoBid failed!", $"An attempted autobid on {item.Name} failed because it exceeds the maximum autobid amount.");
        }
        public void NotifyTotalAmountBid(string bidder, AntiqueItem item, int amount)
        {
            Send(GetBidderEmail(bidder), "Maximum AutoBid amount reached!", $"Your latest autobid on {item.Name} was just created and is at ${amount}. After this autobid, you no longer have enough funds for future autobids!");
        }
        public void NotifyWinner (string winner, string itemName)
        {
            Send(GetBidderEmail(winner), "Congratulations!", $"You have been awarded the {itemName}!");
        }
        public void NotifyItemAwarded (string losingBidder, string itemName, Bid winningBid)
        {
            Send(GetBidderEmail(losingBidder), $"The {itemName} was awarded!", $"The bidding time on {itemName} has finished and the item was awarded to {winningBid.Bidder} with ${winningBid.Amount}!");
        }
        private string GetBidderEmail(string bidder)
        {
            if (bidder == "user1")
            {
                return User1Email;
            }
            else if (bidder == "user2")
            {
                return User2Email;
            }
            return null;
        }
        private async void Send(string email, string subject, string message)
        {
            await EmailSender.SendEmailAsync(email, subject, message);
        }
    }
}