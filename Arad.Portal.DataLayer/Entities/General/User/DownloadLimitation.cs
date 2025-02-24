using Arad.Portal.DataLayer.Entities.Abstractions;

using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Arad.Portal.DataLayer.Entities.General.User;

public class DownloadLimitation : BaseEntity
{
    public string ShoppingCartDetailId { get; set; }

    public string ProductId { get; set; }

    public int? DownloadedCount { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? StartDate { get; set; }
}