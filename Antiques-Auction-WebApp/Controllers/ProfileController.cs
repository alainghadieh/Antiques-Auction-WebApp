using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Antiques_Auction_WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Antiques_Auction_WebApp.Services;
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Antiques_Auction_WebApp.Interfaces;

namespace Antiques_Auction_WebApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AntiqueItemService _antqSvc;
        private readonly BillService _billSvc;
        private readonly NotificationService _notifSvc;
        private readonly IAuctionRepository _auctionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        private ISession Session => _httpContextAccessor.HttpContext.Session;
        private readonly string _NotificationsSessionKey = "Notifications";
        private readonly string _notificationsCountSessionKey = "NotificationsCount";

        public ProfileController(AntiqueItemService antiqueItemService, BillService billService, IAuctionRepository auctionRepository, IHttpContextAccessor httpContextAccessor, NotificationService notificationService, IMapper mapper, ILogger<HomeController> logger)
        {
            _antqSvc = antiqueItemService;
            _billSvc = billService;
            _auctionRepository = auctionRepository;
            _httpContextAccessor = httpContextAccessor;
            _notifSvc = notificationService;
            _mapper = mapper;
            _logger = logger;
        }

        [Authorize(Roles = "Admin,Regular")]
        [HttpGet]
        public IActionResult Index(bool isSuccess = false)
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index), "Dashboard");
            }
            List<NotificationViewModel> notifications = new List<NotificationViewModel>();

            notifications = _mapper.Map<List<NotificationViewModel>>(_notifSvc.Read(User.Identity.Name));

            Session.SetString(_NotificationsSessionKey, JsonConvert.SerializeObject(notifications));
            Session.SetInt32(_notificationsCountSessionKey, notifications.Count);
            ViewBag.Bids = _auctionRepository.GetUserDetailedBids(User.Identity.Name);
            ViewBag.Awards = _auctionRepository.GetUserAwards(User.Identity.Name);
            return View();
        }

    }
}
