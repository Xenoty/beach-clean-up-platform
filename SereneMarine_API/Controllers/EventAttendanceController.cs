﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private readonly AppSettings _appSettings;

        public EventAttendanceController(
            IEventAttendanceService attendanceService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _attendanceService = attendanceService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Gets all eventAttendances
        /// </summary>
        /// 
        [HttpGet]
        public IActionResult GetAll()
        {
            var events = _attendanceService.GetAll();
            var model = _mapper.Map<IList<EventAttendanceModel>>(events);
            return Ok(model);
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] EventAttendanceRegisterModel model)
        {
            // map model to entity
            var ea = _mapper.Map<EventAttendance>(model);

            try
            {
                ////initalise values and assign to model value
                ea.date_accepted = DateTime.Now;
                ea.event_attended = false;
                // create ea
                _attendanceService.Create(ea);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets attendance by specific event_id
        /// </summary>
        /// 
        //[AllowAnonymous]
        [HttpGet("event/{event_id}")]
        public IActionResult GetAttendanceByEvent(string event_id)
        {
            var ev = _attendanceService.GetAttendanceByEvent(event_id);
            var model = _mapper.Map<IList<EventAttendanceModel>>(ev);
            return Ok(model);
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
            var ev = _attendanceService.GetAttendanceByUser(User_Id, event_attended);
            var model = _mapper.Map<IList<EventAttendanceModel>>(ev);
            return Ok(model);
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
