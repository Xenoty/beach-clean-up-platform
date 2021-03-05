using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SereneMarine_Web.ViewModels.EventAttendace
{
    public class EventAttendanceViewModel
    {
        public string event_id { get; set; }
        public string user_id { get; set; }
        public DateTime date_accepted { get; set; }
        public bool event_attended { get; set; }
    }
}
