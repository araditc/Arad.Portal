using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Currency;
using Arad.Portal.DataLayer.Models.Domain;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Entities.General.Permission;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Models.Language;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Entities.Shop.ProductUnit;

namespace Arad.Portal.UI.Shop.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DomainDTO, Domain>().ReverseMap();
            CreateMap<CurrencyDTO, Currency>().ReverseMap();
            CreateMap<PermissionDTO,Permission>().ReverseMap();
            CreateMap<LanguageDTO, Language>().ReverseMap();
            CreateMap<ProductUnitDTO, ProductUnit>().ReverseMap();
        }
    }
}
