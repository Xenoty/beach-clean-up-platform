namespace WebApi.Models.Users
{
  public class UpdateModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public int ContactNo { get; set; }
        public string Address { get; set; }
        public string Email_address { get; set; }
    }
}