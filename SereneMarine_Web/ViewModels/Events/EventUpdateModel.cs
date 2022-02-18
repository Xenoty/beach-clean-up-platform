using System;
using System.ComponentModel.DataAnnotations;

namespace SereneMarine_Web.ViewModels.Events
{
    public class EventUpdateModel
    {
        public string event_id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Please enter an Event name between 3 and 50 characters in length")]
        [RegularExpression(@"^[@.,;\'""a-zA-Z0-9'-'\s]*$", ErrorMessage = "Please enter a event name made up of letters and numbers only")]
        public string event_name { get; set; }

        [Required]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Please enter a event description between 10 and 200 characters in length")]
        [RegularExpression(@"^[@.,;\'""a-zA-Z0-9'-'\s]*$", ErrorMessage = "Please enter a event description made up of letters and numbers only")]
        public string event_descr { get; set; }
    
        [Required]
        [Display(Name = "Longitude")]
        public double longitude { get; set; }
        [Required]
        [Display(Name = "Latitude")]
        public double latitude { get; set; }

        [Required]
        [Display(Name = "Address")]
        [DataType(DataType.MultilineText)]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Please enter an Event address between 3 and 100 characters in length")]
        public string address { get; set; }

        [Required]
        [Display(Name = "Starting Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:MM}")]
        public DateTime event_startdate { get; set; }

        [Required]
        [Display(Name = "Ending Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:MM}")]
        public DateTime event_enddate { get; set; }

        [Required]
        [Display(Name = "Max Attedance for Event")]
        [Range(5, 1000, ErrorMessage = "The field Max attendance must be between 5 - 1000.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Max Attendance must be numeric values only")]
        public int max_attendance { get; set; }
        public Boolean event_completed { get; set; }
    }
}
