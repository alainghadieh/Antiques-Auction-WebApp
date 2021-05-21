using System;
using System.Globalization;
using Antiques_Auction_WebApp.Models;
using Antiques_Auction_WebApp.ViewModels;
using AutoMapper;

namespace Antiques_Auction_WebApp.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DateTime, string>().ConvertUsing(new StringTypeConverter());
            CreateMap<AutoBidConfig, AutoBidConfigViewModel>().ReverseMap();
            CreateMap<Bid, BidViewModel>().ReverseMap();
            CreateMap<AntiqueItem, AntiqueItemViewModel>().ReverseMap();
            CreateMap<Notification, NotificationViewModel>().ReverseMap();
            CreateMap<Bill, BillViewModel>().ReverseMap();
        }

        public class StringTypeConverter :ITypeConverter<DateTime, string>{
            public string Convert(DateTime source, string destination, ResolutionContext context)
            {
                return source.ToString("f", CultureInfo.CreateSpecificCulture("en-US"));
            }
        }
    }
}