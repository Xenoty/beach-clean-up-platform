using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models.ThreadMessages
{
    public class ThreadMessagesModel
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
