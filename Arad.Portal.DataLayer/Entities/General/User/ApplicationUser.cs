using Arad.Portal.DataLayer.Models.Shared;

using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Models.Shared.User;
using Arad.Portal.DataLayer.Entities.Shop.Product;

namespace Arad.Portal.DataLayer.Entities.General.User;

/// <summary>
/// all users of site whether they are site user or admin user will be store here 
/// this entity inherit from mongoUser which inherit identityUser 
/// </summary>
/// 
[CustomCollectionName("Users")]
public class ApplicationUser : IdentityUser<string> , IEntity
{
    public ApplicationUser()
    {
        LoginData = [];
        Profile = new();
        LoginData = [];
        Otp = new();
        Domains = [];
    }
    /// <summary>
    /// if isSystemAccount = true this user have full access to all links and full ability only one user have this ability
    /// </summary>
    public bool IsSystemAccount { get; set; }
    public List<UserFavorites> Favorites { get; set; } 
    /// <summary>
    /// inactive user cant login and work with its account
    /// </summary>
    public bool IsActive { get; set; }

    public bool IsVendor { get; init; }
    /// <summary>
    /// if user constructed in site this field is true
    /// </summary>
    public bool IsSiteUser { get; set; }

    /// <summary>
    /// some information of user
    /// </summary>
    public Profile Profile { get; set; }

    /// <summary>
    /// the primary key of Role which this user have
    /// </summary>
    public string UserRoleId { get; init; }

    /// <summary>
    /// one time password which send for registration and changing pass of user
    /// </summary>
    public OTP Otp { get; init; }

    /// <summary>
    /// we have soft deleted all deleted entities store with isdeleted = true
    /// </summary>
    public bool IsDeleted { get; set; }
    public AddressType AddressType { get; init; }
    public List<Product> Products { get; init; }
    /// <summary>
    /// all domains which user belong to whether domainOwner or not
    /// </summary>
    public List<UserDomain> Domains { get; set; }
    //public string DomainId { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public string CreatorId { get; set; }

    public string AssociateDomainId { get; set; }
    public string CreatorUserName { get; set; }
    public DateTime LastLoginDate { get; set; }
    public List<LoginLogoutRecord> LoginData { get; init; } 
}


public class UserDomain
{
    public string? DomainId { get; set; }

    public string? DomainName { get; set; }

    public bool IsOwner { get; set; }
}