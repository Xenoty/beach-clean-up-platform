using SereneMarine_Web.ViewModels.EventAttendace;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
        public bool Current { get; set; }
        [Display(Name = "Past")]
        public bool Completed { get; set; }

        [Display(Name = "Completed")]
        public bool UserCompleted { get; set; }

        [Display(Name = "Start")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:MM}")]
        public DateTime? event_startdate { get; set; }
        [Display(Name = "End")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:MM}")]
        public DateTime? event_enddate { get; set; }

        [Display(Name = "Max Attedance")]
        [Range(5, 1000, ErrorMessage = "The field Max attendance must be between 5 - 1000.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Max Attendance must be numeric values only")]
        public int? max_attendance { get; set; }
    }
}
