using Antiques_Auction_WebApp.Models;
using Antiques_Auction_WebApp.ViewModels;
using AutoMapper;

namespace Antiques_Auction_WebApp.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AutoBidConfig, AutoBidConfigViewModel>().ReverseMap();
            CreateMap<Bid, BidViewModel>().ReverseMap();
            CreateMap<AntiqueItem, AntiqueItemViewModel>().ReverseMap();
        }
    }
}