using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
