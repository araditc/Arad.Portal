﻿using Arad.Portal.DataLayer.Contracts.General.BasicData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public class CodeGenerator
    {
        private readonly IBasicDataRepository _basicDataRepository;
        public static ConcurrentDictionary<long, long> dict = new ConcurrentDictionary<long, long>();
        public CodeGenerator(IBasicDataRepository basicDataRepository)
        {
            _basicDataRepository = basicDataRepository; 
        }
       
        public long GetNewId()
        {
            bool added = false;
            var lst = _basicDataRepository.GetList("LastId");

            long newId = long.Parse(lst[0].Value) + 1;
            while (!added)
            {
                newId = long.Parse(lst[0].Value) + 1;
                added = dict.TryAdd(newId, newId);
            }
            return newId;
        }
        public bool SaveToDB(long lastId)
        {
            return _basicDataRepository.SaveLastId(lastId);
        }

        //public static void ClearOldId(object state)
        //{
        //    foreach (var item in dict)
        //    {
        //        long outItem;
        //        dict.TryGetValue(item.Key, out outItem);
        //        if ((DateTime.Now - new DateTime(outItem)).TotalMinutes > Startup.interval)
        //        {
        //            dict.TryRemove(item.Key, out outItem);
        //        }
        //    }
        //    Log.Information($"ClearOldId at: {DateTime.Now}");
        //}
    }
}