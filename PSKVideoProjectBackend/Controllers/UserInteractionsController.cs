using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Properties;
using PSKVideoProjectBackend.Repositories;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using PSKVideoProjectBackend.Models.Enums;
using PSKVideoProjectBackend.Interfaces;
using PSKVideoProjectBackend.Helpers;

namespace PSKVideoProjectBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserInteractionsController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly ILogger<UserInteractionsController> _logger;
        private readonly IObjectValidator _objectValidator;
        private readonly SignalRConnectionMapping _signalRConnectionMapper;

        public UserInteractionsController(ILogger<UserInteractionsController> logger, UserRepository userRepository, IObjectValidator objectValidator, SignalRConnectionMapping signalRConnectionMapping)
        {
            _userRepository = userRepository;
            _logger = logger;
            _objectValidator = objectValidator;
            _signalRConnectionMapper = signalRConnectionMapping;
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

                var validateInfo = _objectValidator.IsValid(userToRegister);
                if (validateInfo != InfoValidation.Validated) return BadRequest(validateInfo.ToString());

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
                    IsPersistent = true
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
                if (User.Identity == null || !User.Identity.IsAuthenticated || String.IsNullOrEmpty(User.Identity.Name))
                    return StatusCode(StatusCodes.Status401Unauthorized);

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                //Disconnect all user's sessions from notificationHub
                _signalRConnectionMapper.DisconnectUser(User.Identity.Name);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
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

        [HttpGet("GetUserInfo")]
        public async Task<ActionResult<UserInfo>> GetUserInfo()
        {
            try
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated) return StatusCode(StatusCodes.Status401Unauthorized);

                if (String.IsNullOrEmpty(User.Identity.Name)) return StatusCode(StatusCodes.Status404NotFound);

                var userInfo = _userRepository.GetUserInfo(User.Identity.Name);

                if (userInfo == null) return StatusCode(StatusCodes.Status404NotFound, Resources.UserNotFoundInDb);

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPut("UpdateUserInfo")]
        public async Task<ActionResult<RegisteredUser>> UpdateUserInfo([FromBody, Required] UserInfo userInfo, bool overwriteChanges = false)
        {
            try
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated ||
                    String.IsNullOrEmpty(User.Identity.Name) || !uint.TryParse(User.Identity.Name, out uint userId))
                    return StatusCode(StatusCodes.Status401Unauthorized);

                var validated = _objectValidator.IsValid(userInfo);
                if (validated != InfoValidation.Validated) return StatusCode(StatusCodes.Status400BadRequest, validated.ToString());

                if (!overwriteChanges && !_userRepository.ValidateInfoVersions(User.Identity.Name, userInfo))
                    return StatusCode(StatusCodes.Status409Conflict, Resources.ErrUserInfoVersions);

                var usernameTakenBy = await _userRepository.GetUserIdByUsername(userInfo.Username);

                if (usernameTakenBy != 0 && usernameTakenBy != userId)
                    return StatusCode(StatusCodes.Status409Conflict, InfoValidation.UsernameTaken.ToString());

                var updatedInfo = await _userRepository.UpdateUserInfo(userInfo, userId);

                return Ok(updatedInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}