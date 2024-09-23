using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlogTest.model
{
    public class Note
    {
        [Key]
        public Guid Guid { get; set; }
        public string Text { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;// Nav prop
        public DateTime PublishingDate { get; set; }
        public int Rating { get; set; } = 0;
        public Guid UserGuid { get; set; } // F. key
        public List<string> WhoLiked { get; set; } = new List<string>();
        public List<string> WhoDisliked { get; set; } = new List<string>();
        [JsonConstructor]
        private Note(Guid guid, string text, Guid userGuid, string userName, DateTime publishingDate)
        {
            Guid = guid;
            Text = text;
            UserGuid = userGuid;
            UserName = userName;
            PublishingDate = publishingDate;
        }

        public static (Note?, string error) Create(Guid guid, string text, Guid userGuid, string userName, DateTime publishingDate)
        {
            string error = string.Empty;

            if(string.IsNullOrEmpty(text))
            {
                error = "Текст записи не может быть пустым";
                return (null, error);
            }
            if(string.IsNullOrEmpty(userName))
            {
                error = "Имя пользователя не может быть пустым";
                return (null, error);
            }

            var note = new Note(guid, text, userGuid, userName, publishingDate);
            return (note, error);
        }

    }
}
