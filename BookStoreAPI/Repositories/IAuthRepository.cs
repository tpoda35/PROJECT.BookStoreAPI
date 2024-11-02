using BookStoreAPI.Data;
using BookStoreAPI.Dtos;
using BookStoreAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Repositories
{
    public interface IAuthRepository
    {
        bool CheckRefreshToken(AppUser appUser);
        Task<(bool IsSuccess, RefreshTokenResponseDto? response)> AddTokenToUser(AppUser appUser, string refreshToken);
        Task<bool> RevokeToken(AppUser user);
    }

    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IAuthRepository> _logger;
        public AuthRepository(ApplicationDbContext context, ILogger<IAuthRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool IsSuccess, RefreshTokenResponseDto? response)> AddTokenToUser(AppUser appUser, string refreshToken)
        {
            try
            {
                appUser.RefreshToken = refreshToken;
                appUser.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
                await _context.SaveChangesAsync();
                return (true, new RefreshTokenResponseDto
                {
                    RefreshToken = refreshToken,
                    ExpiratonDate = appUser.RefreshTokenExpiry
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("An error ocurred during AddTokenToUser. Details: {details}", ex.Message);
                return (false, null);
            }
        }

        public bool CheckRefreshToken(AppUser appUser)
        {
            return appUser.RefreshToken is not null && appUser.RefreshTokenExpiry > DateTime.UtcNow;
        }

        public async Task<bool> RevokeToken(AppUser user)
        {
            try
            {
                if (_context.Entry(user).State == EntityState.Detached) _context.Attach(user);

                user.RefreshToken = null;
                user.RefreshTokenExpiry = DateTime.MinValue;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Something went wrong during Revoking a Token. Details: {details}",
                    ex.Message);
                return false;
            }
        }
    }
}