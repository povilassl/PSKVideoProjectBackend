using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Properties;
using PSKVideoProjectBackend.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace PSKVideoProjectBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserInteractionsController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly ILogger<UserInteractionsController> _logger;

        public UserInteractionsController(ILogger<UserInteractionsController> logger, UserRepository userRepository)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Check if username is already taken by someone
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>True if taken, false if not</returns>
        [AllowAnonymous]
        [HttpGet("CheckIfUsernameTaken")]
        public async Task<ActionResult<bool>> CheckIfUsernameTaken([Required] string username)
        {
            try
            {
                var result = await _userRepository.CheckIfUsernameTaken(username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [AllowAnonymous]
        [HttpPost("RegisterNewUser")]
        public async Task<ActionResult<RegisteredUser>> RegisterNewUser([FromBody] UserToRegister userToRegister)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var passSecure = _userRepository.CheckIfPasswordSecure(userToRegister.Password);
                if (!passSecure) return StatusCode(StatusCodes.Status400BadRequest, Resources.PasswordNotSecureErr);

                var usernameTaken = await _userRepository.CheckIfUsernameTaken(userToRegister.Username);
                if (usernameTaken) return StatusCode(StatusCodes.Status409Conflict, Resources.UsernameTakenErr);

                var registeredUser = await _userRepository.RegisterNewUser(userToRegister);
                if (registeredUser == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);

                return Ok(registeredUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [AllowAnonymous]
        [HttpGet("Login")]
        public async Task<ActionResult<RegisteredUser>> Login([Required] string username, [Required] string password)
        {
            try
            {
                var principal = await _userRepository.Login(username, password);

                if (principal == null) return StatusCode(StatusCodes.Status401Unauthorized, Resources.CredentialsDontMatchtErr);

                var authProperties = new AuthenticationProperties {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(10),
                    IsPersistent = true //TODO: Add checkbox in frontend
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [AllowAnonymous]
        [HttpGet("LogOut")]
        public async Task<ActionResult<RegisteredUser>> LogOut()
        {
            try
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated) return StatusCode(StatusCodes.Status401Unauthorized);

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [AllowAnonymous]
        [HttpGet("ChangePassword")]
        public async Task<ActionResult<RegisteredUser>> ChangePassword([Required] string username, [Required] string password, [Required] string newPassword)
        {
            try
            {
                var user = await _userRepository.ChangePassword(username, password, newPassword);

                if (user == null) return StatusCode(StatusCodes.Status401Unauthorized, Resources.CredentialsDontMatchtErr);

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }
    }
}