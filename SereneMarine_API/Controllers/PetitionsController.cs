using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Petitions;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class PetitionsController : ControllerBase
    {
        private IPetitionService _petitionService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public PetitionsController(
           IPetitionService petitionService,
           IMapper mapper,
           IOptions<AppSettings> appSettings)
        {
            _petitionService = petitionService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }


        [HttpPost("create")]
        public IActionResult Create([FromBody] PetitionRegisterModel model)
        {
            // map model to entity
            var pet = _mapper.Map<Petition>(model);

            try
            {
                ////initalise values and assign to model value
                //get the userid by using the ClaimTypes.Name
                pet.User_Id = User.FindFirstValue(ClaimTypes.Name);
                pet.created_date = DateTime.Now;
                pet.petition_id = Guid.NewGuid().ToString();
                pet.completed = false;
                // create pet
                _petitionService.Create(pet);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all petitions
        /// </summary>
        /// 
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAll()
        {
            var petitions = _petitionService.GetAll();
            var model = _mapper.Map<IList<PetitionsModel>>(petitions);
            return Ok(model);
        }

        /// <summary>
        /// Gets petition by specific id
        /// </summary>
        /// 
        [AllowAnonymous]
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var pet = _petitionService.GetById(id);
            var model = _mapper.Map<PetitionsModel>(pet);
            return Ok(model);
        }     
        
        /// <summary>
        /// Gets petition by completion
        /// </summary>
        /// 
        [AllowAnonymous]
        [HttpGet("completion/{val}")]
        public IActionResult GetByCompletion(bool val)
        {
            var pet = _petitionService.GetByCompletion(val);
            var model = _mapper.Map<IList<PetitionsModel>>(pet);
            return Ok(model);
        }  
        
        /// <summary>
        /// Gets petition by user id
        /// </summary>
        /// 
        [AllowAnonymous]
        [HttpGet("user/{id}")]
        public IActionResult GetByUser(string id)
        {
            var pet = _petitionService.GetByUser(id);
            var model = _mapper.Map<PetitionsModel>(pet);
            return Ok(model);
        } 
        

        /// <summary>
        /// Updates petition by id
        /// </summary>
        /// <remarks>
        /// NB* Cannot pass empty values to datetime properties
        /// </remarks>
        [HttpPut("update/{id}")]
        public IActionResult Update(string id, [FromBody] PetitionUpdateModel model)
        {
            // map model to entity and set id
            var pet = _mapper.Map<Petition>(model);
            pet.petition_id = id;

            try
            {
                // update pet 
                _petitionService.Update(pet);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes petition by id
        /// </summary>
        /// 
        [HttpDelete("delete/{id}")]
        public IActionResult Delete(string id)
        {
            _petitionService.Delete(id);
            return Ok();
        }


    }
}
