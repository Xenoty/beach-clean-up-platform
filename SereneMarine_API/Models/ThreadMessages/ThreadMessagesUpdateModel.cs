using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models.ThreadMessages
{
    public class ThreadMessagesUpdateModel
    {
        public string thread_message { get; set; }
        public DateTime replied_date { get; set; }
    }
}
