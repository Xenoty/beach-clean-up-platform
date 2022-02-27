using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApi.Models.PetitionsSigned
{
    public class PetitionsSignedModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string petition_id { get; set; }
        public string User_Id { get; set; }
        public DateTime signed_date { get; set; }
    }
}