using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Helpers;
using WebApi.Services;

namespace WebApi.Controllers
{
    public class BaseController : ControllerBase
    {
        protected IPetitionService _petitionService;
        protected IMapper _mapper;
        protected readonly AppSettings _appSettings;

        public BaseController(
           IPetitionService petitionService,
           IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _petitionService = petitionService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }
    }
}
