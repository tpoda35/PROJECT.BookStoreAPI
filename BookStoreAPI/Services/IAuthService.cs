using BookStoreAPI.Dtos;
using BookStoreAPI.Models;
using BookStoreAPI.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookStoreAPI.Services
{
    public interface IAuthService
    {
        Task<bool> LoginUser(LoginDto loginData);
        Task<bool> RegisterUser(RegisterDto registerData);
        AccessTokenDto GenerateJwtToken(string email, string role);
        Task<bool> CheckRefreshToken(string email);
        ClaimsPrincipal GetClaimsPrincipal(string token);
        Task<(bool IsSuccess, RefreshTokenResponseDto? response)> AddTokenToUser(string Email, string refreshToken);
        Task<LoginResponseDto?> GetAuthDataFromUser(string Email, AccessTokenDto AccessToken);
        Task<AppUser?> GetUser(string email);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IAuthRepository _authRepository;

        public AuthService(UserManager<AppUser> userManager, IConfiguration configuration,
            IAuthRepository authRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _authRepository = authRepository;
        }

        public async Task<bool> LoginUser(LoginDto loginData)
        {
            var identityUser = await _userManager.FindByEmailAsync(loginData.Email);

            if (identityUser is null) return false;

            return await _userManager.CheckPasswordAsync(identityUser, loginData.Password);
        }

        public async Task<bool> RegisterUser(RegisterDto registerData)
        {
            var identityUser = new AppUser
            {
                UserName = registerData.Email,
                Email = registerData.Email
            };

            var result = await _userManager.CreateAsync(identityUser, registerData.Password);
            await _userManager.AddToRoleAsync(identityUser, "User");

            return result.Succeeded;
        }

        public AccessTokenDto GenerateJwtToken(string email, string role)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var jwtKey = _configuration.GetSection("Jwt:Key").Value ??
                throw new ArgumentException("Jwt Key is not configured.");
            var issuer = _configuration.GetSection("Jwt:Issuer").Value ??
                throw new ArgumentException("JWT Issuer is not configured.");
            var audience = _configuration.GetSection("Jwt:Audience").Value ??
                throw new ArgumentException("JWT Audience is not configured.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var signinCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            var securityToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(20),
                issuer: issuer,
                audience: audience,
                signingCredentials: signinCred);

            return new AccessTokenDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(securityToken),
                ExpirationDate = securityToken.ValidTo
            };
        }

        public async Task<bool> CheckRefreshToken(string email)
        {
            var appUser = await _userManager.FindByEmailAsync(email);
            if (appUser is null) return false;

            return _authRepository.CheckRefreshToken(appUser);
        }

        public ClaimsPrincipal GetClaimsPrincipal(string token)
        {
            var validation = new TokenValidationParameters
            {
                ValidateActor = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = false,
                RequireExpirationTime = true,
                ValidIssuer = _configuration.GetSection("Jwt:Issuer").Value,
                ValidAudience = _configuration.GetSection("Jwt:Audience").Value,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _configuration.GetSection("Jwt:Key").Value)),
                ClockSkew = TimeSpan.Zero
            };

            return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
        }

        public async Task<(bool IsSuccess, RefreshTokenResponseDto? response)> AddTokenToUser(string Email, string refreshToken)
        {
            var appUser = await _userManager.FindByEmailAsync(Email);
            if (appUser is null) return (false, null);

            return await _authRepository.AddTokenToUser(appUser, refreshToken);
        }

        public async Task<LoginResponseDto?> GetAuthDataFromUser(string Email, AccessTokenDto AccessToken)
        {
            var appUser = await _userManager.FindByEmailAsync(Email);
            if (appUser is null) return null;

            return new LoginResponseDto
            {
                AccessToken = AccessToken.AccessToken,
                AccessExpirationDate = AccessToken.ExpirationDate,
                RefreshToken = appUser.RefreshToken,
                RefreshExpirationDate = appUser.RefreshTokenExpiry
            };
        }

        public async Task<AppUser?> GetUser(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
    }
}