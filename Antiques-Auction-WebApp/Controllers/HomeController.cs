using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Antiques_Auction_WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Antiques_Auction_WebApp.Services;
using Antiques_Auction_WebApp.Models;
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Antiques_Auction_WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AntiqueItemService _antqSvc;
        private readonly AutoBidConfigService _configSvc;
        private readonly NotificationService _notifSvc;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        private ISession Session => _httpContextAccessor.HttpContext.Session;
        private readonly string _NotificationsSessionKey = "Notifications";
        private readonly string _notificationsCountSessionKey = "NotificationsCount";

        public HomeController(AntiqueItemService antiqueItemService, AutoBidConfigService configurationService, NotificationService notificationService, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<HomeController> logger)
        {
            _antqSvc = antiqueItemService;
            _configSvc = configurationService;
            _notifSvc = notificationService;
            _httpContextAccessor = httpContextAccessor;
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
            ViewBag.IsSuccess = isSuccess;
            List<NotificationViewModel> notifications = new List<NotificationViewModel>();

            notifications = _mapper.Map<List<NotificationViewModel>>(_notifSvc.Read(User.Identity.Name));

            Session.SetString(_NotificationsSessionKey, JsonConvert.SerializeObject(notifications));
            Session.SetInt32(_notificationsCountSessionKey, notifications.Count);
            return View(_mapper.Map<List<AntiqueItemViewModel>>(_antqSvc.GetItemsForSale()));
        }

        [Authorize(Roles = "Regular")]
        [HttpGet]
        public IActionResult AutoBidConfiguration(bool isSuccess = false)
        {
            AutoBidConfigViewModel configViewModel = new AutoBidConfigViewModel();
            AutoBidConfig config = _configSvc.Find(User.Identity.Name);
            ViewBag.ActionName = "";
            if (config == null)
            {
                configViewModel.UserName = User.Identity.Name;
                ViewBag.ActionName = "Create";
            }
            else
            {
                configViewModel = _mapper.Map<AutoBidConfigViewModel>(config);
                ViewBag.ActionName = "Update";
            }
            List<NotificationViewModel> notifications = new List<NotificationViewModel>();
            notifications = _mapper.Map<List<NotificationViewModel>>(_notifSvc.Read(User.Identity.Name));
            Session.SetString(_NotificationsSessionKey, JsonConvert.SerializeObject(notifications));
            Session.SetInt32(_notificationsCountSessionKey, notifications.Count);

            ViewBag.IsSuccess = isSuccess;
            return View(configViewModel);
        }

        [Authorize(Roles = "Regular")]
        [HttpPost]
        public IActionResult Create(AutoBidConfigViewModel configViewModel)
        {
            if (ModelState.IsValid)
            {
                AutoBidConfig config = _mapper.Map<AutoBidConfig>(configViewModel);
                _configSvc.Create(config);
                return RedirectToAction(nameof(AutoBidConfiguration), new { isSuccess = true });
            }
            return RedirectToAction(nameof(AutoBidConfiguration), new { isSuccess = false });
        }

        [Authorize(Roles = "Regular")]
        [HttpPost]
        public IActionResult Update(AutoBidConfigViewModel configViewModel)
        {
            if (ModelState.IsValid)
            {
                var mappedConfig = _mapper.Map<AutoBidConfig>(configViewModel);
                _configSvc.Update(mappedConfig);
                return RedirectToAction(nameof(AutoBidConfiguration), new { isSuccess = true });
            }
            return RedirectToAction(nameof(AutoBidConfiguration), new { isSuccess = false });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
