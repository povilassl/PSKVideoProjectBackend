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
using PSKVideoProjectBackend.Models.Enums;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

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

        public async Task<uint> GetUserIdByUsername(string username)
        {
            var user = await _apiDbContext.Users.FirstOrDefaultAsync(el => el.Username == username);

            if (user == null) return 0;

            return user.Id;
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

        public UserInfo GetUserInfo(string principalName)
        {

            if (!uint.TryParse(principalName, out uint id)) return null!;

            var user = _apiDbContext.Users.FirstOrDefault(el => el.Id == id);

            if (user is null) return null!;

            return new UserInfo(user);
        }

        internal InfoValidation ValidateInputUserInfo(UserToRegister userToRegister)
        {
            var userInfo = new UserInfo() {
                Username = userToRegister.Username,
                EmailAddress = userToRegister.EmailAddress,
                FirstName = userToRegister.FirstName,
                LastName = userToRegister.LastName,
            };

            return ValidateInputUserInfo(userInfo);
        }

        internal InfoValidation ValidateInputUserInfo(UserInfo userInfo)
        {
            if (!ValidateEmail(userInfo.EmailAddress)) return InfoValidation.BadEmail;
            if (!ValidateUsername(userInfo.Username)) return InfoValidation.BadUsername;
            if (!ValidateNameLastName(userInfo.FirstName)) return InfoValidation.BadFirstName;
            if (!ValidateNameLastName(userInfo.LastName)) return InfoValidation.BadLastName;

            return InfoValidation.Validated;
        }

        internal bool ValidateUsername(string username)
        {
            if (String.IsNullOrEmpty(username)) return false;

            if (username.Length <= 4 || username.Length > 20) return false;

            // Username should contain characters a-z A-Z 0-9 
            string allowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return username.All(c => allowedCharacters.Contains(c));
        }

        internal bool ValidateEmail(string email)
        {
            if (String.IsNullOrEmpty(email)) return false;

            // Email should be of email format
            var emailRegex = new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");
            return emailRegex.IsMatch(email);
        }

        internal bool ValidateNameLastName(string name)
        {
            if (String.IsNullOrEmpty(name)) return false;

            if (name.Length > 50) return false;

            // Name and lastname should only contain characters a-z A-Z
            Regex nameRegex = new Regex("^[a-zA-Z]+$");
            return nameRegex.IsMatch(name);
        }

        internal async Task<UserInfo> UpdateUserInfo(UserInfo userInfo, uint userId)
        {
            var existingUser = _apiDbContext.Users.FirstOrDefault(el => el.Id == userId)!;

            existingUser.Username = userInfo.Username;
            existingUser.EmailAddress = userInfo.EmailAddress;
            existingUser.FirstName = userInfo.FirstName;
            existingUser.LastName = userInfo.LastName;
            existingUser.LastInfoUpdateDateTime = userInfo.LastInfoUpdateDateTime;
            existingUser.LastInfoUpdateDateTime = DateTime.Now;

            _apiDbContext.Users.Update(existingUser);
            await _apiDbContext.SaveChangesAsync();

            return new UserInfo(existingUser);
        }

        internal bool ValidateInfoVersions(string principalName, UserInfo userInfo)
        {
            if (!uint.TryParse(principalName, out uint id)) return false;

            var user = _apiDbContext.Users.FirstOrDefault(el => el.Id == id);

            if (user == null) return false;

            if (user.LastInfoUpdateDateTime != userInfo.LastInfoUpdateDateTime) return false;

            return true;
        }
    }
}
