using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models.Threads
{
    public class ThreadsUpdateModel
    {
        public string thread_topic { get; set; }
        public string thread_descr { get; set; }
        public bool thread_closed { get; set; }
    }
}
