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
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Entities.General.MessageTemplate
{
    public class MessageTemplate : BaseEntity
    {
        public MessageTemplate()
        {
            MessageTemplateMultiLingual = new();
        }
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string MessageTemplateId { get; set; }

        public string TemplateName { get; set; }

        public string TemplateDescription { get; set; }
       
        public bool IsSystemTemplate { get; set; }

        public NotificationType NotificationType { get; set; }

        public List<MessageTemplateMultiLingual> MessageTemplateMultiLingual { get; set; }
    }

    public class MessageTemplateMultiLingual
    {
        /// <summary>
        /// this is symbol in languageTemplate
        /// </summary>
        public string LanguageName { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
