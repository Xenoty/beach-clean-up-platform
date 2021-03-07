using System;

namespace WebApi.Models.ThreadMessages
{
    public class ThreadMessagesUpdateModel
    {
        public string thread_message { get; set; }
        public DateTime replied_date { get; set; }
    }
}
