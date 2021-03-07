using MongoDB.Driver;
using System.Linq;
using WebApi.Entities;
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
        private readonly IMongoCollection<PetitionSigned> _ps;
        private readonly IMongoCollection<ThreadMessage> _tm;
        private readonly IMongoCollection<EventAttendance> _ea;

        public APIStatsService(IUserDatabseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _ps = database.GetCollection<PetitionSigned>(settings.PetitionsSignedCollectionName);
            _tm = database.GetCollection<ThreadMessage>(settings.ThreadMessagesCollectionName);
            _ea = database.GetCollection<EventAttendance>(settings.EventAttendanceCollectionName);
        }

        public Statistics GetAllStats()
        {
            Statistics stats = new Statistics();
            stats.PetitionsSigned = _ps.Find(x => true).ToList().Count();
            stats.ThreadMessages = _tm.Find(x => true).ToList().Count();
            stats.EventsAttended = _ea.Find(x => true).ToList().Count();
            return stats;
        }

        public int CountPetitionsSigned() => _ps.Find(x => true).ToList().Count();

        public int CountEventsAttended() => _ea.Find(x => true).ToList().Count();

        public int CountThreadMessages() => _tm.Find(x => true).ToList().Count();
    }
}
