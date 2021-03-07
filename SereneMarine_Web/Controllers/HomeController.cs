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

        #region Task Methods

        public async Task<IActionResult> Index()
        {
            //get all the totals from the api
            ApiStatisticsModel model = new ApiStatisticsModel();
            string baseUrl = SD.ApiStatsPath;
            string url = baseUrl;

            response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode == false)
            {
                //set defualt values
                return View();
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            model = JsonConvert.DeserializeObject<ApiStatisticsModel>(jsonString);

            return View(model);
        }

        #endregion
    }
}
