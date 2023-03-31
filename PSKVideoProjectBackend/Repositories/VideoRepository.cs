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
    }
}
