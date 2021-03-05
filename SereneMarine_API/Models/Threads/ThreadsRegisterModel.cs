using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models.Threads
{
    public class ThreadsRegisterModel
    {
        [Required]
        public string thread_topic { get; set; }
        [Required]
        public string thread_descr { get; set; }
    }
}
