using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models.ApiStats
{
    public class StatisticsModel
    {
        public int EventsAttended { get; set; }
        public int PetitionsSigned { get; set; }
        public int ThreadMessages { get; set; }
    }
}
