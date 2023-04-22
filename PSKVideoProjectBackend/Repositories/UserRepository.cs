using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Properties;
using System.Diagnostics;

namespace PSKVideoProjectBackend.Repositories
{
    public class UserRepository
    {
        private readonly ApiDbContext _apiDbContext;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ApiDbContext apiDbContext, ILogger<UserRepository> logger)
        {
            _apiDbContext = apiDbContext;
            _logger = logger;
        }

        public async Task<bool> CheckIfUsernameTaken(string username)
        {
            return _apiDbContext.Users.Any(el => el.Username == username);
        }

        public async Task<User> RegisterNewUser(User user)
        {
            var result = await _apiDbContext.Users.AddAsync(user);
            await _apiDbContext.SaveChangesAsync();
            return result.Entity;
        }
    }
}
