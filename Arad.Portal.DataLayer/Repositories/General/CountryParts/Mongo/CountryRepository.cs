﻿using Arad.Portal.DataLayer.Contracts.General.CountryParts;
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
using Arad.Portal.DataLayer.Entities.General.Country;
using Arad.Portal.GeneralLibrary.Utilities;

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

        public IMongoCollection<Country> Countries { get; set; }
        
      
        public List<SelectListModel> GetAllCountries()
        {
            var lst = _context.CountryCollection.AsQueryable().Select(_ => new SelectListModel()
            {
                Value = _.Id,
                Text = _.Name
            }).ToList();
            lst.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("Choose"), Value = "-1" });
            return lst;
        }

        public List<SelectListModel> GetStates(string countryName)
        {
            var states = _context.CountryCollection.AsQueryable()
                .Where(_ => _.Name == countryName).SelectMany(_ => _.States).OrderBy(_ => _.Name)
                .Select(_ => new SelectListModel() { Text = _.Name, Value = _.Id }).ToList();
            states.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("Choose"), Value = "-1" });   
            return states;
        }

        public List<SelectListModel> GetCities(string stateId)
        {
            var cities = _context.CountryCollection.AsQueryable()
               .Where(c => c.States.Any(s => s.Id.Equals(stateId)))
               .SelectMany(s => s.States).Where(c => c.Id.Equals(stateId))
               .SelectMany(s => s.Cities).OrderBy(s => s.Name)
               .Select(_ => new SelectListModel() { Text = _.Name, Value = _.Id }).ToList();
            cities.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("Choose"), Value = "-1" });
            return cities;
        }

        public Country GetCountry(string countryId)
        {
            return Countries.Find(_ => _.Id == countryId).FirstOrDefault();
        }

        public Country GetCountryByName(string countryName)
        {
            return Countries.Find(_ => _.Name == countryName).FirstOrDefault();
        }
    }
}
