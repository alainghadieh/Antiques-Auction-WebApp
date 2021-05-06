using System.Collections.Generic;
using Antiques_Auction_WebApp.Services;
using Antiques_Auction_WebApp.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Antiques_Auction_WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AntiqueItemService _antqSvc;
        private readonly IMapper _mapper;

        public DashboardController(AntiqueItemService antiqueItemService, IMapper mapper)
        {
            _antqSvc = antiqueItemService;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<List<AntiqueItemViewModel>>(_antqSvc.Read()));
        }
    }
}

