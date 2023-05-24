using Azure.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PSKVideoProjectBackend.Helpers;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Properties;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Security.Principal;

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

        public async Task<RegisteredUser> RegisterNewUser(UserToRegister userToRegister)
        {
            var registeredUser = new RegisteredUser(userToRegister);

            var result = await _apiDbContext.Users.AddAsync(registeredUser);
            await _apiDbContext.SaveChangesAsync();
            return result.Entity;
        }

        internal async Task<ClaimsPrincipal> Login(string username, string password)
        {
            //usernames must be unique
            var user = await _apiDbContext.Users.FirstOrDefaultAsync(el => el.Username == username);

            //user doesnt exists or password doesnt match
            if (user == null || !HashHelpers.VerifyPassword(password, user.Salt, user.PasswordHashed)) return null!;

            user.LastLoginDateTime = DateTime.Now;

            //Changing last login dateTime
            _apiDbContext.Users.Update(user);
            await _apiDbContext.SaveChangesAsync();

            // Create the authentication cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }

        internal bool CheckIfPasswordSecure(string password)
        {
            //length is 8 - 20
            if (password.Length < 8 || password.Length > 20) return false;

            //At least 1 uppercase character
            if (!Regex.IsMatch(password, "[A-Z]")) return false;

            //At least 1 lowercase character
            if (!Regex.IsMatch(password, "[a-z]")) return false;

            //At least 1 special character
            if (!Regex.IsMatch(password, "[!@#$%^&*(),.?\":{}|<>]")) return false;

            return true;
        }

        internal async Task<RegisteredUser> ChangePassword(string username, string password, string newPassword)
        {
            var user = await _apiDbContext.Users.FirstOrDefaultAsync(el => el.Username == username);

            //user doesnt exists or password doesnt match
            if (user == null || !HashHelpers.VerifyPassword(password, user.Salt, user.PasswordHashed)) return null!;

            user.Salt = HashHelpers.GenerateSalt();
            user.PasswordHashed = newPassword.HashPassword(user.Salt);

            var result = _apiDbContext.Update(user);
            await _apiDbContext.SaveChangesAsync();

            return result.Entity;
        }

        public Task<User GetUserInfo(string principalName)
        {
            int id = 0;

            if (!Int32.TryParse(principalName, out id)) return -1;

           
           var user = _apiDbContext.Users.FirstOrDefault(el => el.Id == id)


        }
}
}
