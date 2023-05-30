using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSKVideoProjectBackend.Interfaces;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Models.Enums;
using PSKVideoProjectBackend.Properties;
using PSKVideoProjectBackend.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;

namespace PSKVideoProjectBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoInteractionsController : ControllerBase
    {
        private readonly VideoRepository _videoRepository;
        private readonly ILogger<VideoInteractionsController> _logger;

        public VideoInteractionsController(VideoRepository videoRepository, ILogger<VideoInteractionsController> logger)
        {
            _videoRepository = videoRepository;
            _logger = logger;
        }

        [HttpPost("AddLike")]
        public async Task<ActionResult<UploadedVideo>> AddLike([Required] uint videoId)
        {
            try
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated) return StatusCode(StatusCodes.Status401Unauthorized);

                var user = await _videoRepository.GetUserByPrincipal(User);
                if (user == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.UserNotFoundInDb);

                var result = await _videoRepository.LikeAVideo(videoId, true, true, user);
                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpPost("RemoveLike")]
        public async Task<ActionResult<UploadedVideo>> RemoveLike([Required] uint videoId)
        {
            try
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated) return StatusCode(StatusCodes.Status401Unauthorized);

                var user = await _videoRepository.GetUserByPrincipal(User);
                if (user == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.UserNotFoundInDb);

                var result = await _videoRepository.LikeAVideo(videoId, true, false, user);
                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpPost("AddDislike")]
        public async Task<ActionResult<UploadedVideo>> AddDislike([Required] uint videoId)
        {
            try
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated) return StatusCode(StatusCodes.Status401Unauthorized);

                var user = await _videoRepository.GetUserByPrincipal(User);
                if (user == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.UserNotFoundInDb);

                var result = await _videoRepository.LikeAVideo(videoId, false, true, user);
                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpPost("RemoveDislike")]
        public async Task<ActionResult<UploadedVideo>> RemoveDislike([Required] uint videoId)
        {
            try
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated) return StatusCode(StatusCodes.Status401Unauthorized);

                var user = await _videoRepository.GetUserByPrincipal(User);
                if (user == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.UserNotFoundInDb);

                var result = await _videoRepository.LikeAVideo(videoId, false, false, user);
                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        [HttpPost("CommentOnAVideo")]
        public async Task<ActionResult<VideoComment>> CommentOnAVideo([Required] VideoComment videoComment)
        {
            try
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated) return StatusCode(StatusCodes.Status401Unauthorized);

                if (!ModelState.IsValid) return BadRequest(ModelState);

                var user = await _videoRepository.GetUserByPrincipal(User);
                if (user == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.UserNotFoundInDb);

                videoComment.Id = 0;
                videoComment.CommentId = 0;
                videoComment.HasComments = false;

                var result = await _videoRepository.AddComment(videoComment, user);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        /// <summary>
        /// Reply to a specific comment
        /// </summary>
        /// <param name="videoComment"></param>
        /// <returns></returns>
        [HttpPost("ReplyToAComment")]
        public async Task<ActionResult<VideoComment>> ReplyToAComment([Required] VideoComment videoComment)
        {
            try
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated) return StatusCode(StatusCodes.Status401Unauthorized);

                if (!ModelState.IsValid) return BadRequest(ModelState);

                var user = await _videoRepository.GetUserByPrincipal(User);
                if (user == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.UserNotFoundInDb);

                videoComment.VideoId = 0;

                var result = await _videoRepository.AddComment(videoComment, user);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrCommentNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        /// <summary>
        /// Gets comments for a specific video
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("GetVideoComments")]
        public async Task<ActionResult<UploadedVideo>> GetVideoComments([Required] uint videoId)
        {
            try
            {
                var result = await _videoRepository.GetComments(videoId, true);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        /// <summary>
        /// Gets Replies for a specific comment
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("GetCommentReplies")]
        public async Task<ActionResult<UploadedVideo>> GetCommentReplies([Required] uint commentId)
        {
            try
            {
                var result = await _videoRepository.GetComments(commentId, false);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        /// <summary>
        /// Used to increase video view count by 1
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("IncreaseViewCount")]
        public async Task<ActionResult<UploadedVideo>> IncreaseViewCount([Required] uint videoId)
        {
            try
            {
                var result = await _videoRepository.IncreaseViewCount(videoId);

                if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrVideoNotFoundById);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        /// <summary>
        /// Used to get count of all uploaded videos
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("GetCountOfAllVideos")]
        public async Task<ActionResult<uint>> GetCountOfAllVideos()
        {
            try
            {
                var result = await _videoRepository.GetCountOfAllVideos();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        /// <summary>
        /// Used to get reaction for a video (like, dislike or none)
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("GetVideoReaction")]
        public async Task<ActionResult<uint>> GetVideoReaction([Required] uint videoId)
        {
            try
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated) return Ok(VideoReactionEnum.None);

                var reaction = await _videoRepository.GetVideoReaction(User, videoId);

                if (reaction == null) return Ok(VideoReactionEnum.None);

                return Ok(reaction.Reaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }
    }
}