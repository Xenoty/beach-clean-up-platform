using Microsoft.AspNetCore.Mvc;
using SereneMarine_Web.Helpers;
using SereneMarine_Web.Models;
using System;
using System.Diagnostics;

namespace SereneMarine_Web.Controllers
{
    public class HomeController : BaseController
    {
        #region Views

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        #endregion

        #region Private Variables

        private ICacheProvider _cacheProvider;
        private ApiStatisticsModel previousStaticsModel = new ApiStatisticsModel();

        #endregion

        #region Constructor

        public HomeController(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        #endregion

        #region Task Methods

        public IActionResult Index()
        {
            try
            {
                ApiStatisticsModel model = _cacheProvider.GetCachedResponse().Result;
                if (previousStaticsModel != model)
                {
                    previousStaticsModel = model;
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                ApiException exception = new ApiException()
                {
                    StatusCode = 500,
                    Content = "{ \n error : " + ex.Message + "}"
                };

                TempData["ApiError"] = exception.GetApiErrorMessage();

                return View(previousStaticsModel);
            }
        }

        #endregion
    }
}