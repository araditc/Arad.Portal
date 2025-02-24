using Arad.Portal.DataLayer.Entities.Abstractions;

namespace Arad.Portal.DataLayer.Entities.General.BasicData;

/// <summary>
/// some shared data that we wanna use in whole application store here and witll access through its GroupKey
/// </summary>
public class BasicData : BaseEntity
{
    public string GroupKey { get; set; }
    public string Value { get; set; }
    public string Text { get; set; }
    /// <summary>
    /// its the order of records in exact groupkey 
    /// </summary>
    public int Order { get; set; }
}