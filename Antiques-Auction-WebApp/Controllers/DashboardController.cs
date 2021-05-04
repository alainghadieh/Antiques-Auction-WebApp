using System.Collections.Generic;
using Antiques_Auction_WebApp.Models.Entities;
using Antiques_Auction_WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Antiques_Auction_WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AntiqueItemService _antqSvc;

        public DashboardController(AntiqueItemService antiqueItemService)
        {
            _antqSvc = antiqueItemService;
        }

        public ActionResult<IList<AntiqueItem>> Index() => View(_antqSvc.Read());
    }
}

