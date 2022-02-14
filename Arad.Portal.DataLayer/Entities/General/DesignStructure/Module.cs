using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.DesignStructure
{
    public class Module : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ModuleId { get; set; }
        public ModuleContentType ModuleContentType { get; set; }
        public string ComponentName { get; set; }

        /// <summary>
        /// this list contains some parameters for executing and designing this component
        /// for example input parameters for viewcomponent 
        /// if componentName = 'ContentTemplate' one parameter should be enum ContentTemplate and its value
        /// if its enum is equal to forth or fifth count it should also declare
        /// </summary>
       public List<KeyVal> ParamsWithDefaultValues { get; set; }
       

    }
    public enum ModuleContentType
    {
        MostPopulareProducts,
        MostVisitedProducts,
        BestSaleProducts,
        NewestProduct,
        ImageSlider,
        MostPopularContent,
        MostVisitedContent,
        NewestContent,
        Advertisement
    }

    public enum ProductType
    {
        Newest,
        MostPopular,
        BestSale, 
        MostVisited
    }

    public enum ContentType
    {
        Newest,
        MostPopular,   
        MostVisited
    }

    public enum ContentTemplate
    {
        First,
        Second,
        Third,
        Forth,
        Fifth
    }
}
