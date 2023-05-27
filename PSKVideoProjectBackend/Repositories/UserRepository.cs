using Azure.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PSKVideoProjectBackend.Helpers;
using PSKVideoProjectBackend.Models;
using System.Security.Claims;

namespace PSKVideoProjectBackend.Repositories
{
    public class UserRepository
    {
        private readonly ApiDbContext _apiDbContext;

        public UserRepository(ApiDbContext apiDbContext)
        {
            _apiDbContext = apiDbContext;
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
