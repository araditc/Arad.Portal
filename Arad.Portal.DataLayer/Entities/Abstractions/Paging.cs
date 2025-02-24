using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.Abstractions;

public class Paging<T> where T : class
{
    public int CurrentPage { get; set; }

    public List<T> Items { get; set; } = [];

    public int PageSize { get; set; }

    public string QueryString { get; set; } = null!;
}