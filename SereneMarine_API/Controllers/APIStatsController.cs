using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helpers;
using WebApi.Models.ApiStats;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class APIStatsController : ControllerBase
    {
        private IAPIStatsService _apiStatsService;
        private IMapper _mapper;

        public APIStatsController(IAPIStatsService apiStatsService, IMapper mapper)
        {
            _apiStatsService = apiStatsService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAllStats()
        {
            try
            {
                var statistics = _apiStatsService.GetAllStats();
                var model = _mapper.Map<StatisticsModel>(statistics);
                return Ok(model);
            }
            catch (AppException ex)
            {
                return BadRequest( new {message = ex.Message});
            }
        }

        [HttpGet("petitionsSigned")]
        public IActionResult CountPetitionsSigned()
        {
            try
            {
                int result = _apiStatsService.CountPetitionsSigned();
                return Ok(result);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("eventsAttended")]
        public IActionResult CountEventsAttended()
        {
            try
            {
                int result = _apiStatsService.CountEventsAttended();
                return Ok(result);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("threadMessages")]
        public IActionResult CountThreadMessages()
        {
            try
            {
                int result = _apiStatsService.CountThreadMessages();
                return Ok(result);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}