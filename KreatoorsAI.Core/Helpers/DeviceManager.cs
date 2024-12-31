using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Core.Helpers
{
    public static class DeviceManager
    {
        public static string GenerateDeviceId(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ipAddress}:{userAgent}"));
        }

    }
}
