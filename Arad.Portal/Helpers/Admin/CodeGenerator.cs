using Arad.Portal.DataLayer.Entities.General.BasicData;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.BasicData;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.Helpers.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.Helpers.Admin;

public class CodeGenerator(
    IBasicDataRepository basicDataRepository,
    IHttpContextAccessor accessor,
    IDomainRepository domainRepository,
    ControllerHelper controllerHelper,
    IMapper mapper)
{
    private readonly IDomainRepository _domainRepository = domainRepository;
    private readonly IMapper _mapper = mapper;
    private static readonly ConcurrentDictionary<long, long> _dictionary = new();

    public long GetNewId(CancellationToken cancellationToken = default)
    {
        List<BasicData> lst = controllerHelper.GetBasicDataList("LastId", false);
        long newId;

        if (lst is { Count: > 0 })
        {
            newId = long.Parse(lst[0].Value) + 1;
        }
        else
        {
            if (accessor.HttpContext != null)
            {
                Domain domain = controllerHelper.GetCurrentUserDomain();

                BasicData basicData = new()
                                      {
                                          AssociatedDomainId = domain.Id,
                                          Id = Guid.NewGuid().ToString(),
                                          GroupKey = "LastId",
                                          Value = "0",
                                          Text = "0"
                                      };

                _ = controllerHelper.InsertNewRecord(basicData, cancellationToken);
            }

            return 1; // Start from 1 if no record exists.
        }

        // Ensure unique ID is added to the dictionary.
        bool added = false;
        long finalId = newId;

        while (!added)
        {
            if (!_dictionary.ContainsKey(finalId))
            {
                added = _dictionary.TryAdd(finalId, finalId);
            }

            if (!added)
            {
                finalId += 1; // Increment only if the ID was not added.
            }
        }

        return finalId;
    }


    public async Task<bool> SaveToDb(long lastId, CancellationToken cancellationToken)
    {
        long item = 0;
        bool result = false;
        Domain domain = controllerHelper.GetCurrentUserDomain();
        
        bool hasLastId = controllerHelper.HasLastId();
        if (!hasLastId)
        {
            BasicData def = new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                GroupKey = "LastId",
                                Text = "0",
                                Value = "0",
                                Order = 1,
                                AssociatedDomainId = domain.Id
                            };
            await basicDataRepository.InsertAsync(def, cancellationToken);
        }
        BasicData entity = await basicDataRepository.FirstOrDefaultAsync(d => d.GroupKey.ToLower() == "lastid" && d.AssociatedDomainId == domain.Id, cancellationToken);
        entity.Text = lastId.ToString();
        entity.Value = lastId.ToString();
        Result<BasicData> updateResult = await basicDataRepository.UpdateAsync(entity, d => d.Id, entity.Id, cancellationToken);
        if (updateResult.Succeeded)
        {
            result = true;
        }
        _dictionary.TryRemove(lastId, out item);
        return result;
    }
}