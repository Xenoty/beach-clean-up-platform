using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SereneMarine_Web.ViewModels.Threads
{
    public class ThreadDeleteViewModel
    {
        public string thread_id { get; set; }
        public string User_Id { get; set; }

        [Display(Name = "Topic")]
        public string thread_topic { get; set; }
        [Display(Name = "Description")]
        public string thread_descr { get; set; }
        [Display(Name = "Close Thread")]
        public bool thread_closed { get; set; }
        [Display(Name = "Author")]
        public string author { get; set; }
    }
}
