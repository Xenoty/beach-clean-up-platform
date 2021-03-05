using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Models.EventAttendance;

namespace WebApi.Helpers
{
    public class SereneMarineMappings : Profile
    {
        public SereneMarineMappings()
        {
            CreateMap<EventAttendance, EventAttendanceRegisterModel>().ReverseMap();
            CreateMap<EventAttendance, EventAttendanceModel>().ReverseMap();
        }
       
    }
}
