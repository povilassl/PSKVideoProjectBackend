using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var allVideos = await _apiDbContext.UploadedVideos.ToListAsync();

            if (startIndex + count > allVideos.Count) return null;

            return allVideos.GetRange(startIndex, count).ToList();
        }

        public async Task<UploadedVideo> UploadVideo(UploadedVideo video)
        {
            video.LikeCount = 0;
            video.DislikeCount = 0;
            video.ViewCount = 0;

            var result = await _apiDbContext.UploadedVideos.AddAsync(video);
            await _apiDbContext.SaveChangesAsync();
            return result.Entity;
        }
    }
}
