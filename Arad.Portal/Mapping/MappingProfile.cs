using Arad.Portal.DataLayer.Entities.General.ApplicationRole;
using Arad.Portal.DataLayer.Entities.General.BasicData;
using Arad.Portal.DataLayer.Entities.General.Comment;
using Arad.Portal.DataLayer.Entities.General.Content;
using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.Menu;
using Arad.Portal.DataLayer.Entities.General.Notify;
using Arad.Portal.DataLayer.Entities.General.Permission;
using Arad.Portal.DataLayer.Entities.General.SendMessage;
using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Arad.Portal.DataLayer.Entities.General.SMS;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Entities.Shop.ProductGroup;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Entities.Shop.ProductUnit;
using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Arad.Portal.DataLayer.Entities.Shop.Setting;
using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Shared.Product;
using Arad.Portal.DataLayer.Models.Shared.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared.User;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Comment;
using Arad.Portal.Models.Shared.Content;
using Arad.Portal.Models.Shared.ContentCategory;
using Arad.Portal.Models.Shared.Currency;
using Arad.Portal.Models.Shared.Domain;
using Arad.Portal.Models.Shared.Language;
using Arad.Portal.Models.Shared.Menu;
using Arad.Portal.Models.Shared.Notification;
using Arad.Portal.Models.Shared.Permission;
using Arad.Portal.Models.Shared.Product;
using Arad.Portal.Models.Shared.ProductSpecification;
using Arad.Portal.Models.Shared.ProductSpecificationGroup;
using Arad.Portal.Models.Shared.Promotion;
using Arad.Portal.Models.Shared.Role;
using Arad.Portal.Models.Shared.SendMessage;
using Arad.Portal.Models.Shared.Setting;
using Arad.Portal.Models.Shared.ShoppingCart;
using Arad.Portal.Models.Shared.SlideModule;
using Arad.Portal.Models.Shared.Transaction;
using Arad.Portal.Models.Shared.User;

namespace Arad.Portal.Mapping;

public class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
    {
        CreateMap<SmtpDto, Smtp>().ReverseMap();
        CreateMap<SmtpDto, SendMessage>()
            .ForMember(dest => dest.SendMessageMetaData, opt => opt.MapFrom(src => src));
        CreateMap<SmsDto, SendMessage>()
            .ForMember(dest => dest.SendMessageMetaData, opt => opt.MapFrom(src => src));
        CreateMap<ApplicationUser, UserListView>();
        CreateMap<SendMessage, SendMessageViewModel>().ReverseMap();
        CreateMap<SmsDto, Sms>().ReverseMap();
        CreateMap<SmsDto, SendMessage>()
            .ForMember(dest => dest.SendMessageMetaData, opt => opt.MapFrom(src => src));
        CreateMap<ApplicationUser, UserListView>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Profile.FullName))
            .ReverseMap();
        CreateMap<ApplicationRole, RoleListView>().ReverseMap();
        CreateMap<Language, LanguageDto>().ReverseMap();
        CreateMap<RegisterUserModel, ApplicationUser>().ReverseMap();
        CreateMap<DomainDto, Domain>().ReverseMap();
        CreateMap<Profile, UserProfileDto>().ReverseMap();
        CreateMap<AddressDto, Address>().ReverseMap();
        CreateMap<PermissionDto, Permission>().ReverseMap();
        CreateMap<ProductUnitDto, ProductUnit>().ReverseMap();
        CreateMap<UserDto, ApplicationUser>().ReverseMap();
        CreateMap<ProductGroupDto, ProductGroup>().ReverseMap();
        CreateMap<RoleDto, ApplicationRole>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ?? true))
            .ReverseMap();
        CreateMap<SpecificationGroupDto, ProductSpecGroup>().ReverseMap();
        CreateMap<ProductSpecification, ProductSpecificationDto>().ReverseMap();
        CreateMap<ProductInputDto, Product>().ReverseMap();
        CreateMap<Product, ProductOutputDto>().ReverseMap();
        CreateMap<Promotion, PromotionDto>().ReverseMap();
        CreateMap<Price, PriceDto>()
            .ForMember(dest => dest.SDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EDate, opt => opt.MapFrom(src => src.EndDate))
            .ReverseMap();
        CreateMap<ShoppingCart, ShoppingCartDto>().ReverseMap();
        CreateMap<ShoppingCartDetail, ShoppingCartDetailDto>().ReverseMap();
        CreateMap<NotificationDto, Notification>().ReverseMap();
        CreateMap<ContentCategory, ContentCategoryDto>().ReverseMap();
        CreateMap<Content, ContentDto>()
            .ForMember(dest => dest.PersianStartShowDate, opt => opt.MapFrom(src => src.StartShowDate))
            .ForMember(dest => dest.PersianEndShowDate, opt => opt.MapFrom(src => src.EndShowDate))
            .ReverseMap();
        _ = CreateMap<ContentViewModel, Content>().ReverseMap();
        CreateMap<Comment, CommentDto>().ReverseMap();
        CreateMap<Comment, CommentVm>().ReverseMap();
        CreateMap<Menu, MenuDto>().ReverseMap();
        CreateMap<BasicDataModel, BasicData>().ReverseMap();
        CreateMap<ShippingSetting, ShippingSettingDto>().ReverseMap();
        CreateMap<ShippingTypeDetail, ShippingTypeDetailDto>().ReverseMap();
        CreateMap<PurchasePerSeller, PurchasePerSellerDto>().ReverseMap();
        CreateMap<ShippingCoupon, ShippingCouponDto>().ReverseMap();
        CreateMap<ProviderDetail, ProviderDetailDto>().ReverseMap();
        CreateMap<Permission, PermissionTreeViewDto>()
            .ReverseMap();
        CreateMap<DataLayer.Entities.General.Permission.Action, ActionDto>().ReverseMap();
        CreateMap<Slide, SlideDto>().ReverseMap();
        CreateMap<UserFavorites, UserFavoritesDto>().ReverseMap();
        CreateMap<UserCoupon, UserCouponDto>().ReverseMap();
        CreateMap<CurrencyDto, Currency>().ReverseMap();
        CreateMap<Result<Domain>, Result<DomainDto>>().ReverseMap();
        CreateMap<Permission, PermissionConverter>()
            .ForMember(c => c.Children, m => m.MapFrom(c => c.Children))
            .ForMember(c => c.Actions, m => m.MapFrom(c => c.Actions))
            .ReverseMap();
        CreateMap<ApplicationUser, UserEdit>().ReverseMap();
        CreateMap<ContentGlance, Content>()
            .ReverseMap();
        CreateMap<Layer, LayerView>().ReverseMap();
        CreateMap<Domain, DomainModel>().ReverseMap();
        CreateMap<SendMessage, SendMessageDto>().ReverseMap();
        CreateMap<Transaction, TransactionDto>().ReverseMap();
    }
}