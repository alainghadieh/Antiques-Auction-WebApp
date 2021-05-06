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
    [Authorize(Roles = "Regular")]
    public class NotificationController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly NotificationService _notifSvc;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;


        private ISession Session => _httpContextAccessor.HttpContext.Session;
        private readonly string _NotificationsSessionKey = "Notifications";
        private readonly string _notificationsCountSessionKey = "NotificationsCount";
        public NotificationController(NotificationService notificationService, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<HomeController> logger)
        {
            _notifSvc = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()  
        {  
            List<NotificationViewModel> notifViewModels = new List<NotificationViewModel>();
            List<Notification> notifications = _notifSvc.Read(User.Identity.Name);
            notifViewModels = _mapper.Map<List<NotificationViewModel>>(notifications);
            foreach(var notif in notifications)
            {
                _notifSvc.UpdateRead(notif);
            }
            Session.SetString(_NotificationsSessionKey, JsonConvert.SerializeObject(notifViewModels));
            Session.SetInt32(_notificationsCountSessionKey, 0);
            return View(notifViewModels);  
        }  

    }
}
