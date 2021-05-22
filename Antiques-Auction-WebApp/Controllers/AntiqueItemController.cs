using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Antiques_Auction_WebApp.Models;
using Antiques_Auction_WebApp.Services;
using Antiques_Auction_WebApp.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Antiques_Auction_WebApp.Helpers;
using System.Globalization;

namespace Antiques_Auction_WebApp.Controllers
{
    public class AntiqueItemController : Controller
    {
        private readonly AntiqueItemService _antqSvc;
        private readonly BidService _bidSvc;
        private readonly BillService _billSvc;
        private readonly AutoBidConfigService _configSvc;
        private readonly NotificationService _notifSvc;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IMapper _mapper;
        private EmailService EmailService { get; set; }
        private ISession Session => _httpContextAccessor.HttpContext.Session;
        private readonly string _NotificationsSessionKey = "Notifications";
        private readonly string _notificationsCountSessionKey = "NotificationsCount";

        public AntiqueItemController(AntiqueItemService antiqueItemService, BidService bidService, AutoBidConfigService autoBidConfigService, BillService billService, NotificationService notificationService, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment, IMapper mapper, EmailService emailService)
        {
            _antqSvc = antiqueItemService;
            _bidSvc = bidService;
            _billSvc = billService;
            _configSvc = autoBidConfigService;
            _notifSvc = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
            EmailService = emailService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create(bool isSuccess = false, string itemId = "")
        {
            var viewModel = new AntiqueItemViewModel();
            ViewBag.IsSuccess = isSuccess;
            ViewBag.ItemId = itemId;
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AntiqueItemViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                AntiqueItem item = _mapper.Map<AntiqueItem>(viewModel);
                if (viewModel.Image != null)
                {
                    string folder = "AntiqueItemImages/";
                    item.ImageUrl = await UploadImage(folder, viewModel.Image);
                }
                item.BiddingClosed = false;
                string id = _antqSvc.Create(item);
                if (!string.IsNullOrEmpty(id))
                {
                    return RedirectToAction(nameof(Create), "AntiqueItem", new { isSuccess = true, itemId = id });
                }
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Update(string id)
        {
            AntiqueItem antiqueItem = _antqSvc.Find(id);
            AntiqueItemViewModel antiqueItemViewModel = _mapper.Map<AntiqueItemViewModel>(antiqueItem);
            return View(antiqueItemViewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AntiqueItemViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                AntiqueItem item = _mapper.Map<AntiqueItem>(viewModel);
                if (viewModel.Image != null)
                {
                    string folder = "AntiqueItemImages/";
                    item.ImageUrl = await UploadImage(folder, viewModel.Image);
                }
                _antqSvc.Update(item);
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Delete(string id)
        {
            _antqSvc.Delete(id);
            _bidSvc.DeleteByItemId(id);
            _billSvc.DeleteByItemId(id);
            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize(Roles = "Admin, Regular")]
        [HttpGet]
        [Route("AntiqueItems/Item/{id}", Name = "antiqueItemRoute")]
        public IActionResult Details(string id, bool isSuccess = false)
        {
            AutoBidConfig config = _configSvc.Find(User.Identity.Name);
            ViewBag.DisallowAutoBid = (config != null) ? false : true;
            ViewBag.IsSuccess = isSuccess;
            AntiqueItem item = _antqSvc.Find(id);
            AntiqueItemViewModel viewModel = _mapper.Map<AntiqueItemViewModel>(item);
            ViewBag.ViewModelId = viewModel.Id;
            List<NotificationViewModel> notifications = new List<NotificationViewModel>();
            notifications = _mapper.Map<List<NotificationViewModel>>(_notifSvc.Read(User.Identity.Name));
            Session.SetString(_NotificationsSessionKey, JsonConvert.SerializeObject(notifications));
            Session.SetInt32(_notificationsCountSessionKey, notifications.Count);
            return View(new BidViewModel());
        }
        public JsonResult GetItemDetails(string itemId)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            AntiqueItem item = _antqSvc.Find(itemId);
            CultureInfo ci = new CultureInfo("en-us");
            string message = "";
            if (item.BiddingClosed == false)
                message = $"Ends {item.AuctionCloseDateTime.ToString("f", CultureInfo.CreateSpecificCulture("en-US"))}";
            else
                message = $"Auction ended on {item.AuctionCloseDateTime.ToString("f", CultureInfo.CreateSpecificCulture("en-US"))}";
            AntiqueItemViewModel itemViewModel = _mapper.Map<AntiqueItemViewModel>(item);
            result.Add("itemViewModel", itemViewModel);
            result.Add("message", message);
            string remainingTime = item.AuctionCloseDateTime.ToString("dd-MM-yyyy h:mm:ss tt");
            result.Add("remainingTime", remainingTime);
            return Json(result);
        }
        public JsonResult GetBidUpdates(string itemId)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            int? highestBidOnItem = null;
            AntiqueItem item = _antqSvc.Find(itemId);
            result.Add("biddingClosed", item.BiddingClosed);
            highestBidOnItem = _bidSvc.GetHighestBidOnItem(itemId);
            var autoBidEnabled = false;
            if (item.BiddingClosed == true)
            {
                result.Add("winner", _bidSvc.GetWinningBid(itemId)?.Bidder??"no one");
            }
            else
            {
                int? minAmountAllowed = (highestBidOnItem != null) ? (highestBidOnItem + 1) : item.Price;
                int? maxAmountAllowed = null;
                bool notAllowedToBid = false;
                Bid oldBid = _bidSvc.GetBiddersBidOnItem(itemId, User.Identity.Name);
                if (oldBid != null)
                {
                    maxAmountAllowed = _bidSvc.GetHiggestBid() - 1;
                    if (minAmountAllowed > maxAmountAllowed)
                        notAllowedToBid = true;
                    result.Add("oldBidId", oldBid.Id);
                    autoBidEnabled = oldBid.AutoBiddingEnabled;
                }
                result.Add("minAmountAllowed", minAmountAllowed);
                result.Add("maxAmountAllowed", maxAmountAllowed);
                result.Add("notAllowedToBid", notAllowedToBid);
            }
            int? price = (highestBidOnItem != null) ? 0 : item.Price;
            result.Add("autoBidEnabled", autoBidEnabled);
            result.Add("highestBidOnItem", highestBidOnItem);
            result.Add("price", price);
            result.Add("bidViewModels", _mapper.Map<List<BidViewModel>>(_bidSvc.GetBidsForItem(itemId)));
            return Json(result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Regular")]
        public IActionResult CreateBid(BidViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                Bid bid = _mapper.Map<Bid>(viewModel);
                bid.State = State.InProgress;
                bid.CreatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(bid.Id))
                    _bidSvc.Update(bid);
                else
                    _bidSvc.Create(bid);
                var item = _antqSvc.Find(viewModel.AntiqueItemId);
                int result = AutoBid(item);
                if (result == 0) // if no autobidding done, then notify about the amount submitted
                    EmailService.NotifyBidders(item, viewModel.Amount);
                return RedirectToAction("Index", "Home", new { isSuccess = true });
            }
            return RedirectToAction("Index", "Home", new { isSuccess = false });
        }

        private int AutoBid(AntiqueItem item)
        {
            int result = 0;
            List<Bid> bids = _bidSvc.GetAutoBidsOnItem(item.Id);
            foreach (var bid in bids)
            {
                if (bid.Bidder != User.Identity.Name)
                {
                    int highestBid = (int)_bidSvc.GetHighestBidOnItem(item.Id);
                    var reserved = _bidSvc.GetReservedAmountByAutoBid(bid.Bidder);
                    var bidderConfig = _configSvc.Find(bid.Bidder);
                    CheckIfPassedAlertThreshold(bidderConfig, reserved);
                    if (highestBid + 1 > (bidderConfig.MaxBidAmount - reserved))
                    {
                        SendNotification("Insufficient Funds!", bidderConfig.UserName);
                        EmailService.NotifyAutoBidFailed(bid.Bidder, item);
                    }
                    else
                    {
                        bid.Amount = highestBid + 1;
                        bid.CreatedAt = DateTime.UtcNow;
                        _bidSvc.Update(bid);
                        result = 1; // autobid succeeded
                        EmailService.NotifyBidders(item, highestBid + 1);
                        if (highestBid + 1 == (bidderConfig.MaxBidAmount - reserved))
                            EmailService.NotifyTotalAmountBid(bid.Bidder, item, highestBid + 1);
                    }
                }
            }
            return result;
        }
        private void CheckIfPassedAlertThreshold(AutoBidConfig config, int reservedAmount)
        {
            bool crossedThreshold = ((reservedAmount / config.MaxBidAmount) * 100) >= config.AlertThreshold;
            if (crossedThreshold)
                SendNotification($"You have surpassed {config.AlertThreshold}% of maximum bid amount reserved for auto-bidding!", config.UserName);
        }
        private void SendNotification(string message, string toUser)
        {
            _notifSvc.Create(new Notification()
            {
                Message = message,
                UserName = toUser,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        private async Task<string> UploadImage(string folderPath, IFormFile file)
        {
            string imageName = Guid.NewGuid().ToString() + "_" + file.FileName;
            folderPath += imageName;
            string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folderPath);
            await file.CopyToAsync(new FileStream(serverFolder, FileMode.Create));
            return imageName;
        }
    }
}
