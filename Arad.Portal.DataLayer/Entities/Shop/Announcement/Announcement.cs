﻿using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.Announcement
{
    //not have been implemented yet
    public class Announcement : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string AnnouncementId { get; set; }

        public string ProductId { get; set; }

        public bool IsNew { get; set; }
    }
}
