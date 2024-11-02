﻿using Microsoft.AspNetCore.Identity;

namespace BookStoreAPI.Models
{
    public class AppUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
