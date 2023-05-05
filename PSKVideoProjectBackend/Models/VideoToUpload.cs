using PSKVideoProjectBackend.Properties;
using System.ComponentModel.DataAnnotations;

namespace PSKVideoProjectBackend.Models
{
    public class VideoToUpload
    {

        public String VideoName { get; set; }


        public String Description { get; set; }


        public IFormFile VideoFile { get; set; }


        public IFormFile ThumbnailImage { get; set; }

        public VideoToUpload()
        {
            VideoName = Resources.FillerVideoName;
            Description = Resources.FillerVideoDescription;
            VideoFile = null!;
            ThumbnailImage = null!;
        }
    }
}
