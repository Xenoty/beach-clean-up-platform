using Microsoft.Extensions.Configuration;

namespace SereneMarine_Web.Helpers
{
    public static class SD
    {
        public static string apiBaseURL = Startup.staticConfiguration.GetValue<string>("APIConnection:URL");
        public static string UserPath = apiBaseURL + "users/";
        public static string EventsPath = apiBaseURL + "Events/";
        public static string EventAttendancePath = apiBaseURL + "EventAttendance/";
        public static string PetitionsPath = apiBaseURL + "Petitions/";
        public static string PetitionsSignedPath = apiBaseURL + "PetitionsSigned/";
        public static string ThreadsPath = apiBaseURL + "Threads/";
        public static string ThreadsMessagesPath = apiBaseURL + "ThreadMessages/";
        public static string ApiStatsPath = apiBaseURL + "APIStats/";
    }
}
