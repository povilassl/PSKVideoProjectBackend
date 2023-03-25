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
        public async Task<ActionResult<UploadedVideo>> UploadVideo(UploadedVideo video)
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
    }
}