using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApi.Models.EventAttendance
{
    public class EventAttendanceModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string event_id { get; set; }
        public string User_Id { get; set; }
        public DateTime date_accepted { get; set; }
        public bool event_attended { get; set; }

    }
}