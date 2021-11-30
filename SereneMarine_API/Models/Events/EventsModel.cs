﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApi.Models.Events
{
    public class EventsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string event_id { get; set; }
        public string event_name { get; set; }
        public string event_descr { get; set; }
        public string User_Id { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string address { get; set; }
        public DateTime event_startdate { get; set; }
        public DateTime event_enddate { get; set; }
        public DateTime event_createddate { get; set; }
        public int max_attendance { get; set; }
        public int current_attendance { get; set; }
        public Boolean event_completed { get; set; }
    }
}