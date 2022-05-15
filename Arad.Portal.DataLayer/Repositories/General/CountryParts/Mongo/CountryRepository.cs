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
using Microsoft.AspNetCore.Hosting;


namespace Arad.Portal.DataLayer.Repositories.General.CountryParts.Mongo
{
    public class CountryRepository: BaseRepository, ICountryRepository
    {
        private readonly CountryContext _context;
        private readonly IMapper _mapper;
        public CountryRepository(IHttpContextAccessor httpContextAccessor,
            CountryContext context,
            IWebHostEnvironment env,
            IMapper mapper) :base(httpContextAccessor, env)
        {
            _mapper = mapper;
            _context = context;
            Countries = context.CountryCollection;
          
        }

        public IMongoCollection<Entities.General.Country.Country> Countries { get; set; }
        

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
            var states = _context.CountryCollection.AsQueryable()
                .Where(_ => _.Id == countryId).SelectMany(_ => _.States).OrderBy(_ => _.Name)
                .Select(_ => new SelectListModel() { Text = _.Name, Value = _.Id }).ToList();
               
            return states;
        }

        public List<SelectListModel> GetCities(string stateId)
        {
            return _context.CountryCollection.AsQueryable()
               .Where(c => c.States.Any(s => s.Id.Equals(stateId)))
               .SelectMany(s => s.States).Where(c => c.Id.Equals(stateId))
               .SelectMany(s => s.Cities).OrderBy(s => s.Name)
               .Select(_ => new SelectListModel() { Text = _.Name, Value = _.Id }).ToList();
        }

    }
}
