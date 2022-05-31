using Arad.Portal.DataLayer.Models.DesignStructure;
using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.DesignStructure
{
    public class Module : BaseEntity
    {
        public Module()
        {
            ModuleParameters = new();
        }
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ModuleId { get; set; }
        public ModuleCategoryType ModuleCategoryType { get; set; }
        public string ModuleName { get; set; }
        public string ComponentName { get; set; }
        /// <summary>
        /// this list contains some parameters for executing and designing this component
        /// for example input parameters for viewcomponent 
        /// if componentName = 'ContentTemplate' one parameter should be enum ContentTemplateDesign and its value
        /// if its enum is equal to forth or fifth 'count' should also declare
        /// </summary>
       public ModuleParameters ModuleParameters { get; set; }
    }

    public enum ModuleCategoryType
    {
        Product = 0,
        Content = 1,
        Advertisement = 2,
        Slider = 3,
        Menu = 5
    }
   
    public enum ProductOrContentType
    {
       
        Newest = 0,
        MostPopular = 1,
        BestSale = 2,
        MostVisited = 3
    }

    public enum ImageSliderTemplateDesign
    {
        First = 0
    }

    public enum AdvertisementTemplateDesign
    {
        First = 0
    }
    public enum ProductTemplateDesign
    {
        First = 0
    }
    
    public enum ContentTemplateDesign
    {
        First = 0,
        Second = 1,
        Third = 2,
        Forth = 3,
        Fifth = 4
    }
}
