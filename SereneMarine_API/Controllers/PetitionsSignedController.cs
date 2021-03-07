using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.PetitionsSigned;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class PetitionsSignedController : ControllerBase
    {
        private IPetitionsSignedService _petitionSignedService;
        private IMapper _mapper;

        public PetitionsSignedController(
            IPetitionsSignedService petitionsSignedService,
            IMapper mapper)
        {
            _petitionSignedService = petitionsSignedService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var petitionsSigned = _petitionSignedService.GetAll();
            var model = _mapper.Map<IList<PetitionsSignedModel>>(petitionsSigned);
            return Ok(model);
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] PetitionsSignedRegisterModel model)
        {
            // map model to entity
            var ps = _mapper.Map<PetitionSigned>(model);

            try
            {
                ////initalise values and assign to model value
                ps.signed_date = DateTime.Now;
                // create ps
                _petitionSignedService.Create(ps);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets petitions signed by specific petition_id
        /// </summary>
        /// 
        //[AllowAnonymous]
        [HttpGet("signature/{petition_id}")]
        public IActionResult GetByPetition(string petition_id)
        {
            var ps = _petitionSignedService.GetByPetition(petition_id);
            var model = _mapper.Map<IList<PetitionsSignedModel>>(ps);
            return Ok(model);
        }

        /// <summary>
        /// Gets petitions signed by specific user_id
        /// </summary>
        //[AllowAnonymous]
        [HttpGet("user/{user_id}")]
        public IActionResult GetByUser(string user_id)
        {
            var ps = _petitionSignedService.GetByUser(user_id);
            var model = _mapper.Map<IList<PetitionsSignedModel>>(ps);
            return Ok(model);
        }

        /// <summary>
        /// Deletes petitions signed by petition_id
        /// </summary>
        /// 
        //[AllowAnonymous]
        [HttpDelete("delete/signature/{petition_id}")]
        public IActionResult DeleteByPetition(string petition_id)
        {
            _petitionSignedService.DeleteByPetition(petition_id);
            return Ok();
        }

        /// <summary>
        /// Deletes petitions signed by user_id
        /// </summary>
        /// 
        //[AllowAnonymous]
        [HttpDelete("delete/user/{user_id}")]
        public IActionResult DeleteByUser(string user_id)
        {
            _petitionSignedService.DeleteByUser(user_id);
            return Ok();
        }

        /// <summary>
        /// Deletes petitions signed by petition_id and user_id
        /// </summary>
        /// 
        //[AllowAnonymous]
        [HttpDelete("delete/signature/{petition_id}/user/{user_id}")]
        public IActionResult DeleteByPetitionAndUser(string petition_id, string user_id)
        {
            _petitionSignedService.DeleteByPetitionAndUser(petition_id, user_id);
            return Ok();
        }
    }
}
