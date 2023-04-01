using Microsoft.AspNetCore.Mvc;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Properties;
using PSKVideoProjectBackend.Repositories;

namespace PSKVideoProjectBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoInteractionsController : ControllerBase
    {
        private readonly VideoRepository _videoRepository;

        public VideoInteractionsController(VideoRepository videoRepository)
        {
            _videoRepository = videoRepository;
        }

        [HttpPost("AddLike")]
        public async Task<ActionResult<UploadedVideo>> AddLike([FromBody] int videoId)
        {
            try
            {
                var result = await _videoRepository.LikeAVideo(videoId, true, true);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.Exception + " : " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpPost("RemoveLike")]
        public async Task<ActionResult<UploadedVideo>> RemoveLike([FromBody] int videoId)
        {
            try
            {
                var result = await _videoRepository.LikeAVideo(videoId, true, false);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.Exception + " : " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpPost("AddDislike")]
        public async Task<ActionResult<UploadedVideo>> AddDislike([FromBody] int videoId)
        {
            try
            {
                var result = await _videoRepository.LikeAVideo(videoId, false, true);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.Exception + " : " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpPost("RemoveDislike")]
        public async Task<ActionResult<UploadedVideo>> RemoveDislike([FromBody] int videoId)
        {
            try
            {
                var result = await _videoRepository.LikeAVideo(videoId, false, false);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.Exception + " : " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpPost("CommentOnAVideo")]
        public async Task<ActionResult<VideoComment>> CommentOnAVideo([FromBody] VideoComment videoComment)
        {
            try
            {
                if (String.IsNullOrEmpty(videoComment.Comment) || String.IsNullOrEmpty(videoComment.Username))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, Resources.ErrNotAllInfo);
                }

                videoComment.Id = 0;
                videoComment.CommentId = 0;
                videoComment.HasComments = false;

                var result = await _videoRepository.AddComment(videoComment);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.Exception + " : " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpPost("ReplyToAComment")]
        public async Task<ActionResult<VideoComment>> ReplyToAComment([FromBody] VideoComment videoComment)
        {
            try
            {
                if (String.IsNullOrEmpty(videoComment.Comment) || String.IsNullOrEmpty(videoComment.Username))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, Resources.ErrNotAllInfo);
                }

                videoComment.VideoId = 0;

                var result = await _videoRepository.AddComment(videoComment);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrCommentNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.Exception + " : " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpGet("GetVideoComments")]
        public async Task<ActionResult<UploadedVideo>> GetVideoComments(int videoId)
        {
            try
            {
                var result = await _videoRepository.GetComments(videoId, true);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.Exception + " : " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpGet("GetCommentReplies")]
        public async Task<ActionResult<UploadedVideo>> GetCommentReplies(int commentId)
        {
            try
            {
                var result = await _videoRepository.GetComments(commentId, false);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.Exception + " : " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpPost("IncreaseViewCount")]
        public async Task<ActionResult<UploadedVideo>> IncreaseViewCount([FromBody] int videoId)
        {
            try
            {
                var result = await _videoRepository.IncreaseViewCount(videoId);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

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