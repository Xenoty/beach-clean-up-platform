using System.Linq;

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

        private readonly IMongoClient _client;

        private ClusterState _clusterState;

        public APIStatsService(IMongoClient client, IUserDatabseSettings settings)
        {
            _client = client;
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _petitionsSignedCollection = database.GetCollection<PetitionSigned>(settings.PetitionsSignedCollectionName);
            _threadMessagesCollection = database.GetCollection<ThreadMessage>(settings.ThreadMessagesCollectionName);
            _eventAttendanceCollection = database.GetCollection<EventAttendance>(settings.EventAttendanceCollectionName);

            _clusterState = _client.Cluster.Description.State;
        }

        public Statistics GetAllStats()
        {
            if (!_clusterState.IsConnected())
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
            if (!_clusterState.IsConnected())
            {
                return -1;
            }

            return _petitionsSignedCollection.Find(x => true).ToList().Count();
        }

        public int CountEventsAttended()
        {
            if (!_clusterState.IsConnected())
            {
                return -1;
            }

            return _eventAttendanceCollection.Find(x => true).ToList().Count();
        }

        public int CountThreadMessages() 
        {
            if (!_clusterState.IsConnected())
            {
                return -1;
            }

            return _threadMessagesCollection.Find(x => true).ToList().Count();
        }
    }
}