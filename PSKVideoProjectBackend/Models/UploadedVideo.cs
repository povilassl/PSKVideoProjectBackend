using PSKVideoProjectBackend.Properties;
using System;
using System.ComponentModel.DataAnnotations;

namespace PSKVideoProjectBackend.Models
{
    public class UploadedVideo
    {
        [Key]
        public long Id { get; set; }
        public String VideoName { get; set; }
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
            VideoName = Resources.FillerVideoName;
            URL = Resources.FillerVideoURL;
            VideoLengthInSeconds = 0;
            UploadDateTime = DateTime.Now;
            LikeCount = 0;
            DislikeCount = 0;
            ViewCount = 0;
            Username = Resources.FillerVideoUsername;
            Description = Resources.FillerVideoDescription;
        }

        public UploadedVideo(VideoToUpload videoToUpload)
        {
            VideoName = videoToUpload.VideoName;
            Username = videoToUpload.Username;
            Description = videoToUpload.Description;

            //TODO: finish: url + length
            URL = "";
            VideoLengthInSeconds = 0;
            UploadDateTime = DateTime.Now;
            LikeCount = 0;
            DislikeCount = 0;
            ViewCount = 0;
        }
    }
}
