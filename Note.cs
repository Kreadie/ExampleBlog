using System.ComponentModel.DataAnnotations;

namespace BlogTest
{
    public class Note
    {
        [Key]
        public Guid Guid { get; set; }
        public string Text { get; set; } = null!;
        public Guid UserGuid { get; set; }
        public string UserName { get; set; } = null!;
        public DateTime PublishingDate { get; set; }

    }
}
