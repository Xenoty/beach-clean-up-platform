using Microsoft.AspNetCore.Mvc;
using SereneMarine_Web.Helpers;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SereneMarine_Web.ViewModels.Events
{
    public class EventsViewModel
    {
        public string event_id { get; set; }
        public string User_Id { get; set; }

        [Display(Name = "Event Name")]
        public string event_name { get; set; }

        [Display(Name = "Description")]
        public string event_descr { get; set; }

        public double longitude { get; set; }

        public double latitude { get; set; }

        [Display(Name = "Location")]
        public string address { get; set; }

        [Display(Name = " Starting Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:MM}")]
        public DateTime event_startdate { get; set; }

        [Display(Name = " Ending Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:MM}")]
        public DateTime event_enddate { get; set; }
        public DateTime event_createddate { get; set; }
        public int max_attendance { get; set; }
        public int current_attendance { get; set; }
        public bool event_completed { get; set; }

        [DefaultValue(false)]
        public bool matching_user { get; set; }
    }
}
