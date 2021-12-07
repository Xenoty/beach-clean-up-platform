using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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
            var statistics = _apiStatsService.GetAllStats();
            if (statistics == null)
            {
                return StatusCode(500, "Could not make connection to database.");
            }

            var model = _mapper.Map<StatisticsModel>(statistics);

            return Ok(model);
        }

        [HttpGet("petitionsSigned")]
        public IActionResult CountPetitionsSigned()
        {
            int result = _apiStatsService.CountPetitionsSigned();
            if (result <= -1)
            {
                return StatusCode(500, "Could not make connection to database.");
            }

            return Ok(result);
        }

        [HttpGet("eventsAttended")]
        public IActionResult CountEventsAttended()
        {
            int result = _apiStatsService.CountEventsAttended();
            if (result <= -1)
            {
                return StatusCode(500, "Could not make connection to database.");
            }

            return Ok(result);
        }

        [HttpGet("threadMessages")]
        public IActionResult CountThreadMessages()
        {
            int result = _apiStatsService.CountThreadMessages();
            if (result <= -1)
            {
                return StatusCode(500, "Could not make connection to database.");
            }

            return Ok(result);
        }
    }
}