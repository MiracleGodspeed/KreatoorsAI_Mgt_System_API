using KreatoorsAI.Core.Infrastructure;
using KreatoorsAI.Core.Services.Interfaces;
using KreatoorsAI.Data;
using KreatoorsAI.Data.Dtos;
using KreatoorsAI.Data.Entities;
using KreatoorsAI.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace KreatoorsAI.Core.Services
{
#pragma warning disable
    public class AccountService : Repository<AccountService>, IAccountService
    {
        private readonly IEmailService _emailService;
        private readonly IAssetUploadService _assetUploadService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;

        public AccountService(KreatoorsDbContext context, IEmailService emailService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IAssetUploadService assetUploadService) : base(context)
        {
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _config = configuration;
            _assetUploadService = assetUploadService;
        }

        public async Task<BaseResponse> CreateUserAccountAsync(SignupDto signupRequest, string deviceId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(p => p.emailaddress == signupRequest.emailaddress);

            if (user != null)
                return new BaseResponse { Status = false, Message = $"A User with {signupRequest.emailaddress} already exists" };

          
                try
                {
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(signupRequest.Password);

                    user = new Users()
                    {
                        emailaddress = signupRequest.emailaddress.ToLower(),
                        firstname = signupRequest.firstname,
                        lastname = signupRequest.lastname,
                        addedOn = DateTime.UtcNow,
                        active = false,
                        PasswordHash = hashedPassword,
                    };
                    _context.Add(user);
                    await _context.SaveChangesAsync();

                    //Generate email token
                    var emailToken = GenerateEmailVerificationToken(signupRequest.emailaddress);

                    var emailBody = $"<div style='font-size: 13px;'>" +
                                    $"<p>Hello {user.lastname} {user.firstname},</p>" +
                                    $"<p>Thank you for signing up with KreatoorsAI. To complete your registration, please use the activation link below</p>" +
                                    $"<a href='https://kreatoors-ai.netlify.app/activate-account?token={emailToken}'>Click here to activate account</a>" +
                                    $"<p>This link is valid for the next 5 minutes. If you didn’t request this code, please ignore this email.</p>" +
                                    $"</div>";


                    await CheckOrAddDevice(user.Id, "", deviceId);



                    //send activation mail
                    await _emailService.SendEmailAsync(signupRequest.emailaddress, "Account Activation", emailBody);

                    return new BaseResponse { Status = true, Message = signupRequest.emailaddress };
                }
                catch (Exception ex)
                {
                    // Rollback the transaction if anything fails
                    throw ex;
                }
        }

        public async Task<AuthResponse> LoginUser(LoginRequest loginDTO, string deviceId)
        {
            try
            {
                AuthResponse authResponse = new AuthResponse();

                Users existingUser = await _context.Users.FirstOrDefaultAsync(x => x.emailaddress == loginDTO.emailaddress);
                if (existingUser == null)
                    throw new Exception("Account not Found");

                if (string.IsNullOrEmpty(existingUser.PasswordHash))
                    throw new Exception("Username or password is incorrect");

                if (!BCrypt.Net.BCrypt.Verify(loginDTO.password, existingUser.PasswordHash))
                    throw new Exception("Username or password is incorrect");

                if (existingUser != null && !existingUser.active)
                {
                    throw new Exception("Account not active");
                }
                if (existingUser != null && existingUser.active)
                {
                    authResponse = await AuthenticateUser(existingUser);

                    //manage user current device
                    await CheckOrAddDevice(existingUser.Id, authResponse.AuthToken, deviceId);

                    return authResponse;
                }
                return authResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AuthResponse> AuthenticateUser(Users user)
        {

            AuthResponse authResponseDTO = new AuthResponse();
            authResponseDTO.AuthToken = GenerateJSONWebToken(user);
            authResponseDTO.UserId = user.Id;
            authResponseDTO.email = user?.emailaddress;
            authResponseDTO.FullName = user.firstname + " " + user.lastname;
            return authResponseDTO;
        }


        public async Task<bool> CheckOrAddDevice(Guid userId, string jwtToken, string deviceId)
        {
            var existingDevice = await _context.UserDevices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceId);

            if (existingDevice != null)
            {
                existingDevice.JwtToken = jwtToken;
                existingDevice.LastLoggedIn = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }

            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

            var newDevice = new UserDevice
            {
                UserId = userId,
                DeviceName = userAgent,
                DeviceId = deviceId,
                LastLoggedIn = DateTime.UtcNow,
                JwtToken = jwtToken,
                active = true
            };

            _context.Add(newDevice);
            await _context.SaveChangesAsync();
            return false;
        }
        public async Task<BaseResponse> MarkEmailAsVerified(string email)
        {
            try
            {
                var getUser = await _context.Users.FirstOrDefaultAsync(x => x.emailaddress == email);
                if (getUser == null)
                    throw new Exception("user with email address not found");

                getUser.active = true;
                _context.Update(getUser);
                await _context.SaveChangesAsync();

                return new BaseResponse { Status = true, Message = email };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
      
        public async Task<List<FetchUserDeviceDto>> FetchAllDevicesByUserId(Guid userId)
        {
            return await _context.UserDevices
                .Select(f => new FetchUserDeviceDto()
                {
                    UserId = f.UserId,
                    DeviceId = f.DeviceId, 
                    LastLoggedIn = f.LastLoggedIn.ToLongDateString() + " | " + f.LastLoggedIn.ToShortTimeString(),
                    DeviceName = f.DeviceName,
                    Active = f.active
                })
                .ToListAsync();
        }

        public async Task<GetUserDetailsDto> GetUserDetails(Guid userId)
        {
            return await _context.Users
                .Select(f => new GetUserDetailsDto()
                {
                    firstname = f.firstname,
                    lastname = f.lastname,
                    emailaddress = f.emailaddress,
                    profileImage =  f.profileImage,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateProfile(UpdateProfileDto dto, string filePath, string directory, Guid UserId)
        {
            var getUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (getUser == null)
                throw new Exception("User not found");

            string assetLink = string.Empty;

            if(dto.ImageFile != null)
                assetLink = await _assetUploadService.GetFileUploadLink(dto.ImageFile, filePath, directory, dto.firstname.Trim().ToLower());

            getUser.firstname = dto.firstname;
            getUser.lastname = dto.lastname;
            getUser.profileImage = !string.IsNullOrEmpty(assetLink) ? assetLink : getUser.profileImage;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LogoutDevice(Guid userId, string deviceId)
        {
            var device = await _context.UserDevices.FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.UserId == userId && x.active);
            if (device == null)
                return false;

            _context.Remove(device);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveDevice(Guid userId, string deviceId)
        {
            var device = await _context.UserDevices.FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.UserId == userId && x.active);
            if (device == null)
                return false;

            _context.Remove(device);
            await _context.SaveChangesAsync();

            return true;
        }

        public string GenerateDeviceId(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ipAddress}:{userAgent}"));
        }
        private string GenerateJSONWebToken(Users userInfo)
        {
            var expiryDate = DateTime.UtcNow.AddHours(48);
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new[]
            {
                new Claim(CustomClaim.USER_ID, userInfo.Id.ToString()),
                new Claim(CustomClaim.EMAIL, userInfo.emailaddress),
                new Claim(CustomClaim.USER_NAME, $"{userInfo.firstname} {userInfo.lastname}"),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(expiryDate).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiryDate,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
           
            return tokenHandler.WriteToken(token);
        }

        public string GenerateEmailVerificationToken(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Email, email)
            }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = _config["Jwt:Issuer"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Disable additional delay
                };
                return tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            }
            catch(Exception ex)
            {
                throw new Exception("expired/invalid token");
            }
        }

        public Guid GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == CustomClaim.USER_ID);

                if (userIdClaim != null)
                {
                    return Guid.Parse(userIdClaim.Value);
                }
                else
                {
                    throw new Exception("USER_ID claim not found or invalid");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation failed: {ex.Message}");
                throw;
            }
        }


    }
}
