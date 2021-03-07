using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApi.Entities
{
    public class Petition
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string petition_id { get; set; }
        public string User_Id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int required_signatures { get; set; }
        public int current_signatures { get; set; }
        public bool completed { get; set; }
        public DateTime created_date { get; set; }

    }
}
