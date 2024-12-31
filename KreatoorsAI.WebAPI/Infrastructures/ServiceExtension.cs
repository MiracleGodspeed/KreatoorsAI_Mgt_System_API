using KreatoorsAI.Core.Services.Interfaces;
using KreatoorsAI.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace KreatoorsAI.WebAPI.Infrastructures
{
#pragma warning disable
    public static class ServiceExtension
    {
        public static void ConfigureService(this IServiceCollection services)
        {
            services.AddScoped(typeof(IAccountService), typeof(AccountService));
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAssetUploadService, AssetUploadService>();
            services.AddHttpContextAccessor();
        }
        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
        }
        public static void ConfigureEmailService(this IServiceCollection services, IConfiguration Configuration)
        {
            services
                .AddFluentEmail("kobokist@xplur.com")
                .AddMailGunSender(
                    Configuration.GetValue<string>("MailGun:domain"),
                    Configuration.GetValue<string>("MailGun:apiKey"),
                    FluentEmail.Mailgun.MailGunRegion.EU
                    )
                .AddRazorRenderer();
        }

    }
}
