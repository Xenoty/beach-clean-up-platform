using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.EventAttendance;
using WebApi.Services;

namespace WebApi.Controllers
{
    //[Authorize]
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class EventAttendanceController : ControllerBase
    {
        private IEventAttendanceService _attendanceService;
        private IMapper _mapper;

        public EventAttendanceController(IEventAttendanceService attendanceService, IMapper mapper)
        {
            _attendanceService = attendanceService;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets all Events Attended
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            List<EventAttendance> eventAttendanceList = _attendanceService.GetAll();
            IList<EventAttendanceModel> eventAttendanceModelIList = _mapper.Map<IList<EventAttendanceModel>>(eventAttendanceList);

            return Ok(eventAttendanceModelIList);
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] EventAttendanceRegisterModel model)
        {
            // map model to entity
            EventAttendance eventAttendance = _mapper.Map<EventAttendance>(model);

            try
            {
                // Initialize values and assign to model value
                eventAttendance.date_accepted = DateTime.Now;
                eventAttendance.event_attended = false;
                // create ea
                _attendanceService.Create(eventAttendance);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest( new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets attendance by specific event_id
        /// </summary>
        //[AllowAnonymous]
        [HttpGet("event/{event_id}")]
        public IActionResult GetAttendanceByEvent(string event_id)
        {
            List<EventAttendance> eventAttendanceList = _attendanceService.GetAttendanceByEvent(event_id);
            IList<EventAttendanceModel> eventAttendanceModelIList = _mapper.Map<IList<EventAttendanceModel>>(eventAttendanceList);

            return Ok(eventAttendanceModelIList);
        }

        /// <summary>
        /// Gets attendance by specific user_id and if attended or not
        /// </summary>
        /// <remarks>
        /// make attended false for all events not attended
        /// </remarks>
        //[AllowAnonymous]
        [HttpGet("user/{User_Id}&{event_attended}")]
        public IActionResult GetAttendanceByUser(string User_Id, bool event_attended)
        {
            List<EventAttendance> eventAttendanceList = _attendanceService.GetAttendanceByUser(User_Id, event_attended);
            IList<EventAttendanceModel> eventAttendanceModelIList = _mapper.Map<IList<EventAttendanceModel>>(eventAttendanceList);

            return Ok(eventAttendanceModelIList);
        }

        /// <summary>
        /// Deletes attendance by event_id
        /// </summary>
        /// 
        //[AllowAnonymous]
        [HttpDelete("delete/event/{event_id}")]
        public IActionResult Delete(string event_id)
        {
            _attendanceService.DeleteByEvent(event_id);
            return Ok();
        }
        /// <summary>
        /// Deletes attendance by user_id
        /// </summary>
        /// 
        //[AllowAnonymous]
        [HttpDelete("delete/user/{user_id}")]
        public IActionResult DeleteByUser(string user_id)
        {
            _attendanceService.DeleteByUser(user_id);
            return Ok();
        }

        /// <summary>
        /// Deletes attendance by event_id and user_id
        /// </summary>
        /// 
        //[AllowAnonymous]
        [HttpDelete("delete/event/{event_id}/user/{user_id}")]
        public IActionResult DeleteByEventAndUser(string event_id, string user_id)
        {
            _attendanceService.DeleteByEventAndUser(event_id, user_id);
            return Ok();
        }
    }
}