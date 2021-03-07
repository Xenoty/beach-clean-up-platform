using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Events;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private IEventService _eventService;
        private IMapper _mapper;

        public EventsController(
            IEventService eventService,
            IMapper mapper)
        {
            _eventService = eventService;
            _mapper = mapper;
        }


        //[AllowAnonymous]
        [HttpPost("create")]
        public IActionResult Create([FromBody] EventRegisterModel model)
        {
            // map model to entity
            var ev = _mapper.Map<Event>(model);

            try
            {
                //get the userid by using the ClaimTypes.Name
                ev.User_Id = User.FindFirstValue(ClaimTypes.Name);
                ev.event_createddate = DateTime.Now;
                ev.event_id = Guid.NewGuid().ToString();
                ev.event_completed = false;
                // create ev
                _eventService.Create(ev);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all events
        /// </summary>
        /// 
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAll()
        {
            var events = _eventService.GetAll();
            var model = _mapper.Map<IList<EventsModel>>(events);
            return Ok(model);
        }

        /// <summary>
        /// Gets event by specific id
        /// </summary>
        /// 
        [AllowAnonymous]
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var ev = _eventService.GetById(id);
            var model = _mapper.Map<EventsModel>(ev);
            return Ok(model);
        }

        /// <summary>
        /// Updates event by id
        /// </summary>
        /// <remarks>
        /// NB* Cannot pass empty values to datetime properties
        /// </remarks>
        //[AllowAnonymous]
        [HttpPut("update/{id}")]
        public IActionResult Update(string id, [FromBody] EventUpdateModel model)
        {
            // map model to entity and set id
            var ev = _mapper.Map<Event>(model);
            ev.event_id = id;

            try
            {
                // update ev 
                _eventService.Update(ev);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes event by id
        /// </summary>
        /// 
        //[AllowAnonymous]
        [HttpDelete("delete/{id}")]
        public IActionResult Delete(string id)
        {
            _eventService.Delete(id);
            return Ok();
        }
    }
}
