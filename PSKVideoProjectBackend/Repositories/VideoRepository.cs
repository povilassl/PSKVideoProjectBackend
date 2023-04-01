using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PSKVideoProjectBackend.Models;

namespace PSKVideoProjectBackend.Repositories
{
    public class VideoRepository
    {
        private readonly ApiDbContext _apiDbContext;

        public VideoRepository(ApiDbContext apiDbContext)
        {
            _apiDbContext = apiDbContext;
        }

        public async Task<IEnumerable<UploadedVideo>>? GetListOfVideos(int startIndex, int count)
        {
            var allVideos = _apiDbContext.UploadedVideos.ToList();

            int endIndex = startIndex + count - 1;

            // If startIndex is greater than or equal to the length of the originalList, return a new empty list
            if (startIndex >= allVideos.Count) return new List<UploadedVideo>();

            // If endIndex is greater than or equal to the length of the originalList, set it to the last index of the originalList
            if (endIndex >= allVideos.Count) endIndex = allVideos.Count - 1;

            // Create a sublist from the originalList starting at the startIndex and ending at the endIndex
            return allVideos.GetRange(startIndex, endIndex - startIndex + 1);
        }

        public async Task<UploadedVideo> UploadVideo(VideoToUpload video)
        {
            //TODO: cia reikia ikelt i az dar + gaut url ir length
            var uploaded = new UploadedVideo(video);

            var result = await _apiDbContext.UploadedVideos.AddAsync(uploaded);
            await _apiDbContext.SaveChangesAsync();
            return result.Entity;
        }

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
        /// <returns></returns>
        /// TODO: right now a user can like and dislike a video multiple times, this can be solved with register and login
        public async Task<UploadedVideo> LikeAVideo(int videoId, bool like, bool increase)
        {
            var video = await _apiDbContext.UploadedVideos.FirstOrDefaultAsync(el => el.Id == videoId);

            if (video == null) return null!;

            if (like)
            {
                if (increase)
                {
                    if (video.LikeCount != uint.MaxValue) video.LikeCount++;
                }
                else
                {
                    if (video.LikeCount != 0) video.LikeCount--;
                }
            }
            else
            {
                if (increase)
                {
                    if (video.DislikeCount != uint.MaxValue) video.DislikeCount++;
                }
                else
                {
                    if (video.DislikeCount != 0) video.DislikeCount--;
                }
            }

            var result = _apiDbContext.UploadedVideos.Update(video);
            await _apiDbContext.SaveChangesAsync();

            return video;
        }

        public async Task<VideoComment> AddComment(VideoComment comment)
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

            comment.DateTime = DateTime.Now;

            var result = await _apiDbContext.Comments.AddAsync(comment);

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

            //TODO: klausimas, ar cia uztenka sito vieno - ar nereikia dar vieno iskart pridejus komentara?
            await _apiDbContext.SaveChangesAsync();


            return result.Entity;
        }

        public async Task<IEnumerable<VideoComment>> GetComments(int parentId, bool isVideo)
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

            return retList;
        }

        public async Task<UploadedVideo> IncreaseViewCount(int videoId)
        {
            var video = await _apiDbContext.UploadedVideos.FirstOrDefaultAsync(el => el.Id == videoId);

            if (video == null) return null!;

            if (video.ViewCount != uint.MaxValue) video.ViewCount++;

            _apiDbContext.UploadedVideos.Update(video);
            await _apiDbContext.SaveChangesAsync();

            return video;
        }
    }
}
