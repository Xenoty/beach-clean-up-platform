using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models.Petitions
{
    public class PetitionUpdateModel
    {
        public string name { get; set; }
        public string description { get; set; }
        public int required_signatures { get; set; }
        public bool completed { get; set; }
    }
}
