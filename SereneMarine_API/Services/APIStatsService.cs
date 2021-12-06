using MongoDB.Driver;
using System.Linq;
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

        public APIStatsService(IUserDatabseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            // TODO: Look into client settings
            // Set timeout session
            // If database/client is null, return error?
            //client.Settings.ConnectionMode;
            //client.StartSessionAsync();
            //client.Settings.ConnectTimeout;

            _petitionsSignedCollection = database.GetCollection<PetitionSigned>(settings.PetitionsSignedCollectionName);
            _threadMessagesCollection = database.GetCollection<ThreadMessage>(settings.ThreadMessagesCollectionName);
            _eventAttendanceCollection = database.GetCollection<EventAttendance>(settings.EventAttendanceCollectionName);
        }

        public Statistics GetAllStats()
        {
            Statistics stats = new Statistics();
            try
            {
                stats.PetitionsSigned = _petitionsSignedCollection.Find(x => true).ToList().Count();
                stats.ThreadMessages = _threadMessagesCollection.Find(x => true).ToList().Count();
                stats.EventsAttended = _eventAttendanceCollection.Find(x => true).ToList().Count();
            }
            catch (System.Exception ex)
            {
                throw new AppException(ex.Message);
            }

            return stats;
        }

        public int CountPetitionsSigned() => _petitionsSignedCollection.Find(x => true).ToList().Count();

        public int CountEventsAttended() => _eventAttendanceCollection.Find(x => true).ToList().Count();

        public int CountThreadMessages() => _threadMessagesCollection.Find(x => true).ToList().Count();
    }
}