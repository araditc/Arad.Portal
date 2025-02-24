using System;
using System.Collections.Generic;

using Arad.Portal.DataLayer.Entities.Abstractions;


namespace Arad.Portal.DataLayer.Entities.General.Modification;

public class Modification : LiteEntity
{
    public string RecordId { get; set; }

    public string Ip { get; set; } = null!;

    public DateTime ModifyDateTime { get; set; } = DateTime.Now;

    public string ModifierId { get; set; }

    public string ModifierUserName { get; set; }

    public string UserId { get; set; }

    public string UserName { get; set; }

    public List<string> Reasons { get; set; }

    public ActionTypes ActionTypes { get; set; }

    public CollectionType CollectionType { get; set; }
}