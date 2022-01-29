using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
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
        EventAttendance Create(EventAttendance eventAttendance);
        void DeleteByEvent(string id);
        void DeleteByUser(string id);
        void DeleteByEventAndUser(string event_id, string user_id);
    }

    public class EventAttendanceService : IEventAttendanceService
    {
        private readonly IMongoCollection<EventAttendance> _eventAttendanceCollection;
        private readonly IMongoCollection<Event> _eventCollection;
        private readonly ICluster _ICluster;

        public EventAttendanceService(IMongoClient client, IUserDatabseSettings settings)
        {
            _ICluster = client.Cluster;
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _eventAttendanceCollection = database.GetCollection<EventAttendance>(settings.EventAttendanceCollectionName);
            _eventCollection = database.GetCollection<Event>(settings.EventsCollectionName);
        }

        public List<EventAttendance> GetAll() 
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            return _eventAttendanceCollection.Find(ea => true).ToList();
        }

        public List<EventAttendance> GetAttendanceByEvent(string id)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            return _eventAttendanceCollection.Find(ea => ea.event_id == id).ToList();
        } 

        public List<EventAttendance> GetAttendanceByUser(string id, bool value)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            return _eventAttendanceCollection.Find(ea => ea.User_Id == id && ea.event_attended == value).ToList();
        }

        public EventAttendance Create(EventAttendance eventAttendance)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            // need to assign userid from bearer token to ea.user_id
            if (string.IsNullOrEmpty(eventAttendance.User_Id))
            {
                throw new AppException("User_id is required");
            }

            // check if event actually exists
            // use linq standard for inner join between two collections
            var query = from x in _eventCollection.AsQueryable()
                        join y in _eventAttendanceCollection.AsQueryable() on x.event_id equals y.event_id
                        into MatchedEvents
                        where (x.event_id == eventAttendance.event_id)
                        select new
                        {
                            event_id = x.event_id
                        };

            //see if query has any results
            if (!query.Any())
            {
                throw new AppException("Event '" + eventAttendance.event_id + "' does not exist");
            }

            bool userHasAlreadyAcceptedEvent = _eventAttendanceCollection.Find(x => x.User_Id == eventAttendance.User_Id && x.event_id == eventAttendance.event_id).FirstOrDefault() != null;
            if (userHasAlreadyAcceptedEvent)
            {
                throw new AppException("User has already participated in event");
            }

            if (eventAttendance.date_accepted == default(DateTime))
            {
                throw new AppException("EventAttendance property 'Start Date' is required");
            }

            _eventAttendanceCollection.InsertOne(eventAttendance);

            return eventAttendance;
        }

        public void DeleteByEvent(string id)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            EventAttendance eventAttendance = _eventAttendanceCollection.Find(ea => ea.event_id == id).FirstOrDefault();
            if (eventAttendance != null)
            {
                _eventAttendanceCollection.DeleteMany(ea => ea.event_id == id);
            }
        }

        public void DeleteByUser(string id)
        {
            EventAttendance evenAttendance = _eventAttendanceCollection.Find(ea => ea.User_Id == id).FirstOrDefault();
            if (evenAttendance != null)
            {
                _eventAttendanceCollection.DeleteMany(ea => ea.User_Id == id);
            }
        }

        public void DeleteByEventAndUser(string event_id, string user_id)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            EventAttendance eventAttendance = _eventAttendanceCollection.Find(ea => ea.event_id == event_id && ea.User_Id == user_id).FirstOrDefault();
            if (eventAttendance != null)
            {
                _eventAttendanceCollection.DeleteOne(ea => ea.event_id == event_id && ea.User_Id == user_id);
            }
        }
    }
}