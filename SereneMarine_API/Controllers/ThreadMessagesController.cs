using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.ThreadMessages;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class ThreadMessagesController : ControllerBase
    {
        private IThreadMessagesService _threadMessagesService;
        private IMapper _mapper;

        public ThreadMessagesController(
            IThreadMessagesService threadMessagesService,
            IMapper mapper)
        {
            _threadMessagesService = threadMessagesService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAll()
        {
            var threadMessages = _threadMessagesService.GetAll();
            if (threadMessages == null)
            {
                return StatusCode(500, "Could not make connection to database.");
            }

            var model = _mapper.Map<IList<ThreadMessagesModel>>(threadMessages);
            return Ok(model);
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] ThreadMessagesRegisterModel model)
        {
            // map model to entity
            var tm = _mapper.Map<ThreadMessage>(model);

            try
            {
                ////initalise values and assign to model value
                tm.thread_message_id = Guid.NewGuid().ToString();
                tm.User_Id = User.FindFirstValue(ClaimTypes.Name);
                tm.replied_date = DateTime.Now;
                // create tm
                _threadMessagesService.Create(tm);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets messages by specific thread_id
        /// </summary>
        /// 
        [AllowAnonymous]
        [HttpGet("thread/{thread_id}")]
        public IActionResult GetByThread(string thread_id)
        {
            var tm = _threadMessagesService.GetByThread(thread_id);
            if (tm == null)
            {
                return StatusCode(500, "Could not make connection to database.");
            }
            var model = _mapper.Map<IList<ThreadMessagesModel>>(tm);
            return Ok(model);
        }

        /// <summary>
        /// Gets messages by specific user_id
        /// </summary>
        [HttpGet("user/{user_id}")]
        public IActionResult GetByUser(string user_id)
        {
            var tm = _threadMessagesService.GetByUser(user_id);
            if (tm == null)
            {
                return StatusCode(500, "Could not make connection to database.");
            }
            var model = _mapper.Map<IList<ThreadMessagesModel>>(tm);
            return Ok(model);
        }

        /// <summary>
        /// Updates message by message_id
        /// </summary>
        /// <remarks>
        /// NB* Cannot pass empty values to datetime properties
        /// </remarks>
        [HttpPut("update/{message_id}")]
        public IActionResult Update(string message_id, [FromBody] ThreadMessagesUpdateModel model)
        {
            // map model to entity and set id
            var ev = _mapper.Map<ThreadMessage>(model);
            ev.thread_message_id = message_id;
            ev.replied_date = DateTime.Now;

            try
            {
                // update ev 
                _threadMessagesService.UpdateMessage(ev);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }


        /// <summary>
        /// Deletes messages by thread_id
        /// </summary>
        /// 
        [HttpDelete("delete/thread/{thread_id}")]
        public IActionResult DeleteByThread(string thread_id)
        {
            try
            {
                _threadMessagesService.DeleteByThread(thread_id);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes messages by message_id
        /// </summary>
        /// 
        [HttpDelete("delete/message/{message_id}")]
        public IActionResult DeleteByMessage(string message_id)
        {
            try
            {
                _threadMessagesService.DeleteByMessage(message_id);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Deletes messages by user_id
        /// </summary>
        /// 
        [HttpDelete("delete/user/{user_id}")]
        public IActionResult DeleteByUser(string user_id)
        {
            try
            {
                _threadMessagesService.DeleteByUser(user_id);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes messages by thread_id and user_id
        /// </summary>
        /// 
        [HttpDelete("delete/thread/{thread_id}/user/{user_id}")]
        public IActionResult DeleteByThreadAndUser(string thread_id, string user_id)
        {
            try
            {
                _threadMessagesService.DeleteByThreadAndUser(thread_id, user_id);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}