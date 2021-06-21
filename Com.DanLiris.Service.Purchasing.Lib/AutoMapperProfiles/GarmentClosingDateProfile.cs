using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentClosingDateModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentClosingDateViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class GarmentClosingDateProfile : Profile
    {
        public GarmentClosingDateProfile()
        {
            CreateMap<GarmentClosingDate, GarmentClosingDateViewModel>()
              .ReverseMap();
        }
    }
}
