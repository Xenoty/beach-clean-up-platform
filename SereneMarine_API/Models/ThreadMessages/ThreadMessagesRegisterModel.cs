using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models.ThreadMessages
{
    public class ThreadMessagesRegisterModel
    {
        [Required]
        public string thread_id { get; set; }
        [Required]
        public string thread_message { get; set; }
    }
}
