using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.CountryParts
{
    public interface ICountryRepository
    {
       
        List<SelectListModel> GetAllCountries();

        List<SelectListModel> GetStates(string countryId);

        List<SelectListModel> GetCities(string cityId);

        Entities.General.Country.Country GetCountry(string countryId);

        Entities.General.Country.Country GetCountryByName(string countryName);
    }
}
