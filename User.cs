using System.ComponentModel.DataAnnotations;

namespace BlogTest
{
    public class User
    {
        [Key]
        public Guid Guid { get; set; }
        public string Name { get; set; } = "";
        public string Password { get; set; } = null!;
        public int Age { get; set; }
        public float Rep { get; set; } = 0;
        public List<Note> Notes { get; set; } = new List<Note>();
        public string ImagePath { get; set; } = "wwwroot/image/user/DefaultUserImg.png";

    }
}
