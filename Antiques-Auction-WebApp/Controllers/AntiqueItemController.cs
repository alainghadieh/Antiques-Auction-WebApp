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

namespace Antiques_Auction_WebApp.Controllers
{
    public class AntiqueItemController : Controller
    {
        private readonly AntiqueItemService _antqSvc;
        private readonly BidService _bidSvc;
        private readonly AutoBidConfigService _configSvc;
        private readonly NotificationService _notifSvc;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IMapper _mapper;
        private ISession Session => _httpContextAccessor.HttpContext.Session;
        private readonly string _NotificationsSessionKey = "Notifications";
        private readonly string _notificationsCountSessionKey = "NotificationsCount";

        public AntiqueItemController(AntiqueItemService antiqueItemService, BidService bidService, AutoBidConfigService autoBidConfigService, NotificationService notificationService, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment, IMapper mapper)
        {
            _antqSvc = antiqueItemService;
            _bidSvc = bidService;
            _configSvc = autoBidConfigService;
            _notifSvc = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
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
            ViewBag.MaxAmountAllowed = null;
            ViewBag.NotAllowedToBid = false;
            ViewBag.BidCount = _bidSvc.GetBidsForItem(id).Count;
            Bid oldBid = _bidSvc.GetBiddersBidOnItem(item.Id, User.Identity.Name);
            ViewBag.OldBidId = oldBid?.Id ?? null;
            if (oldBid != null)
            {
                ViewBag.MaxAmountAllowed = _bidSvc.GetHiggestBid() - 1;
                if (ViewBag.MinAmountAllowed > ViewBag.MaxAmountAllowed)
                    ViewBag.NotAllowedToBid = true;
            }
            ViewBag.ViewModel = viewModel;
            ViewData["RemainingTime"] = item.AuctionCloseDateTime.ToString("dd-MM-yyyy h:mm:ss tt");
            List<NotificationViewModel> notifications = new List<NotificationViewModel>();
            notifications = _mapper.Map<List<NotificationViewModel>>(_notifSvc.Read(User.Identity.Name));
            Session.SetString(_NotificationsSessionKey, JsonConvert.SerializeObject(notifications));
            Session.SetInt32(_notificationsCountSessionKey, notifications.Count);
            return View(new BidViewModel());
        }
        public JsonResult GetBidUpdates(string itemId)
        {
            Dictionary<string,object> result = new Dictionary<string, object>();
            int? highestBidOnItem = _bidSvc.GetHighestBidOnItem(itemId);
            int? minAmountAllowed = (highestBidOnItem != null) ? (highestBidOnItem + 1) : null;
            result.Add("highestBidOnItem",highestBidOnItem);
            result.Add("minAmountAllowed",minAmountAllowed);
            var bidViewModels = _mapper.Map<List<BidViewModel>>(_bidSvc.GetBidsForItem(itemId));
            result.Add("bidViewModels",bidViewModels);
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
                bid.CreatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(bid.Id))
                    _bidSvc.Update(bid);
                else
                    _bidSvc.Create(bid);
                AutoBid(bid.AntiqueItemId);
                return RedirectToAction("Index", "Home", new { isSuccess = true });
            }
            return RedirectToAction("Index", "Home", new { isSuccess = false });
        }

        private void AutoBid(string itemId)
        {
            List<Bid> bids = _bidSvc.GetAutoBidsOnItem(itemId);
            foreach (var bid in bids)
            {
                if (bid.Bidder != User.Identity.Name)
                {
                    int highestBid = (int)_bidSvc.GetHighestBidOnItem(itemId);
                    var reserved = _bidSvc.GetReservedAmountByAutoBid(bid.Bidder);
                    var bidderConfig = _configSvc.Find(bid.Bidder);
                    CheckIfPassedAlertThreshold(bidderConfig, reserved);
                    if (highestBid + 1 <= (bidderConfig.MaxBidAmount - reserved))
                    {
                        bid.Amount = highestBid + 1;
                        bid.CreatedAt = DateTime.UtcNow;
                        _bidSvc.Update(bid);
                    }
                    else
                        SendNotification("Insufficient Funds!", bidderConfig.UserName);
                }
            }
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
