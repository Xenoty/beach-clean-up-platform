using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.EventAttendance
{
    public class EventAttendanceRegisterModel
    {
        [Required]
        public string event_id { get; set; }
        [Required]
        public string User_Id { get; set; }
    }
}