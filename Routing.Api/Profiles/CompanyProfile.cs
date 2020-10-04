using AutoMapper;
using Routing.Api.Entities;
using Routing.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Routing.Api.Profiles
{
    public class CompanyProfile:Profile
    {
        public CompanyProfile()
        {
            CreateMap<Company, CompanyDto>()
                .ForMember(dest => dest.CompanyName,
                            opt => opt.MapFrom(src => src.Name));

            CreateMap<CompanyAddDto, Company>();
            CreateMap<Company, CompanyFullDto>();
        }
    }
}
