﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ProductSpecificationGroup
{
    public class SpecificationGroupDTO
    {
        public string SpecificationGroupId { get; set; }
        public string GroupName { get; set; }
        public string LanguageId { get; set; }
        public string LanguageName { get; set; }

        public string ModificationReason { get; set; }
    }
}
