using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Antiques_Auction_WebApp.Services;
using Antiques_Auction_WebApp.Helpers;

namespace Antiques_Auction_WebApp.BackgroundTasks
{
    public class TimedDeadlineMonitorService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly AntiqueItemService _antqSvc;
        private readonly BidService _bidSvc;
        private readonly BillService _billSvc;
        private EmailService EmailService { get; set; }

        private readonly ILogger<TimedDeadlineMonitorService> _logger;
        private Timer _timer;

        public TimedDeadlineMonitorService(AntiqueItemService antiqueItemService, BidService bidService, BillService billService, EmailService emailService, ILogger<TimedDeadlineMonitorService> logger)
        {
            _antqSvc = antiqueItemService;
            _bidSvc = bidService;
            _billSvc = billService;
            EmailService = emailService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(BiddingDeadlineMonitor, null, TimeSpan.Zero, 
                TimeSpan.FromSeconds(60));

            return Task.CompletedTask;
        }

        private void BiddingDeadlineMonitor(object state)
        {
            var count = Interlocked.Increment(ref executionCount);
            var items = _antqSvc.GetItemsUpForBidding();
            foreach (var item in items)
            {
                if (DateTime.Now >= item.AuctionCloseDateTime)
                {
                    var winningBid = _bidSvc.GetLastBidForItem(item.Id);
                    winningBid.State = Models.State.Won;
                    winningBid.AutoBiddingEnabled = false;
                    _bidSvc.Update(winningBid);
                    var losingBids = _bidSvc.GetLosingBids(item.Id, winningBid.Bidder);
                    item.BiddingClosed = true;
                    _antqSvc.Update(item);
                    if (winningBid != null)
                    {
                        var bill = new Models.Bill();
                        bill.Winner = winningBid.Bidder;
                        bill.Amount = winningBid.Amount;
                        bill.AntiqueItemId = item.Id;
                        bill.CreatedAt = DateTime.Now;
                        _billSvc.Create(bill);
                        EmailService.NotifyWinner(winningBid.Bidder, item.Name);
                        foreach(var bid in losingBids)
                        {
                            bid.State = Models.State.Lost;
                            bid.AutoBiddingEnabled = false;
                            _bidSvc.Update(bid);
                            EmailService.NotifyItemAwarded(bid.Bidder, item.Name, winningBid);
                        }
                    }
                }
            }
            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}