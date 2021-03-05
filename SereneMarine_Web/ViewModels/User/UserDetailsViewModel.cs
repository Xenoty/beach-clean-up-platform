using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SereneMarine_Web.ViewModels.User
{
    public class UserDetailsViewModel
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
    }
}
