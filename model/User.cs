using Microsoft.EntityFrameworkCore.Query.Internal;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlogTest.model
{
    public class User
    {
        const int MIN_NAME_LENGTH = 3;
        const int MIN_PASSWORD_LENGHT = 8;

        [Key]
        public Guid Guid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Age { get; set; }
        public float Rep { get; set; } = 0;
        public List<Note> Notes { get; set; } = [];
        public string ImagePath { get; set; } = "wwwroot/image/user/_.png";

        [JsonConstructor]
        private User(Guid guid, string name, string password)
        {
            Guid = guid;
            Name = name;
            Password = password;
        }

        public static (User?, string error) Create(Guid guid, string name, string password)
        {
            string error = string.Empty;
            if(string.IsNullOrEmpty(name) || name.Length < MIN_NAME_LENGTH)
            {
                error = "Имя пользователя не может быть пустым или меньше 3 символов";
                return (null, error);
            }
            if (string.IsNullOrEmpty(password) || password.Length < MIN_PASSWORD_LENGHT)
            {
                error = "Пароль не может быть пустым или меньше 8 символов";
                return (null, error);
            }

            var user = new User(guid, name, password);
            return (user, error);
        }
    }
}
