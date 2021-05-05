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

namespace Antiques_Auction_WebApp.Controllers
{
    public class AntiqueItemController : Controller
    {
        private readonly AntiqueItemService _antqSvc;
        private readonly BidService _bidSvc;
        private readonly AutoBidConfigService _configSvc;

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;

        public AntiqueItemController(AntiqueItemService antiqueItemService, BidService bidService, AutoBidConfigService autoBidConfigService, IWebHostEnvironment webHostEnvironment, IMapper mapper)
        {
            _antqSvc = antiqueItemService;
            _bidSvc = bidService;
            _configSvc = autoBidConfigService;
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
            return RedirectToAction("Index", "Dashboard");
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
            ViewBag.DisallowAutoBid = (config != null)?  false : true;
            ViewBag.IsSuccess = isSuccess;
            AntiqueItem item = _antqSvc.Find(id);
            AntiqueItemViewModel viewModel = _mapper.Map<AntiqueItemViewModel>(item);
            int? highestBidOnItem = _bidSvc.GetHighestBidOnItem(id);
            ViewBag.HighestBidOnItem = highestBidOnItem ?? null;
            ViewBag.MinAmountAllowed = (highestBidOnItem != null) ?  highestBidOnItem + 1 : item.Price + 1;
            ViewBag.MaxAmountAllowed = null;
            ViewBag.NotAllowedToBid = false;

            Bid oldBid = _bidSvc.GetBiddersBidOnItem(item.Id, User.Identity.Name);
            ViewBag.OldBidId = oldBid?.Id ?? null;
            if (oldBid != null)
            {
                ViewBag.MaxAmountAllowed = _bidSvc.GetHiggestBid() - 1;
                if (ViewBag.MinAmountAllowed > ViewBag.MaxAmountAllowed)
                    ViewBag.NotAllowedToBid = true;
            }
            ViewBag.ViewModel = new ItemDetailsViewModel()
            {
                AntiqueItemViewModel = viewModel,
                BidViewModels = _mapper.Map<List<BidViewModel>>(_bidSvc.GetBidsForItem(item.Id))
            };
            ViewData["RemainingTime"] = item.AuctionCloseDateTime.ToString("dd-MM-yyyy h:mm:ss tt");
            return View(new BidViewModel());
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
                {
                    _bidSvc.Update(bid);
                }
                else
                {
                    _bidSvc.Create(bid);
                }
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
                    if (highestBid + 1 <= (bidderConfig.MaxBidAmount - reserved))
                    {
                        bid.Amount = highestBid + 1;
                        bid.CreatedAt = DateTime.UtcNow;
                        _bidSvc.Update(bid);
                    }
                }
            }
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
