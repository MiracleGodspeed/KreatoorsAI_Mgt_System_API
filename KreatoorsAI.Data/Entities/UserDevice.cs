using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Data.Entities
{
#pragma warning disable
    public class UserDevice : BaseModel
    {
        public Guid UserId { get; set; }
        public string? DeviceName { get; set; }
        public string DeviceId { get; set; }
        public DateTime LastLoggedIn { get; set; }
        public string? JwtToken { get; set; }
        [ForeignKey(nameof(UserId))]
        public Users Users { get; set; }
    }
}
