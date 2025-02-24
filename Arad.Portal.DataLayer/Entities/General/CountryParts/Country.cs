using Arad.Portal.DataLayer.Entities.Abstractions;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.General.CountryParts;

public class Country : BaseEntity
{
    public Country()
    {
        States = new();
    }
    public string Name { get; set; }

    /// <summary>
    /// Alpha-2 code
    /// </summary>
    public string SortName { get; set; }

    public List<State> States { get; set; }
}

public class State
{
    public State()
    {
        Cities = new();
    }

    public string Id { get; set; }

    public string Name { get; set; }

    public string CountryId { get; set; }

    public string CountryName { get; set; }

    public List<City> Cities { get; set; }
}

public class City
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string StateId { get; set; }
    public string StateName { get; set; }
    public string CountryId { get; set; }
    public string CountryName { get; set; }
}