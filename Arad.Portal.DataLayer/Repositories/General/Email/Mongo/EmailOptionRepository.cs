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


using Arad.Portal.DataLayer.Contracts.General.Email;
using Arad.Portal.DataLayer.Repositories.General.Email.Mongo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace Arad.Portal.DataLayer.Repositories.General.Mongo
{
    public class EmailOptionRepository :BaseRepository, IEmailOptionRepository
    {
        private readonly EmailOptionContext _context;
        public EmailOptionRepository(IHttpContextAccessor accessor,
            IWebHostEnvironment env,
                                      EmailOptionContext context):base(accessor, env)
        {
            _context = context;
        }
    }
}
