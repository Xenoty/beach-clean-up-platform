namespace WebApi.Models.Threads
{
    public class ThreadsUpdateModel
    {
        public string thread_topic { get; set; }
        public string thread_descr { get; set; }
        public bool thread_closed { get; set; }
        public string author { get; set; }
    }
}