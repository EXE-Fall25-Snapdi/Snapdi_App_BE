using AutoMapper;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs.RequestModels;
using Snapdi.Services.DTOs.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.Mapping
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<Booking, BookingResponse>()
                  .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
                  .ForMember(dest => dest.PhotographerName, opt => opt.MapFrom(src => src.Photographer.Name))
                  .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.StatusName));
        }
    }
}
