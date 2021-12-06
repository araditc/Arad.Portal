using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class Result
    {
        public bool Succeeded { get; set; } = false;
        public string Message { get; set; }
    }
    public class Result<TReturnType> where TReturnType : class
    {
        public bool Succeeded { get; set; } = false;
        public string Message { get; set; }
        public TReturnType ReturnValue { get; set; }
    }
}
