using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApi.Entities
{
    public class ThreadMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string thread_message_id { get; set; }
        public string thread_id { get; set; }
        public string User_Id { get; set; }
        public string Name { get; set; }
        public string thread_message { get; set; }
        public DateTime replied_date { get; set; }
    }
}