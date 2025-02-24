using Arad.Portal.DataLayer.Entities.Abstractions;
using Microsoft.AspNetCore.Identity;

using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.General.ApplicationRole;

public class ApplicationRole : IdentityRole<string>, IEntity
{
    public List<string> PermissionIds { get; set; }
    #region Properties
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreationDate { get; set; }
    public string CreatorUserId { get; set; }
    public string CreatorUserName { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    public string AssociatedDomainId { get; set; }
    #endregion       
}