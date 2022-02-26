using SereneMarine_Web.ViewModels.EventAttendace;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SereneMarine_Web.ViewModels.Events
{
    public class EventsIndexViewModel
    {
        public List<EventsViewModel> EventsViewModel { get; set; }

        public List<EventAttendanceViewModel> EventAttendanceViewModel { get; set; }

        public FilterEvents filterEvents { get; set; }
    }

    public class FilterEvents
    {
        [Display(Name = "Upcomming")]
        public bool GetUpCommingEvents { get; set; }

        [Display(Name = "Past")]
        public bool GetPastEvents { get; set; }

        [Display(Name = "Completed")]
        public bool UserParticipatedEvents { get; set; }

        [Display(Name = "Start")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:MM}")]
        public DateTime? EventStartDate { get; set; }

        [Display(Name = "End")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:MM}")]
        public DateTime? EventEndDate { get; set; }

        [Display(Name = "Max Attedance")]
        [Range(5, 1000, ErrorMessage = "The field Max attendance must be between 5 - 1000.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Max Attendance must be numeric values only")]
        public int? MaxAttendance { get; set; }
    }
}
