using PSKVideoProjectBackend.Properties;
using System.ComponentModel.DataAnnotations;

namespace PSKVideoProjectBackend.Models
{
    public class VideoToUpload
    {
        [Required]
        public String VideoName { get; set; }

        [Required]
        public String Description { get; set; }

        [Required]
        public IFormFile? VideoFile { get; set; }

        [Required]
        public IFormFile? ThumbnailImage { get; set; }

        public VideoToUpload()
        {
            VideoName = Resources.FillerVideoName;
            Description = Resources.FillerVideoDescription;
            VideoFile = null;
            ThumbnailImage = null;
        }
    }
}
