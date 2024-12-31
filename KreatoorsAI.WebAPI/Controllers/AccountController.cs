using KreatoorsAI.Core.Infrastructure;
using KreatoorsAI.Core.Services.Interfaces;
using KreatoorsAI.Data.Dtos;
using KreatoorsAI.WebAPI.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KreatoorsAI.WebAPI.Controllers
{
#pragma warning disable
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IWebHostEnvironment _webHostingEnvironment;

        public AccountController(IAccountService accountService, IWebHostEnvironment webHostingEnvironment)
        {
            _accountService = accountService;
            _webHostingEnvironment = webHostingEnvironment;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateAccount([FromBody] SignupDto dto)
        {
            if (!string.IsNullOrEmpty(dto.emailaddress) && !string.IsNullOrEmpty(dto.Password))
            {
                var deviceId = _accountService.GenerateDeviceId(HttpContext);
                var user = await _accountService.CreateUserAccountAsync(dto, deviceId);
                return Ok(user);
            }
            return BadRequest("Enter email address and password");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UserLogin([FromBody] LoginRequest dto)
        {
            if (!string.IsNullOrEmpty(dto.emailaddress) && !string.IsNullOrEmpty(dto.password))
            {
                var deviceId = _accountService.GenerateDeviceId(HttpContext);
                var user = await _accountService.LoginUser(dto, deviceId);
                return Ok(user);
            }
            return BadRequest("Enter email address and password");
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            try
            {
                var claimsPrincipal = _accountService.ValidateToken(token);

                var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;

                if (email != null)
                {
                    // Mark the email as verified
                    var completeActivation = await _accountService.MarkEmailAsVerified(email);
                    return Ok(completeActivation);
                }

                return BadRequest("Invalid token.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Verification failed: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<IActionResult> FetchUserDetails()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var userId = _accountService.GetUserIdFromToken(token);
                var res = await _accountService.GetUserDetails(userId);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest($"Verification failed: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            if (ModelState.IsValid)
            {
                var directory = Path.Combine("Resources", "ProfileImages");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var userId = _accountService.GetUserIdFromToken(token);
                var filePath = Path.Combine(_webHostingEnvironment.ContentRootPath, directory);
                return Ok(await _accountService.UpdateProfile(dto, filePath, directory, userId));
            }

            throw new Exception("Model validation error");
        }


        [Authorize]
        [HttpGet("[action]")]
        public async Task<IActionResult> FetchMyDevices()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var userId = _accountService.GetUserIdFromToken(token);
                var res = await _accountService.FetchAllDevicesByUserId(userId);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest($"Verification failed: {ex.Message}");
            }
        }


        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> LogoutDevice(string deviceId)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = _accountService.GetUserIdFromToken(token);

            var result = await _accountService.LogoutDevice(userId, deviceId);

            if (result)
                return Ok("Logged out successfully!");

            return BadRequest("Logout failed. Device not found or unauthorized.");
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveDevice(string deviceId)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = _accountService.GetUserIdFromToken(token);

            var result = await _accountService.RemoveDevice(userId, deviceId);

            if (result)
                return Ok("Device removed successfully!");

            return BadRequest("Delete failed. Device not found or unauthorized.");
        }

        private Guid GetUserIdFromJwt()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var userIdClaim = identity?.FindFirst(CustomClaim.USER_ID);


            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            return Guid.Parse(userIdClaim.Value);
        }

    }


}
