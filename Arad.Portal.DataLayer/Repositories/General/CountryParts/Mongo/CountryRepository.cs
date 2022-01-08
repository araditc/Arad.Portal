using Arad.Portal.DataLayer.Contracts.General.CountryParts;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.CountryParts.Mongo
{
    public class CountryRepository: BaseRepository, ICountryRepository
    {
        private readonly CountryContext _context;
        private readonly IMapper _mapper;
        public CountryRepository(IHttpContextAccessor httpContextAccessor,CountryContext context,
            IMapper mapper) :base(httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
            Countries = context.CountryCollection;
            States = context.StateCollection;
            Cities = context.CityCollection;
        }

        public IMongoCollection<Entities.General.Country.Country> Countries { get; set; }
        public IMongoCollection<Entities.General.State.State> States { get; set; }
        public IMongoCollection<Entities.General.City.City> Cities { get; set; }


        //public async Task InsertMany(List<Entities.General.Country.Country> countries)
        //{
        //    await _context.CountryCollection.InsertManyAsync(countries);
        //}

        public List<SelectListModel> GetAllCountries()
        {
            var lst = _context.CountryCollection.AsQueryable().Select(_ => new SelectListModel()
            {
                Value = _.Id,
                Text = _.Name
            }).ToList();
            return lst;
        }

        public List<SelectListModel> GetStates(string countryId)
        {
            var states = _context.StateCollection.Find(_ => _.Id == countryId)
                .Project(_ => new SelectListModel() { Text = _.Name,
                                                       Value = _.Id}).ToList();
           
            return states;
        }

        public List<SelectListModel> GetCities(string stateId)
        {
            var cities = _context.CityCollection.Find(_ => _.StateId == stateId)
                .Project(_ => new SelectListModel() { Text = _.Name, Value = _.Id}).ToList();
            return cities;
        }

    }
}
