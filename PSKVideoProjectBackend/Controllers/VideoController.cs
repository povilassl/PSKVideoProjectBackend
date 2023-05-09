using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Properties;
using PSKVideoProjectBackend.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace PSKVideoProjectBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly VideoRepository _videoRepository;
        private readonly ILogger<VideoController> _logger;

        public VideoController(VideoRepository videoRepository, ILogger<VideoController> logger)
        {
            _videoRepository = videoRepository;
            _logger = logger;
        }

        //TODO: galima bus padaryt specific selection of videos for logged in users
        [AllowAnonymous]
        [HttpGet("GetListOfVideos")]
        public ActionResult<IEnumerable<UploadedVideo>> GetListOfVideos(int startIndex = 0, int count = 20)
        {
            try
            {
                if (startIndex < 0) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrIndexLessThanZero);

                if (count <= 0) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrCountLessOrEqualZero);

                var videos = _videoRepository.GetListOfVideos(startIndex, count);

                if (videos == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrRetrieveDbOutOfRange);

                return Ok(videos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrRetrieveFromDB);
            }
        }

        [HttpPost("UploadVideo")]
        public async Task<ActionResult<UploadedVideo>> UploadVideo([FromForm, Required] VideoToUpload video, [FromForm, Required] bool sendEmail)
        {
            try
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated) return StatusCode(StatusCodes.Status401Unauthorized);

                if (video.ThumbnailImage.ContentType != "image/jpeg" && video.ThumbnailImage.ContentType != "image/png")
                    return StatusCode(StatusCodes.Status400BadRequest, Resources.IncorrectImageFormat);

                if (video.ThumbnailImage.Length > Math.Pow(10, 6)) //1 Mb
                    return StatusCode(StatusCodes.Status400BadRequest, Resources.ErrImageTooLarge);

                if (video.VideoFile.Length > 10 * Math.Pow(10, 6))// 10 Mb
                    return StatusCode(StatusCodes.Status400BadRequest, Resources.ErrVideoTooLarge);

                var user = await _videoRepository.GetUserByPrincipal(User);
                if (user == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.UserNotFoundInDb);

                var res = await _videoRepository.UploadVideo(video, user, sendEmail);

                return res ? Ok() : StatusCode(StatusCodes.Status500InternalServerError, Resources.VideoUploadNotificationFail);
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
        [HttpPost("UploadVideoTemp")]
        public async Task<ActionResult<IEnumerable<UploadedVideo>>> UploadVideoTemp([FromBody] List<UploadedVideo> videos)
        {
            try
            {
                var resList = new List<UploadedVideo>();

                foreach (var video in videos)
                {
                    video.Id = 0;
                    var result = await _videoRepository.UploadVideoTemp(video);

                    if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

                    resList.Add(result);
                }

                return Ok(resList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }
    }
}