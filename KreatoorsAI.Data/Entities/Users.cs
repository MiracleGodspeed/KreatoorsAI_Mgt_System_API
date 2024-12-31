using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Data.Entities
{
#pragma warning disable
    public class Users
    {
        public Guid Id { get; set; }
        public string emailaddress { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string? profileImage { get; set; }
        public string? PasswordHash { get; set; }
        public bool active { get; set; }
        public DateTime addedOn { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; }

    }
}
