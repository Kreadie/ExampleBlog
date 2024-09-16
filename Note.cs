using System.ComponentModel.DataAnnotations;

namespace BlogTest
{
    public class Note
    {
        [Key]
        public Guid Guid { get; set; }
        public string Text { get; set; } = null!;
        public Guid UserGuid { get; set; } // F. key
        public string UserName { get; set; } = null!; // Nav prop
        public DateTime PublishingDate { get; set; }
        public int Rating { get; set; } = 0;
        public List<string> WhoLiked { get; set; } = new List<string>();
        public List<string> WhoDisliked { get; set; } = new List<string>();

    }
}
