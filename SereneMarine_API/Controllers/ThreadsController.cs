using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Threads;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class ThreadsController : ControllerBase
    {
        private IThreadsService _threadService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public ThreadsController(
            IThreadsService threadService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _threadService = threadService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Gets all threads
        /// </summary>
        /// 
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAll()
        {
            var events = _threadService.GetAll();
            var model = _mapper.Map<IList<ThreadsModel>>(events);
            return Ok(model);
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] ThreadsRegisterModel model)
        {
            // map model to entity
            var ea = _mapper.Map<Thread>(model);

            try
            {
                ////initalise values and assign to model value
                ea.User_Id = User.FindFirstValue(ClaimTypes.Name);
                ea.thread_id = Guid.NewGuid().ToString();
                ea.created_date = DateTime.Now;
                ea.thread_closed = false;
                // create ea
                _threadService.Create(ea);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets thread by specific thread_id
        /// </summary>
        /// 
        [AllowAnonymous]
        [HttpGet("{thread_id}")]
        public IActionResult GetById(string thread_id)
        {
            var threads = _threadService.GetById(thread_id);
            var model = _mapper.Map<ThreadsModel>(threads);
            return Ok(model);
        }

        /// <summary>
        /// Gets thread by specific user_id
        /// </summary>
        //[AllowAnonymous]
        [HttpGet("user/{User_Id}")]
        public IActionResult GetByUser(string User_Id)
        {
            var threads = _threadService.GetByUser(User_Id);
            var model = _mapper.Map<IList<ThreadsModel>>(threads);
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
        public IActionResult Update(string id, [FromBody] ThreadsUpdateModel model)
        {
            // map model to entity and set id
            var threads = _mapper.Map<Thread>(model);
            threads.thread_id = id;

            try
            {
                // update threads 
                _threadService.Update(threads);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes thread by thread_id
        /// </summary>
        /// 
        //[AllowAnonymous]
        [HttpDelete("delete/thread/{thread_id}")]
        public IActionResult DeleteByThread(string thread_id)
        {
            _threadService.DeleteByThread(thread_id);
            return Ok();
        }
        /// <summary>
        /// Deletes thread by user_id
        /// </summary>
        /// 
        //[AllowAnonymous]
        [HttpDelete("delete/user/{user_id}")]
        public IActionResult DeleteByUser(string user_id)
        {
            _threadService.DeleteByUser(user_id);
            return Ok();
        }

    }
}
