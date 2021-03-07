using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Services
{
    public interface IEventAttendanceService
    {
        List<EventAttendance> GetAll();
        List<EventAttendance> GetAttendanceByEvent(string id);
        List<EventAttendance> GetAttendanceByUser(string id, bool attended);
        EventAttendance Create(EventAttendance ea);
        void DeleteByEvent(string id);
        void DeleteByUser(string id);
        void DeleteByEventAndUser(string event_id, string user_id);
    }
    public class EventAttendanceService : IEventAttendanceService
    {
        private readonly IMongoCollection<EventAttendance> _ea;
        private readonly IMongoCollection<Event> _events;

        public EventAttendanceService(IUserDatabseSettings settings)
        {
            MongoClient client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _ea = database.GetCollection<EventAttendance>(settings.EventAttendanceCollectionName);
            _events = database.GetCollection<Event>(settings.EventsCollectionName);

        }
        public List<EventAttendance> GetAll() =>
       _ea.Find(ea => true).ToList();

        public List<EventAttendance> GetAttendanceByEvent(string id) => _ea.Find(ea => ea.event_id == id).ToList();

        public List<EventAttendance> GetAttendanceByUser(string id, bool value)
        {
            //check bool value returned
            if (value == null)
            {
                value = false;
            }

            return _ea.Find(ea => ea.User_Id == id && ea.event_attended == value).ToList();
        }

        public EventAttendance Create(EventAttendance ea)
        {
            // validation
            //need to assign userid from bearer token to ea.user_id
            if (string.IsNullOrEmpty(ea.User_Id))
                throw new AppException("User_id is required");

            //check if event actually exists
            //use linq standard for inner join between two collecitons
            var query = from x in _events.AsQueryable()
                        join y in _ea.AsQueryable() on x.event_id equals y.event_id
                        into MatchedEvents
                        where (x.event_id == ea.event_id)
                        select new
                        {
                            event_id = x.event_id
                        };

            //see if query has any results
            if (!query.Any())
                throw new AppException($"Event {ea.event_id} does not exist");

            // throw error if the user has already accepted an event abd event exists
            if (_ea.Find(x => x.User_Id == ea.User_Id && x.event_id == ea.event_id).FirstOrDefault() != null)
                throw new AppException("User has already participated in event");

            if (ea.date_accepted == null || string.IsNullOrEmpty(Convert.ToString(ea.date_accepted)) || ea.date_accepted == default(DateTime))
                throw new AppException("EventAttendance Start Date is required");

            _ea.InsertOne(ea);

            return ea;
        }
        public void DeleteByEvent(string id)
        {
            EventAttendance ea = _ea.Find(ea => ea.event_id == id).FirstOrDefault();
            if (ea != null)
            {
                _ea.DeleteMany(ea => ea.event_id == id);
            }
        }
        public void DeleteByUser(string id)
        {
            EventAttendance ea = _ea.Find(ea => ea.User_Id == id).FirstOrDefault();
            if (ea != null)
            {
                _ea.DeleteMany(ea => ea.User_Id == id);
            }
        }

        public void DeleteByEventAndUser(string event_id, string user_id)
        {
            EventAttendance ea = _ea.Find(ea => ea.event_id == event_id && ea.User_Id == user_id).FirstOrDefault();
            if (ea != null)
            {
                _ea.DeleteOne(ea => ea.event_id == event_id && ea.User_Id == user_id);
            }
        }
    }
}
