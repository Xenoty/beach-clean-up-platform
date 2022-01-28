namespace WebApi.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }

        public const string DBDisconnectedMessage =  "Database is disconnected";
    }
}