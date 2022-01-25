using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SereneMarine_Web.Helpers;
using SereneMarine_Web.Models;
using System.Diagnostics;
using System.Threading.Tasks;

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

        private ApiStatisticsModel previousStaticsModel = new ApiStatisticsModel();

        #endregion

        #region Task Methods

        public async Task<IActionResult> Index()
        {
            //get all the totals from the api
            string url = SD.ApiStatsPath;

            response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                // TODO: log error message on web. Look at Library Dewey App for example.
                //var content = response.StatusCode;
                //var testing = response.Content.ReadAsStringAsync();

                ApiException exception = new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };

                TempData["ApiError"] = JsonConvert.SerializeObject(exception);

                return View(previousStaticsModel);
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            ApiStatisticsModel model = JsonConvert.DeserializeObject<ApiStatisticsModel>(jsonString);
            previousStaticsModel = model;

            return View(model);
        }

        #endregion
    }
}