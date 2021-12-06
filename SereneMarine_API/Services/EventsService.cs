using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Models.Events;

namespace WebApi.Services
{
    public interface IEventService
    {
        List<Event> GetAll();
        Event GetById(string id);
        Event Create(Event ev);
        void Update(Event ev);
        void Delete(string id);
    }

    public class EventsService : IEventService
    {
        private readonly IMongoCollection<EventAttendance> _eventAttendanceCollection;
        private readonly IMongoCollection<Event> _eventCollection;
        private readonly IConfiguration _configuration;

        public EventsService(IUserDatabseSettings settings, IConfiguration configuration)
        {
            MongoClient client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _eventAttendanceCollection = database.GetCollection<EventAttendance>(settings.EventAttendanceCollectionName);
            _eventCollection = database.GetCollection<Event>(settings.EventsCollectionName);

            _configuration = configuration;
        }

        public List<Event> GetAll() => _eventCollection.Find(ev => true).ToList();

        public Event GetById(string id)
        {
            Event ev = _eventCollection.Find(e => e.event_id == id).FirstOrDefault();
            ev.current_attendance = _eventAttendanceCollection.Find(ea => ea.event_id == id).ToList().Count();

            return ev;
        }

        public Event Create(Event ev)
        {
            // Need to assign userid from bearer token to ev.user_id
            if (string.IsNullOrEmpty(ev.User_Id))
            {
                throw new AppException("'User_id' is required");
            }

            // Check if event name has been taken.
            if (_eventCollection.Find<Event>(x => x.event_name == ev.event_name).FirstOrDefault() != null)
            {
                throw new AppException("Event '" + ev.event_name + "' is already taken");
            }
                
            if (string.IsNullOrEmpty(ev.event_name))
            {
                throw new AppException("Event 'Name' is required");
            }

            if (string.IsNullOrEmpty(ev.event_descr))
            {
                throw new AppException("Event 'Description' is required");
            }

            if (ev.event_startdate == DateTime.MinValue)
            {
                throw new AppException("Event 'Start Date' is required");
            }

            if (ev.event_enddate == DateTime.MinValue)
            {
                throw new AppException("Event 'End Date' is required");
            }

            if (ev.max_attendance == default(int) || ev.max_attendance == 0)
            {
                throw new AppException("Max attendance is required");
            }

            if (string.IsNullOrEmpty(ev.address))
            {
                throw new AppException("Address is required");
            }

            if (ev.latitude == 0 || ev.longitude == 0)
            {
                GetCoordinates gc = new GetCoordinates(_configuration);
                EventCoordinatesModel ecm = gc.GetLongLatMapBox(ev.address).Result;
                ev.latitude = ecm.latitude;
                ev.longitude = ecm.longitude;
            }

            _eventCollection.InsertOne(ev);

            return ev;
        }

        public void Update(Event eventParam)
        {
            Event ev = _eventCollection.Find(ev => ev.event_id == eventParam.event_id).SingleOrDefault();

            if (ev == null)
            {
                throw new AppException("Event not found");
            }

            if (!string.IsNullOrWhiteSpace(eventParam.event_name))
            {
                // throw error if the new event name is already taken
                if (_eventCollection.Find(x => x.event_name == eventParam.event_name).FirstOrDefault() != null)
                {
                    throw new AppException("Event " + eventParam.event_name + " is already taken");
                }

                ev.event_name = eventParam.event_name;
            }

            if (!string.IsNullOrEmpty(eventParam.event_descr))
            {
                ev.event_descr = eventParam.event_descr;
            }

            if (eventParam.longitude != default(double)
                && eventParam.longitude != 0)
            {
                ev.longitude = eventParam.longitude;
            }

            if (eventParam.latitude != default(double)
                && eventParam.latitude != 0)
            {
                ev.latitude = eventParam.latitude;
            }

            if (!string.IsNullOrWhiteSpace(eventParam.address.ToString()))
            {
                ev.address = eventParam.address;

                if (eventParam.latitude == 0 || eventParam.longitude == 0)
                {
                    //get coordinates by address
                    GetCoordinates gc = new GetCoordinates(_configuration);
                    EventCoordinatesModel ecm = gc.GetLongLatMapBox(ev.address).Result;
                    ev.latitude = ecm.latitude;
                    ev.longitude = ecm.longitude;
                }

            }

            if (eventParam.event_startdate != default(DateTime))
            {
                ev.event_startdate = eventParam.event_startdate;
            }

            if (eventParam.event_startdate != default(DateTime))
            {
                ev.event_enddate = eventParam.event_enddate;
            }

            if (eventParam.max_attendance != int.MinValue
                && eventParam.max_attendance != 0)
            {
                ev.max_attendance = eventParam.max_attendance;
            }

            if (eventParam.event_completed != ev.event_completed)
            {
                ev.event_completed = eventParam.event_completed;
            }

            _eventCollection.ReplaceOne(ev => ev.event_id == eventParam.event_id, ev);
        }

        public void Delete(string id)
        {
            Event ev = _eventCollection.Find(ev => ev.event_id == id).FirstOrDefault();
            if (ev != null)
            {
                _eventCollection.DeleteOne(ev => ev.event_id == id);
            }
        }
    }
}