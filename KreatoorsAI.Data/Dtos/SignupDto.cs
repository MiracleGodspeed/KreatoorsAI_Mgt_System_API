using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Data.Dtos
{
#pragma warning disable
    public class SignupDto
    {
        public string emailaddress { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string? Password { get; set; }
    }
}
