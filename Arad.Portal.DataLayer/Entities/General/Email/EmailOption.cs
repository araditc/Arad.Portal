//
//  --------------------------------------------------------------------
//  Copyright (c) 2005-2021 Arad ITC.
//
//  Author : Ammar Heidari <ammar@arad-itc.org>
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




using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Entities.General.Email
{
    public class EmailOption : BaseEntity
    {
        public string EmailOptionId { get; set; }

        public DefaultEncoding DefaultEncoding { get; set; }

        /// <summary>
        /// delete those emails which fail to send after x days
        /// </summary>
        public int DeleteFailedEmailsAfter { get; set; } 

        /// <summary>
        /// black list email put in string with comma seperated
        /// </summary>
        public string EmailBlacklist { get; set; } 

        public bool FilterDuplicateIncomingEmail { get; set; }
    }
}
