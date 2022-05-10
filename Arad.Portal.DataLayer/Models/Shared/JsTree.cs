using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class JsTree
    {
        public string Id { get; set; }

        public string Text { get; set; }

        //public string Icon { get; set; }

        public State State { get; set; }

        public List<JsTree> Children { get; set; }
    }

    public class State
    {
        public bool Opened { get; set; }

        public bool Disabled { get; set; }

        public bool Selected { get; set; }
    }
}