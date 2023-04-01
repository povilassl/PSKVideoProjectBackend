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
        public async Task<ActionResult<UploadedVideo>> UploadVideo([FromBody] VideoToUpload video)
        {
            try
            {
                var result = await _videoRepository.UploadVideo(video);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);

                return Ok(result);
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
    }
}