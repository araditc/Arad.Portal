// 
//  --------------------------------------------------------------------
//  Copyright (c) 2005-2021 Arad ITC.
// 
//  Author : Akbar Aslani <aslani@arad-itc.org>
//  Licensed under the Apache License, Version 2.0 (the "License")
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  --------------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Contracts.General.Email;
using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.Email.Mongo;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Arad.Portal.DataLayer.Repositories.General.Mongo
{
    public class POPRepository : BaseRepository, IPOPRepository
    {
        private readonly POPContext _context;
        public POPRepository(IHttpContextAccessor accessor,
                             POPContext context):base(accessor)
        {
            _context = context;
        }

        public async Task<bool> ExistsDefault(string id)
        {
            return await _context.Collection.AsQueryable().AnyAsync(pop => !pop.POPId.Equals(id) && pop.IsDefault);
        }

        //public async  Task<Result> Delete(POP entity)
        //{
            //if (DbContext.Departments.AsQueryable().Any(u => u.POPAccount.Id.Equals(entity.Id)))
            //{
            //    return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_OperationDeleteUseObject") };
            //}

            //return await base.Delete(entity);
        //}
    }
}
