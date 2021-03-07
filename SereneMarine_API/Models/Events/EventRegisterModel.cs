using System;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Events
{
    public class EventRegisterModel
    {
        [Required]
        public string event_name { get; set; }
        [Required]
        public string event_descr { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public DateTime event_startdate { get; set; }
        [Required]
        public DateTime event_enddate { get; set; }
        [Required]
        public int max_attendance { get; set; }
    }
}
