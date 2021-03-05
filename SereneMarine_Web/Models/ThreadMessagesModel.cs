using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SereneMarine_Web.Models
{
    public class ThreadMessagesModel
    {
        public string Id { get; set; }
        public string thread_message_id { get; set; }
        public string thread_id { get; set; }
        public string User_Id { get; set; }
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Message", Prompt = "Enter your message here...")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Please enter an Event address between 3 and 200 characters in length")]
        public string thread_message { get; set; }
        public DateTime replied_date { get; set; }
       
    }
}
