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
        private readonly IMongoCollection<EventAttendance> _eventAttendanceCollection;
        private readonly IMongoCollection<Event> _eventCollection;

        public EventAttendanceService(IUserDatabseSettings settings)
        {
            MongoClient client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _eventAttendanceCollection = database.GetCollection<EventAttendance>(settings.EventAttendanceCollectionName);
            _eventCollection = database.GetCollection<Event>(settings.EventsCollectionName);

        }
        public List<EventAttendance> GetAll() 
        {
            return _eventAttendanceCollection.Find(ea => true).ToList();
        }

        public List<EventAttendance> GetAttendanceByEvent(string id)
        {
            return _eventAttendanceCollection.Find(ea => ea.event_id == id).ToList();
        } 

        public List<EventAttendance> GetAttendanceByUser(string id, bool value)
        {
            return _eventAttendanceCollection.Find(ea => ea.User_Id == id && ea.event_attended == value).ToList();
        }

        public EventAttendance Create(EventAttendance ea)
        {
            // need to assign userid from bearer token to ea.user_id
            if (string.IsNullOrEmpty(ea.User_Id))
            {
                throw new AppException("User_id is required");
            }

            // check if event actually exists
            // use linq standard for inner join between two collections
            var query = from x in _eventCollection.AsQueryable()
                        join y in _eventAttendanceCollection.AsQueryable() on x.event_id equals y.event_id
                        into MatchedEvents
                        where (x.event_id == ea.event_id)
                        select new
                        {
                            event_id = x.event_id
                        };

            //see if query has any results
            if (!query.Any())
            {
                throw new AppException($"Event {ea.event_id} does not exist");
            }

            bool userHasAlreadyAcceptedEvent = _eventAttendanceCollection.Find(x => x.User_Id == ea.User_Id && x.event_id == ea.event_id).FirstOrDefault() != null;
            if (userHasAlreadyAcceptedEvent)
            {
                throw new AppException("User has already participated in event");
            }

            if (ea.date_accepted == default(DateTime))
            {
                throw new AppException("EventAttendance Start Date is required");
            }

            _eventAttendanceCollection.InsertOne(ea);

            return ea;
        }

        public void DeleteByEvent(string id)
        {
            EventAttendance ea = _eventAttendanceCollection.Find(ea => ea.event_id == id).FirstOrDefault();
            if (ea != null)
            {
                _eventAttendanceCollection.DeleteMany(ea => ea.event_id == id);
            }
        }

        public void DeleteByUser(string id)
        {
            EventAttendance ea = _eventAttendanceCollection.Find(ea => ea.User_Id == id).FirstOrDefault();
            if (ea != null)
            {
                _eventAttendanceCollection.DeleteMany(ea => ea.User_Id == id);
            }
        }

        public void DeleteByEventAndUser(string event_id, string user_id)
        {
            EventAttendance ea = _eventAttendanceCollection.Find(ea => ea.event_id == event_id && ea.User_Id == user_id).FirstOrDefault();
            if (ea != null)
            {
                _eventAttendanceCollection.DeleteOne(ea => ea.event_id == event_id && ea.User_Id == user_id);
            }
        }
    }
}