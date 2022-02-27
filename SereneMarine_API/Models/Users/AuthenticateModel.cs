using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Users
{
    public class AuthenticateModel
    {
        [Required]
        public string Email_address { get; set; }

        [Required]
        public string Password { get; set; }
    }
}