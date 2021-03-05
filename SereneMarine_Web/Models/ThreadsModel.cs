using SereneMarine_Web.ViewModels.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SereneMarine_Web.Models
{
    public class ThreadsModel
    {
        public string Id { get; set; }
        public string thread_id { get; set; }
        public string User_Id { get; set; }
        public string Name { get; set; }
        [Display(Name = "Topic")]
        public string thread_topic { get; set; }
        [Display(Name = "Description")]
        public string thread_descr { get; set; }
        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:ddd, MMM, yyyy}")]
        public DateTime created_date { get; set; }
        public bool thread_closed { get; set; }
        [Display(Name = "Author")]
        public string author { get; set; }
        public List<ThreadsModel> ThreadsViewModel { get; set; }
    }
}
