using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{

    public class UserDatabaseSettings : IUserDatabseSettings
    {
        public string UsersCollectionName { get; set; }
        public string EventsCollectionName { get; set; }
        public string EventAttendanceCollectionName { get; set; }
        public string PetitionsCollectionName { get; set; }
        public string PetitionsSignedCollectionName { get; set; }
        public string ThreadsCollectionName { get; set; }
        public string ThreadMessagesCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IUserDatabseSettings
    {
        string UsersCollectionName { get; set; }
        string EventsCollectionName { get; set; }
        string EventAttendanceCollectionName { get; set; }
        string PetitionsCollectionName { get; set; }
        public string PetitionsSignedCollectionName { get; set; }
        public string ThreadsCollectionName { get; set; }
        public string ThreadMessagesCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
    
}
