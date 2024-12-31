using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Data.Models
{
    public class ExceptionBaseResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
    }
}
