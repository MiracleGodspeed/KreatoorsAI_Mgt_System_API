using KreatoorsAI.Data.Dtos;
using KreatoorsAI.Data.Entities;
using KreatoorsAI.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Core.Services.Interfaces
{
    public interface IAccountService
    {
        Task<BaseResponse> CreateUserAccountAsync(SignupDto signupRequest, string deviceId);
        string GenerateDeviceId(HttpContext context);
        ClaimsPrincipal ValidateToken(string token);
        Task<BaseResponse> MarkEmailAsVerified(string email);
        Task<AuthResponse> LoginUser(LoginRequest loginDTO, string deviceId);
        Task<List<FetchUserDeviceDto>> FetchAllDevicesByUserId(Guid userId);
        Task<bool> LogoutDevice(Guid userId, string deviceId);
        Task<bool> RemoveDevice(Guid userId, string deviceId);
        Guid GetUserIdFromToken(string token);
        Task<GetUserDetailsDto> GetUserDetails(Guid userId);
        Task<bool> UpdateProfile(UpdateProfileDto dto, string filePath, string directory, Guid UserId);
    }
}
