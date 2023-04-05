using PSKVideoProjectBackend.Properties;

namespace PSKVideoProjectBackend.Models
{
    public class VideoToUpload
    {
        public String VideoName { get; set; }
        public String Username { get; set; }
        public String Description { get; set; }
        //public IFormFile? File { get; set; }
        public IFormFile? ThumbnailImage { get; set; }

        public VideoToUpload()
        {
            VideoName = Resources.FillerVideoName;
            Username = Resources.FillerVideoUsername;
            Description = Resources.FillerVideoDescription;
            //File = null;
            ThumbnailImage = null;
        }
    }
}
