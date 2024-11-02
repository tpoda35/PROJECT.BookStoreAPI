using BookStoreAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace BookStoreAPI.Services
{
    public interface IRoleService
    {
        Task SeedRolesAsync(); //Admin, User
        Task CreateAdminAccountAsync();
        Task<string?> GetUserRoleAsync(string Email);
    }

    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task CreateAdminAccountAsync()
        {
            if (await _userManager.FindByEmailAsync("admin@admin.com") is null)
            {
                var adminUser = new AppUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com"
                };

                await _userManager.CreateAsync(adminUser, "Admin123.");

                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        public async Task<string?> GetUserRoleAsync(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user is null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return roles.FirstOrDefault();
        }

        public async Task SeedRolesAsync()
        {
            string[] roles = { "User", "Admin" };
            IdentityResult identityResult;

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    identityResult = await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}