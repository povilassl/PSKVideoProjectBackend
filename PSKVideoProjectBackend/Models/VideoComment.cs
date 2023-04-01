using PSKVideoProjectBackend.Properties;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace PSKVideoProjectBackend.Models
{
    public class VideoComment
    {
        [Key]
        public uint Id { get; set; }
        public long VideoId { get; set; }
        public long CommentId { get; set; }
        public string Comment { get; set; }
        public DateTime DateTime { get; set; }
        public string Username { get; set; }
        public bool HasComments { get; set; }

        public VideoComment()
        {
            VideoId = 0;
            CommentId = 0;
            Comment = Resources.FillerComment;
            Username = Resources.FillerVideoUsername;
            DateTime = DateTime.Now;
            HasComments = false;
        }
    }
}
