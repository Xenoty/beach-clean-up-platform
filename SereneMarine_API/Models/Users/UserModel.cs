using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApi.Models.Users
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
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