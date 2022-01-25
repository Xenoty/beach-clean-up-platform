﻿using System.Linq;

using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Services
{
    public interface IAPIStatsService
    {
        int CountPetitionsSigned();
        int CountEventsAttended();
        int CountThreadMessages();
       Statistics GetAllStats();
    }

    public class APIStatsService : IAPIStatsService
    {
        private readonly IMongoCollection<PetitionSigned> _petitionsSignedCollection;
        private readonly IMongoCollection<ThreadMessage> _threadMessagesCollection;
        private readonly IMongoCollection<EventAttendance> _eventAttendanceCollection;

        private ICluster _iCluster;

        public APIStatsService(IMongoClient client, IUserDatabseSettings settings)
        {
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);
            _iCluster = client.Cluster;

            _petitionsSignedCollection = database.GetCollection<PetitionSigned>(settings.PetitionsSignedCollectionName);
            _threadMessagesCollection = database.GetCollection<ThreadMessage>(settings.ThreadMessagesCollectionName);
            _eventAttendanceCollection = database.GetCollection<EventAttendance>(settings.EventAttendanceCollectionName);
        }

        public Statistics GetAllStats()
        {
            if (!_iCluster.Description.State.IsConnected())
            {
                return null;
            }

            Statistics stats = new Statistics()
            {
                PetitionsSigned = _petitionsSignedCollection.Find(x => true).ToList().Count(),
                ThreadMessages = _threadMessagesCollection.Find(x => true).ToList().Count(),
                EventsAttended = _eventAttendanceCollection.Find(x => true).ToList().Count()
            };

            return stats;
        }

        public int CountPetitionsSigned() 
        {
            if (!_iCluster.Description.State.IsConnected())
            {
                return -1;
            }

            return _petitionsSignedCollection.Find(x => true).ToList().Count();
        }

        public int CountEventsAttended()
        {
            if (!_iCluster.Description.State.IsConnected())
            {
                return -1;
            }

            return _eventAttendanceCollection.Find(x => true).ToList().Count();
        }

        public int CountThreadMessages() 
        {
            if (!_iCluster.Description.State.IsConnected())
            {
                return -1;
            }

            return _threadMessagesCollection.Find(x => true).ToList().Count();
        }
    }
}