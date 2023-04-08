using PSKVideoProjectBackend.Properties;
using System;
using System.ComponentModel.DataAnnotations;

namespace PSKVideoProjectBackend.Models
{
    public class UploadedVideo
    {
        [Key]
        public uint Id { get; set; }
        public String VideoName { get; set; }
        public String VideoURL { get; set; }
        public String ThumbnailURL { get; set; }
        public uint VideoDurationInSeconds { get; set; }
        public DateTime UploadDateTime { get; set; }
        public uint LikeCount { get; set; }
        public uint DislikeCount { get; set; }
        public uint ViewCount { get; set; }
        public String Username { get; set; }
        public String Description { get; set; }
        public bool HasComments { get; set; }

        public UploadedVideo()
        {
            VideoName = Resources.FillerVideoName;
            VideoURL = Resources.FillerURL;
            ThumbnailURL = Resources.FillerURL;
            VideoDurationInSeconds = 0;
            UploadDateTime = DateTime.Now;
            LikeCount = 0;
            DislikeCount = 0;
            ViewCount = 0;
            Username = Resources.FillerVideoUsername;
            Description = Resources.FillerVideoDescription;
            HasComments = false;
        }

        public UploadedVideo(VideoToUpload videoToUpload)
        {
            VideoName = videoToUpload.VideoName;
            Username = videoToUpload.Username;
            Description = videoToUpload.Description;

            Id = 0;
            VideoURL = "";
            ThumbnailURL = "";
            UploadDateTime = DateTime.Now;
            LikeCount = 0;
            DislikeCount = 0;
            ViewCount = 0;
            HasComments = false;
        }
    }
}
