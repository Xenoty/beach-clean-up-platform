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
        private readonly IMongoCollection<EventAttendance> _ea;
        private readonly IMongoCollection<Event> _events;
        private readonly IConfiguration _configuration;

        public EventsService(IUserDatabseSettings settings, IConfiguration configuration)
        {
            MongoClient client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _ea = database.GetCollection<EventAttendance>(settings.EventAttendanceCollectionName);
            _events = database.GetCollection<Event>(settings.EventsCollectionName);

            _configuration = configuration;
        }

        public List<Event> GetAll() => _events.Find(ev => true).ToList();

        public Event GetById(string id)
        {
            //also need to get current attendance for event
            Event temp = _events.Find(ev => ev.event_id == id).FirstOrDefault();

            temp.current_attendance = _ea.Find(ea => ea.event_id == id).ToList().Count();

            return temp;
        }

        public Event Create(Event ev)
        {
            // validation
            //need to assign userid from bearer token to ev.user_id
            if (string.IsNullOrEmpty(ev.User_Id))
                throw new AppException("User_id is required");
            //check if event name has been taken
            // throw error if the new username is already taken
            if (_events.Find<Event>(x => x.event_name == ev.event_name).FirstOrDefault() != null)
                throw new AppException("Event " + ev.event_name + " is already taken");

            if (string.IsNullOrEmpty(ev.event_name))
                throw new AppException("Event Name is required");

            if (string.IsNullOrEmpty(ev.event_descr))
                throw new AppException("Event Description is required");

            if (ev.event_startdate == null || string.IsNullOrEmpty(Convert.ToString(ev.event_startdate)))
                throw new AppException("Event Start Date is required");

            if (ev.event_enddate == null || string.IsNullOrEmpty(Convert.ToString(ev.event_enddate)))
                throw new AppException("Event End Date is required");

            if (string.IsNullOrEmpty(ev.max_attendance.ToString()) || ev.max_attendance == 0)
                throw new AppException("Max attendance is required");

            if (string.IsNullOrEmpty(ev.address))
                throw new AppException("Address is required");

            if (ev.latitude == 0 || ev.longitude == 0)
            {
                GetCoordinates gc = new GetCoordinates(_configuration);
                //get coordinates by address
                EventCoordinatesModel ecm = gc.GetLongLatMapBox(ev.address).Result;
                ev.latitude = ecm.latitude;
                ev.longitude = ecm.longitude;
            }

            _events.InsertOne(ev);

            return ev;
        }

        public void Update(Event eventParam)
        {
            Event ev = _events.Find(ev => ev.event_id == eventParam.event_id).SingleOrDefault();

            if (ev == null)
                throw new AppException("Event not found");

            // update event name if it has changed
            if (!string.IsNullOrWhiteSpace(eventParam.event_name) && eventParam.event_name != ev.event_name)
            {
                // throw error if the new event name is already taken
                if (_events.Find(x => x.event_name == eventParam.event_name).FirstOrDefault() != null)
                    throw new AppException("Event " + eventParam.event_name + " is already taken");

                //assign event name to model
                ev.event_name = eventParam.event_name;
            }

            // update ev properties if provided
            if (!string.IsNullOrEmpty(eventParam.event_descr))
                ev.event_descr = eventParam.event_descr;

            if (!string.IsNullOrWhiteSpace(eventParam.longitude.ToString()) && eventParam.longitude != 0)
                ev.longitude = eventParam.longitude;

            if (!string.IsNullOrWhiteSpace(eventParam.latitude.ToString()) && eventParam.latitude != 0)
                ev.latitude = eventParam.latitude;

            if (eventParam.address != null && !string.IsNullOrWhiteSpace(eventParam.address.ToString()))
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

            if (!string.IsNullOrWhiteSpace(eventParam.event_startdate.ToString()) && eventParam.event_startdate != default(DateTime))
                ev.event_startdate = eventParam.event_startdate;

            if (!string.IsNullOrWhiteSpace(eventParam.event_enddate.ToString()) && eventParam.event_startdate != default(DateTime))
                ev.event_enddate = eventParam.event_enddate;

            if (!string.IsNullOrWhiteSpace(eventParam.max_attendance.ToString()) && eventParam.max_attendance != 0)
                ev.max_attendance = eventParam.max_attendance;

            if (eventParam.event_completed != ev.event_completed)
                ev.event_completed = eventParam.event_completed;

            _events.ReplaceOne(ev => ev.event_id == eventParam.event_id, ev);
        }

        public void Delete(string id)
        {
            Event ev = _events.Find(ev => ev.event_id == id).FirstOrDefault();
            if (ev != null)
            {
                _events.DeleteOne(ev => ev.event_id == id);
            }
        }
    }
}
