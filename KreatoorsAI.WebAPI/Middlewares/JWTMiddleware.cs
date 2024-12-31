using KreatoorsAI.Core.Infrastructure;
using KreatoorsAI.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace KreatoorsAI.WebAPI.Middlewares
{
#pragma warning disable

    public class JWTMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;


        public JWTMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
            {
                AttachAccountToContext(context, token);
            }
            await _next(context);
        }

        private void AttachAccountToContext(HttpContext context, string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                byte[] key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;
                var accountId = jwtToken.Claims.First(x => x.Type == CustomClaim.USER_ID).Value;

                //Check for Token expiration
                long tokenExpiry = long.Parse(jwtToken.Claims.First(x => x.Type == "exp").Value);
                long currentTimeInSeconds = GetCurrentUnixTimestampSeconds(DateTime.Now);
                if (currentTimeInSeconds > tokenExpiry)
                {
                    throw new Exception("Forbidden...Token has expired");
                }


                context.Items["UserId"] = accountId;
                context.Items["Token"] = token;
                context.Items["Email"] = jwtToken.Claims.First(x => x.Type == CustomClaim.EMAIL).Value;
                context.Items["TokenExpirationDateInSeconds"] = tokenExpiry;
                context.Items["CurrentTimeInSconds"] = currentTimeInSeconds;
            }
            catch (Exception)
            {
                // do nothing if jwt validation fails
            }
        }

    
        private static long GetCurrentUnixTimestampSeconds(DateTime localDateTime)
        {
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime univDateTime;
            univDateTime = localDateTime.ToUniversalTime();
            return (long)(univDateTime - UnixEpoch).TotalSeconds;
        }



    }

}
