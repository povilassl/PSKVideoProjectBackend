using Microsoft.AspNetCore.Mvc;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Properties;
using PSKVideoProjectBackend.Repositories;
using System.Diagnostics;

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
        /// <param name="video"></param>
        /// <returns>True if taken, false if not</returns>
        [HttpGet("CheckIfUsernameTaken")]
        public async Task<ActionResult<bool>> CheckIfUsernameTaken(string username)
        {
            try
            {
                var result = await _userRepository.CheckIfUsernameTaken(username);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrRetrieveFromDB);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpGet("RegisterNewUser")]
        public async Task<ActionResult<User>> RegisterNewUser([FromBody] User user)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }


        /// <summary>
        /// Upload video by specifying all parameters
        /// </summary>
        /// <remarks>
        /// Since we don't yet need a complete endpoint (for v1), we can upload and encode videos manually and 
        /// then use this controller to add all info to the database
        /// </remarks>
        /// <param name="video"></param>
        /// <returns></returns>
        //[HttpPost("Register")]
        //public async Task<ActionResult<IEnumerable<UploadedVideo>>> UploadVideoTemp([FromBody] List<UploadedVideo> videos)
        //{
        //    try
        //    {
        //        var resList = new List<UploadedVideo>();

        //        foreach (var video in videos)
        //        {
        //            video.Id = 0;
        //            var result = await _videoRepository.UploadVideoTemp(video);

        //            if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

        //            resList.Add(result);
        //        }

        //        return Ok(resList);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
        //    }
        //}
    }
}