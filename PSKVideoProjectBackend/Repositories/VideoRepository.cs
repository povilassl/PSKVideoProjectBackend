﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PSKVideoProjectBackend.Managers;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Models.Enums;
using PSKVideoProjectBackend.Properties;
using System.Diagnostics;
using System.Security.Claims;

namespace PSKVideoProjectBackend.Repositories
{
    public class VideoRepository
    {
        private readonly ApiDbContext _apiDbContext;
        private readonly ILogger<VideoRepository> _logger;
        private readonly AzureMediaManager _azureMediaManager;

        public VideoRepository(ApiDbContext apiDbContext, ILogger<VideoRepository> logger, AzureMediaManager azureMediaManager)
        {
            _apiDbContext = apiDbContext;
            _logger = logger;
            _azureMediaManager = azureMediaManager;
        }

        //TODO: move to this: _apiDbContext.UploadedVideos.Skip(startIndex).Take(count).ToList();
        public IEnumerable<UploadedVideo>? GetListOfVideos(int startIndex, int count)
        {
            var allVideos = _apiDbContext.UploadedVideos.ToList();

            int endIndex = startIndex + count - 1;

            // If startIndex is greater than or equal to the length of the originalList, return a new empty list
            if (startIndex >= allVideos.Count) return new List<UploadedVideo>();

            // If endIndex is greater than or equal to the length of the originalList, set it to the last index of the originalList
            if (endIndex >= allVideos.Count) endIndex = allVideos.Count - 1;

            // Create a sublist from the originalList starting at the startIndex and ending at the endIndex
            var videosToReturn = allVideos.GetRange(startIndex, endIndex - startIndex + 1);

            foreach (var el in videosToReturn)
            {
                var user = _apiDbContext.Users.FirstOrDefault(user => user.Id == el.UserId);
                el.Username = user is null ? Resources.FillerVideoUsername : user.Username;
            }

            return videosToReturn;
        }

        /// <summary>
        /// Used to upload a video
        /// </summary>
        /// <param name="video"></param>
        /// <param name="user"></param>
        /// <param name="sendEmail"></param>
        /// <returns></returns>
        public async Task<bool> UploadVideo(VideoToUpload video, RegisteredUser user, bool sendEmail)
        {
            try
            {
                return await _azureMediaManager.UploadVideo(video, user, sendEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Temporary endpoint for multiple video object upload
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        public async Task<UploadedVideo> UploadVideoTemp(UploadedVideo video)
        {
            var result = await _apiDbContext.UploadedVideos.AddAsync(video);
            await _apiDbContext.SaveChangesAsync();
            return result.Entity;
        }

        /// <summary>
        /// Supports increasing and decreasing like and dislike count on a video
        /// </summary>
        /// <param name="videoId"></param>
        /// <param name="like"></param>
        /// <param name="increase"></param>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        public async Task<UploadedVideo> LikeAVideo(uint videoId, bool like, bool increase, RegisteredUser user)
        {
            var video = await _apiDbContext.UploadedVideos.FirstOrDefaultAsync(el => el.Id == videoId);

            if (video == null) return null!;

            var currentReaction = await _apiDbContext.VideoReactions.FirstOrDefaultAsync(el =>
                el.UserId == user.Id && el.VideoId == videoId);

            if (currentReaction == null) currentReaction = new() { Reaction = VideoReactionEnum.None };

            var newReaction = currentReaction.Reaction;

            if (like)
            {
                if (increase)
                {
                    if (video.LikeCount != uint.MaxValue && currentReaction.Reaction != VideoReactionEnum.Liked)
                    {
                        video.LikeCount++;

                        if (currentReaction.Reaction == VideoReactionEnum.Disliked) video.DislikeCount--;

                        newReaction = VideoReactionEnum.Liked;
                    }
                }
                else
                {
                    if (video.LikeCount != 0 && currentReaction.Reaction == VideoReactionEnum.Liked)
                    {
                        video.LikeCount--;
                        newReaction = VideoReactionEnum.None;
                    }
                }
            }
            else
            {
                if (increase)
                {
                    if (video.DislikeCount != uint.MaxValue && currentReaction.Reaction != VideoReactionEnum.Disliked)
                    {
                        video.DislikeCount++;

                        if (currentReaction.Reaction == VideoReactionEnum.Liked) video.LikeCount--;

                        newReaction = VideoReactionEnum.Disliked;
                    }
                }
                else
                {
                    if (video.DislikeCount != 0 && currentReaction.Reaction == VideoReactionEnum.Disliked)
                    {
                        video.DislikeCount--;
                        newReaction = VideoReactionEnum.None;
                    }
                }
            }

            if (newReaction != currentReaction.Reaction)
            {
                _apiDbContext.UploadedVideos.Update(video);

                if (newReaction == VideoReactionEnum.None && currentReaction.Id != 0)
                {
                    _apiDbContext.VideoReactions.Remove(currentReaction);
                }
                else
                {
                    currentReaction.Reaction = newReaction;

                    //if it has not beed added before
                    if (currentReaction.Id == 0)
                    {
                        currentReaction.UserId = user.Id;
                        currentReaction.VideoId = videoId;

                        var res = await _apiDbContext.VideoReactions.AddAsync(currentReaction);
                    }
                    else
                    {
                        _apiDbContext.VideoReactions.Update(currentReaction);
                    }
                }

                await _apiDbContext.SaveChangesAsync();
            }

            return video;
        }

        /// <summary>
        /// Used to add a comment or a reply to a comment
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public async Task<VideoComment> AddComment(VideoComment comment, RegisteredUser user)
        {
            comment.Id = 0;
            comment.HasComments = false;

            UploadedVideo? parentVideo = null;
            VideoComment? parentComment = null;

            if (comment.VideoId != 0)
            {
                parentVideo = await _apiDbContext.UploadedVideos.FirstOrDefaultAsync(el => el.Id == comment.VideoId);
                if (parentVideo == null) return null!;
            }
            else
            {
                parentComment = await _apiDbContext.Comments.FirstOrDefaultAsync(el => el.Id == comment.CommentId);
                if (parentComment == null) return null!;
            }

            comment.UserId = user.Id;
            comment.Username = user.Username;
            comment.DateTime = DateTime.Now;

            var result = await _apiDbContext.Comments.AddAsync(comment);
            await _apiDbContext.SaveChangesAsync();

            //updating flag that parent has comments
            if (parentVideo != null)
            {
                parentVideo.HasComments = true;
                _apiDbContext.UploadedVideos.Update(parentVideo);

            }
            else if (parentComment != null)
            {
                parentComment.HasComments = true;
                _apiDbContext.Comments.Update(parentComment);
            }

            await _apiDbContext.SaveChangesAsync();

            return result.Entity;
        }

        /// <summary>
        /// Used to get comments on a video or replies to a comment
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="isVideo"></param>
        /// <returns></returns>
        public async Task<IEnumerable<VideoComment>> GetComments(uint parentId, bool isVideo)
        {
            var allComments = await _apiDbContext.Comments.ToListAsync();
            var retList = new List<VideoComment>();

            if (isVideo)
            {
                retList = allComments
                    .Where(el => el.VideoId == parentId)
                    .OrderByDescending(el => el.DateTime)
                    .ToList();
            }
            else
            {
                retList = allComments
                    .Where(el => el.CommentId == parentId)
                    .OrderByDescending(el => el.DateTime)
                    .ToList();
            }

            foreach (var vid in retList)
            {
                var user = _apiDbContext.Users.FirstOrDefault(el => el.Id == vid.UserId);
                vid.Username = user is null ? Resources.FillerVideoUsername : user.Username;
            }

            return retList;
        }

        /// <summary>
        /// Used to increase a video view count by 1
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        /// TODO: kadangi open endpointas, tai teoriskai sita imanoma uzspammint request'ais
        public async Task<UploadedVideo> IncreaseViewCount(uint videoId)
        {
            var video = await _apiDbContext.UploadedVideos.FirstOrDefaultAsync(el => el.Id == videoId);

            if (video == null) return null!;

            if (video.ViewCount == uint.MaxValue) return video;

            video.ViewCount++;

            _apiDbContext.UploadedVideos.Update(video);
            await _apiDbContext.SaveChangesAsync();

            return video;
        }

        /// <summary>
        /// Returns count of all uploaded videos
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetCountOfAllVideos()
        {
            return await _apiDbContext.UploadedVideos.CountAsync();
        }

        internal async Task<VideoReaction> GetVideoReaction(ClaimsPrincipal claimsPrincipal, uint videoId)
        {
            var loggedInUser = await GetUserByPrincipal(claimsPrincipal);

            if (loggedInUser == null) return null;

            return await _apiDbContext.VideoReactions.FirstOrDefaultAsync(el => el.UserId == loggedInUser.Id && el.VideoId == videoId);
        }

        internal async Task<RegisteredUser> GetUserByPrincipal(ClaimsPrincipal claimsPrincipal)
        {
            var usernameClaim = claimsPrincipal.FindFirst(ClaimTypes.Name);
            var userId = uint.Parse(usernameClaim.Value);

            return await _apiDbContext.Users.FirstOrDefaultAsync(el => el.Id == userId);
        }
    }
}
