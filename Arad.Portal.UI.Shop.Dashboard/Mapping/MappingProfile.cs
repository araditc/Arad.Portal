using Arad.Portal.DataLayer.Entities.General.Content;
using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.Notification;
using Arad.Portal.DataLayer.Entities.General.Permission;
using Arad.Portal.DataLayer.Entities.General.Role;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Entities.Shop.ProductGroup;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Entities.Shop.ProductUnit;
using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Models.Content;
using Arad.Portal.DataLayer.Models.ContentCategory;
using Arad.Portal.DataLayer.Models.Currency;
using Arad.Portal.DataLayer.Models.Domain;
using Arad.Portal.DataLayer.Models.Language;
using Arad.Portal.DataLayer.Models.Notification;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.ProductGroup;
using Arad.Portal.DataLayer.Models.ProductSpecification;
using Arad.Portal.DataLayer.Models.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.Promotion;
using Arad.Portal.DataLayer.Models.Role;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.ShoppingCart;
using Arad.Portal.DataLayer.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Arad.Portal.UI.Shop.Dashboard.Mapping
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<DomainDTO, Domain>().ReverseMap();
            CreateMap<CurrencyDTO, Currency>().ReverseMap();
            CreateMap<PermissionDTO, Permission>().ReverseMap();
            CreateMap<LanguageDTO, Language>().ReverseMap();
            CreateMap<ProductUnitDTO, ProductUnit>().ReverseMap();
            CreateMap<UserDTO, ApplicationUser>().ReverseMap();
            CreateMap<ProductGroupDTO, ProductGroup>().ReverseMap();
            CreateMap<RoleDTO, Role>().ReverseMap();
            CreateMap<SpecificationGroupDTO, ProductSpecGroup>().ReverseMap();
            CreateMap<ProductSpecification, ProductSpecificationDTO>().ReverseMap();
            CreateMap<ProductInputDTO, Product>().ReverseMap();
            CreateMap<Product, ProductOutputDTO>().ReverseMap();
            CreateMap<Promotion, PromotionDTO>().ReverseMap();
            CreateMap<Price, PriceDTO>().ReverseMap();
            CreateMap<ShoppingCart, ShoppingCartDTO>();
            CreateMap<NotificationDTO, Notification>();
            CreateMap<ContentCategory, ContentCategoryDTO>().ReverseMap();
            CreateMap<Content, ContentDTO>().ReverseMap();
        }
        
    }
}
