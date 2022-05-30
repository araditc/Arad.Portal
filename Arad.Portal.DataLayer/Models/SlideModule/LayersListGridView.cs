using Arad.Portal.DataLayer.Entities.General.SliderModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.SlideModule
{
    public class LayersListGridView
    {
        public string Id { get; set; }
        public LayerType Type { get; set; }
        public string Content { get; set; }
        public string Link { get; set; }
        public Position Position { get; set; }
        public TransActionType TransActionType { get; set; }
    }
}
