using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Data.Models
{
#pragma warning disable
    public class AuthResponse
    {
        public Guid UserId { get; set; }
        public string email { get; set; }
        public string AuthToken { get; set; }
        public string FullName { get; set; }
    }
}
