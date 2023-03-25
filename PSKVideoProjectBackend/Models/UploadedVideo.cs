using PSKVideoProjectBackend.Properties;
using System.ComponentModel.DataAnnotations;

namespace PSKVideoProjectBackend.Models
{
    public class UploadedVideo
    {
        [Key]
        public long Id { get; set; }
        public String Name { get; set; }
        public String URL { get; set; }
        public uint VideoLengthInSeconds { get; set; }
        public DateTime UploadDateTime { get; set; }
        public uint LikeCount { get; set; }
        public uint DislikeCount { get; set; }
        public uint ViewCount { get; set; }
        public String Username { get; set; }
        public String Description { get; set; }

        public UploadedVideo()
        {
            Name = Resources.FillerVideoName;
            URL = Resources.FillerVideoURL;
            VideoLengthInSeconds = 0;
            UploadDateTime = DateTime.Now;
            LikeCount = 0;
            DislikeCount = 0;
            ViewCount = 0;
            Username = Resources.FillerVideoUsername;
            Description = Resources.FillerVideoDescription;
        }
    }
}
