using Arad.Portal.DataLayer.Contracts.General.Role;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.General.Role.Mongo
{
    public class RoleRepository : BaseRepository, IRoleRepository
    {
        private readonly RoleContext _context;
        public RoleRepository(RoleContext roleContext, 
            IHttpContextAccessor httpContextAccessor): base(httpContextAccessor)
        {
            _context = roleContext;
        }
        public async Task<Entities.General.Role.Role> FetchRole(string roleId)
        {
            try
            {
                var role = await _context.Collection
                    .Find(c => c.RoleId == roleId).FirstOrDefaultAsync();
                return role;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
