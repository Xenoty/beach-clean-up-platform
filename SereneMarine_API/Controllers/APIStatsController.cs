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
            var model = _mapper.Map<StatisticsModel>(statistics);
            return Ok(model);
        }

        [HttpGet("petitionsSigned")]
        public IActionResult CountPetitionsSigned()
        {
            return Ok(_apiStatsService.CountPetitionsSigned());
        }

        [HttpGet("eventsAttended")]
        public IActionResult CountEventsAttended()
        {
            return Ok(_apiStatsService.CountEventsAttended());
        }

        [HttpGet("threadMessages")]
        public IActionResult CountThreadMessages()
        {
            return Ok(_apiStatsService.CountPetitionsSigned());
        }
    }
}