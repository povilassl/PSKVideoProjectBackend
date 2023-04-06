using Microsoft.AspNetCore.Mvc;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Properties;
using PSKVideoProjectBackend.Repositories;

namespace PSKVideoProjectBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly VideoRepository _videoRepository;

        public VideoController(VideoRepository videoRepository)
        {
            _videoRepository = videoRepository;
        }

        [HttpGet("GetListOfVideos")]
        public async Task<ActionResult<IEnumerable<UploadedVideo>>> GetListOfVideos(int startIndex = 0, int endIndex = 20)
        {
            try
            {
                var videos = await _videoRepository.GetListOfVideos(startIndex, endIndex);

                if (videos == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrRetrieveDbOutOfRange);

                return Ok(videos);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.Exception + " : " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrRetrieveFromDB);
            }
        }

        //Sitas dar neaisku, ar bus open, ar closed - bet kuriuo atveju kazkokia autetifikacija praverstu
        [HttpPost("UploadVideo")]
        public async Task<ActionResult<UploadedVideo>> UploadVideo([FromForm] VideoToUpload video)
        {
            try
            {
                //all info checks

                if (String.IsNullOrEmpty(video.Username) || String.IsNullOrEmpty(video.VideoName) || String.IsNullOrEmpty(video.Description) ||
                    video.VideoFile == null || video.ThumbnailImage == null)
                    return StatusCode(StatusCodes.Status400BadRequest, Resources.ErrNotAllInfo);

                if (video.ThumbnailImage.ContentType != "image/jpeg" && video.ThumbnailImage.ContentType != "image/png")
                    return StatusCode(StatusCodes.Status400BadRequest, Resources.IncorrectImageFormat);

                if (video.ThumbnailImage.Length > Math.Pow(10, 6)) //1 Mb
                    return StatusCode(StatusCodes.Status400BadRequest, Resources.ErrImageTooLarge);

                if (video.VideoFile.Length > 10 * Math.Pow(10, 6))// 10 Mb
                    return StatusCode(StatusCodes.Status400BadRequest, Resources.ErrVideoTooLarge);

                var res = await _videoRepository.UploadVideo(video);

                if (res == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);

                return Ok(res);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.Exception + " : " + ex.Message);
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
                Console.WriteLine(Resources.Exception + " : " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpPost("TestingVideoUpload")]
        public async Task<ActionResult<UploadedVideo>> TestingVideoUpload([FromForm] VideoToUpload video)
        {
            try
            {
                if (video.ThumbnailImage == null) return StatusCode(StatusCodes.Status400BadRequest, Resources.ErrNotAllInfo);

                if (video.ThumbnailImage.ContentType != "image/jpeg" && video.ThumbnailImage.ContentType != "image/png")
                    return StatusCode(StatusCodes.Status400BadRequest, Resources.IncorrectImageFormat);

                var result = await _videoRepository.TestingVideoUpload(video);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.Exception + " : " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }
    }
}