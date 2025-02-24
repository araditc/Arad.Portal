using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Arad.Portal.DataLayer.Entities.Abstractions;

public interface IEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }
}
public class LiteEntity : IEntity
{
    public string Id { get; set; }
}
public class BaseEntity : IEntity
{
    public string Id { get; set; }
    #region Properties
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public string CreatorUserId { get; set; }
    public string CreatorUserName { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    public string AssociatedDomainId { get; set; }
    #endregion       
}
public class BaseEntity2 
{
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public string CreatorUserId { get; set; }
    public string CreatorUserName { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    public string AssociatedDomainId { get; set; }
}
public enum ActionTypes
{
    [Display(Name = "Action_Insert")]
    Insert = 0,
    [Display(Name = "Action_Update")]
    Update = 1,
    [Display(Name = "Action_Delete")]
    Delete = 2,
    [Display(Name = "Action_ChangePassword")]
    ChangePassword = 3,
    [Display(Name = "Action_Comment")]
    Comment = 4,
    [Display(Name = "Action_Restore")]
    Restore = 5

}
public enum CollectionType
{
    [Display(Name = "PermissionTitle_Users")]
    ApplicationUser = 1,
    [Display(Name = "PermissionTitle_Roles")]
    ApplicationRole = 2,
    BasicData = 3,
    Content = 4,
    ContentCategory = 5,
    Country = 6,
    Domain = 7,
    ErrorLog = 8,
    Language = 9,
    Menu = 10,
    MessageTemplate = 11,
    Module = 12,
    Notification = 13,
    Permission = 14,
    Product = 15,
    ProductGroup = 16,
    ProductSpecGroup = 17,
    ProductSpecification = 18,
    ProductUnit = 19,
    Promotion = 20,
    SendMessage = 21,
    Slider = 22,
    Comment = 23,
    Currency = 24,
    Excel = 25,
    Shipping = 26
}