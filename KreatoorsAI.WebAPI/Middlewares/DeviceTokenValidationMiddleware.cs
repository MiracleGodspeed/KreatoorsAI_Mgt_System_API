using KreatoorsAI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KreatoorsAI.WebAPI.Middlewares
{
    public class DeviceTokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public DeviceTokenValidationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _dbContext = scope.ServiceProvider.GetRequiredService<KreatoorsDbContext>();

                    var userDevice = await _dbContext.UserDevices
                        .FirstOrDefaultAsync(d => d.JwtToken == token && d.active);

                    if (userDevice == null)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var response = new { message = "Unauthorized" };
                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
