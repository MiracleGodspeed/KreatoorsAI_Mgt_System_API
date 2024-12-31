using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Data.Dtos
{
    public class FetchUserDeviceDto
    {
        public Guid UserId { get; set; }
        public string? DeviceName { get; set; }
        public string DeviceId { get; set; }
        public string LastLoggedIn { get; set; }
        public bool Active { get; set; }
    }
}
