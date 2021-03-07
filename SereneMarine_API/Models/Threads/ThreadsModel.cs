using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApi.Models.Threads
{
    public class ThreadsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string thread_id { get; set; }
        public string User_Id { get; set; }
        public string thread_topic { get; set; }
        public string thread_descr { get; set; }
        public DateTime created_date { get; set; }
        public bool thread_closed { get; set; }
        public string author { get; set; }
    }
}
