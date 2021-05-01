using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Antiques_Auction_WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Antiques_Auction_WebApp.Services;
using System.Collections.Generic;
using Antiques_Auction_WebApp.Models;

namespace Antiques_Auction_WebApp.Controllers
{
    [Authorize(Roles = "Regular")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AntiqueItemService _antqSvc;

        public HomeController(AntiqueItemService antiqueItemService, ILogger<HomeController> logger)
        {
            _antqSvc = antiqueItemService;
            _logger = logger;
        }

        public IActionResult Index()  
        {  
            return View(_antqSvc.GetItemsForSale());  
        }  

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
