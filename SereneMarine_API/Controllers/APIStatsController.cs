using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Services;
using System;
using System.Collections.Generic;
using WebApi.Models.ApiStats;

namespace WebApi.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class APIStatsController : ControllerBase
    {
        private IAPIStatsService _apiStatsService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public APIStatsController(
            IAPIStatsService apiStatsService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _apiStatsService = apiStatsService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
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
