using System.ComponentModel.DataAnnotations;

namespace Fracto.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string Username { get; set; }

         public string PasswordHash { get; set; }

        public string Role { get; set; } = "User";

        //  Profile Image Path
        public string? ProfileImagePath { get; set; }
    }
}
