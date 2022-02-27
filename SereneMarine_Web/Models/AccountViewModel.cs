using System;
using System.ComponentModel.DataAnnotations;

namespace SereneMarine_Web.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email_address { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        //[Display(Name = "Remember me?")]
        //public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email_address { get; set; }

        //[Required]
        //[Display(Name = "Username")]
        //[StringLength(50)]
        //public string Username { get; set; }
    }

    public class UserViewModel
    {
        public string User_Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email_address { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
        public int ContactNo { get; set; }
        public string Address { get; set; }
        public DateTime Joined { get; set; }
        public string Token { get; set; }
    }
}
