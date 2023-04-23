using PSKVideoProjectBackend.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PSKVideoProjectBackend.Models
{
    public class VideoReaction
    {
        [Key]
        public uint Id { get; set; }
        public uint VideoId { get; set; }
        public uint UserId { get; set; }
        public VideoReactionEnum Reaction { get; set; }
    }
}
