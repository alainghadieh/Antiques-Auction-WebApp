using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Antiques_Auction_WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Antiques_Auction_WebApp.Services;
using Antiques_Auction_WebApp.Models;
using AutoMapper;
using System.Collections.Generic;

namespace Antiques_Auction_WebApp.Controllers
{
    [Authorize(Roles = "Regular")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AntiqueItemService _antqSvc;
        private readonly AutoBidConfigService _configSvc;
        private readonly IMapper _mapper;

        public HomeController(AntiqueItemService antiqueItemService, AutoBidConfigService configurationService, IMapper mapper, ILogger<HomeController> logger)
        {
            _antqSvc = antiqueItemService;
            _configSvc = configurationService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(bool isSuccess = false)  
        {  
            ViewBag.IsSuccess = isSuccess;
            return View(_mapper.Map<List<AntiqueItemViewModel>>(_antqSvc.GetItemsForSale()));  
        }  

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
            ViewBag.IsSuccess = isSuccess;
            return View(configViewModel);
        }

        [HttpPost]
        public IActionResult Create(AutoBidConfigViewModel configViewModel)
        {
            if (ModelState.IsValid)
            {
                var mappedConfig = _mapper.Map<AutoBidConfig>(configViewModel);
                _configSvc.Create(mappedConfig);
                return RedirectToAction(nameof(AutoBidConfiguration), new { isSuccess = true });
            }
            return RedirectToAction(nameof(AutoBidConfiguration), new { isSuccess = false });
        }
        [HttpPost]
        public IActionResult Update(AutoBidConfigViewModel configViewModel)
        {
            if(ModelState.IsValid)
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
