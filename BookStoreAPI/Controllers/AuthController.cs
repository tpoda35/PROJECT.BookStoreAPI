using Azure.Core;
using BookStoreAPI.Dtos;
using BookStoreAPI.Helpers;
using BookStoreAPI.Repositories;
using BookStoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IRoleService _roleService;
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthService authService, IRoleService roleService,
            IAuthRepository authRepository)
        {
            _authService = authService;
            _roleService = roleService;
            _authRepository = authRepository;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody]LoginDto loginData)
        {
            if (await _authService.LoginUser(loginData))
            {
                string? role = await _roleService.GetUserRoleAsync(loginData.Email);
                if (role is null) return NotFound("User role cannot be found.");

                var tokenDto = _authService.GenerateJwtToken(loginData.Email, role);
                if (!await _authService.CheckRefreshToken(loginData.Email))
                {
                    //Ha false, akkor ide jön, ami azt jelenti hogy kell új token
                    var refreshToken = RefreshTokenHelper.GenerateRefreshToken();
                    var result = await _authService.AddTokenToUser(loginData.Email, refreshToken);

                    if (!result.IsSuccess) return BadRequest(
                        "Something went wrong during saving the RefreshToken.");
                }

                return Ok(await _authService.GetAuthDataFromUser(loginData.Email, tokenDto));
            }

            return BadRequest();
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody]RegisterDto registerData)
        {
            if (await _authService.RegisterUser(registerData))
            {
                return Ok("User successfully registered.");
            }

            return BadRequest();
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshModel refreshData)
        {
            var principal = _authService.GetClaimsPrincipal(refreshData.AccessToken);

            if (principal?.Identity?.Name is null) return Unauthorized();

            var appUser = await _authService.GetUser(principal.Identity.Name);
            if (appUser is null ||
                appUser.RefreshToken != refreshData.RefreshToken ||
                appUser.RefreshTokenExpiry < DateTime.UtcNow) return Unauthorized();

            var role = await _roleService.GetUserRoleAsync(principal.Identity.Name);
            if (role is null) return NotFound("Role was not found.");

            var accessToken = _authService.GenerateJwtToken(principal.Identity.Name, role);

            return Ok(new LoginResponseDto
            {
                AccessToken = accessToken.AccessToken,
                AccessExpirationDate = accessToken.ExpirationDate,
                RefreshToken = refreshData.RefreshToken,
                RefreshExpirationDate = appUser.RefreshTokenExpiry
            });
        }

        [Authorize]
        [HttpDelete("Revoke")]
        public async Task<IActionResult> Revoke()
        {
            var username = HttpContext.User.Identity?.Name;
            if (username is null) return Unauthorized();

            var appUser = await _authService.GetUser(username);
            if (appUser is null) return Unauthorized();

            var result = await _authRepository.RevokeToken(appUser);
            if (!result) return BadRequest("Something went wrong.");

            return Ok();
        }
    }
}