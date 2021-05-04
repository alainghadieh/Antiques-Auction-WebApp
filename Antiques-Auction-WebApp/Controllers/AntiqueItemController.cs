using System;
using System.IO;
using System.Threading.Tasks;
using Antiques_Auction_WebApp.Models;
using Antiques_Auction_WebApp.Models.Entities;
using Antiques_Auction_WebApp.Services;
using Antiques_Auction_WebApp.ViewModels;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AntiqueItemController(AntiqueItemService antiqueItemService, BidService bidService, IWebHostEnvironment webHostEnvironment)
        {
            _antqSvc = antiqueItemService;
            _bidSvc = bidService;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create(bool isSuccess = false, int itemId = 0)
        {
            var model = new AntiqueItemModel();
            ViewBag.IsSuccess = isSuccess;
            ViewBag.ItemId = itemId;
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AntiqueItemModel antiqueItemModel)
        {
            if (ModelState.IsValid)
            {
                if (antiqueItemModel.Image != null)
                {
                    string folder = "AntiqueItemImages/";
                    antiqueItemModel.ImageUrl = await UploadImage(folder, antiqueItemModel.Image);
                }

                string id = _antqSvc.Create(antiqueItemModel);
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
            AntiqueItemModel antiqueItemModel = new AntiqueItemModel()
            {
                Id = antiqueItem.Id,
                Name = antiqueItem.Name,
                Description = antiqueItem.Description,
                Price = antiqueItem.Price,
                ImageUrl = antiqueItem.ImageUrl,
                AuctionOpenDateTime = antiqueItem.AuctionOpenDateTime,
                AuctionCloseDateTime = antiqueItem.AuctionCloseDateTime,
                CreatedAt = antiqueItem.CreatedAt
            };
            return View(antiqueItemModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AntiqueItemModel antiqueItemModel)
        {
            if (ModelState.IsValid)
            {
                if (antiqueItemModel.Image != null)
                {
                    string folder = "AntiqueItemImages/";
                    antiqueItemModel.ImageUrl = await UploadImage(folder, antiqueItemModel.Image);
                }
                _antqSvc.Update(antiqueItemModel);
            }
            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public IActionResult Delete(string id)
        {
            _antqSvc.Delete(id);
            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize(Roles = "Admin, Regular")]
        [HttpGet]
        [Route("AntiqueItems/Item/{id}", Name = "antiqueItemRoute")]
        public IActionResult Details(string id, bool isSuccess = false)
        {
            ViewBag.IsSuccess = isSuccess;
            AntiqueItem item = _antqSvc.Find(id);
            int? highestBidOnItem = _bidSvc.GetHighestBidOnItem(id);
            ViewBag.HighestBidOnItem = highestBidOnItem ?? null;
            if (highestBidOnItem != null)
                ViewBag.MinAmountAllowed = highestBidOnItem + 1;
            else
                ViewBag.MinAmountAllowed = item.Price + 1;
            ViewBag.MaxAmountAllowed = null;
            ViewBag.NotAllowedToBid = false;

            Bid oldBid = _bidSvc.GetBiddersBidOnItem(item.Id, User.Identity.Name);
            ViewBag.OldBidId = oldBid?.Id ?? null;
            if (oldBid != null)
            {
                ViewBag.MaxAmountAllowed = _bidSvc.GetBiggestBid() - 1;
                if (ViewBag.MinAmountAllowed > ViewBag.MaxAmountAllowed)
                    ViewBag.NotAllowedToBid = true;
            }
            ViewBag.ViewModel = new AntiqueItemViewModel()
            {
                AntiqueItem = item,
                Bids = _bidSvc.GetBidsForItem(item.Id)
            };
            ViewData["RemainingTime"] = item.AuctionCloseDateTime.ToString("dd-MM-yyyy h:mm:ss tt");
            return View(new Bid());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Regular")]
        public IActionResult CreateBid(Bid bid)
        {
            if (ModelState.IsValid)
            {
                bid.CreatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(bid.Id))
                {
                    _bidSvc.Update(bid);
                }
                else
                {
                    _bidSvc.Create(bid);
                }
                return RedirectToAction("Index", "Home", new { isSuccess = true });
            }
            return RedirectToAction("Index", "Home", new { isSuccess = false });
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
