using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SereneMarine_Web.Helpers;
using SereneMarine_Web.Models;

namespace SereneMarine_Web.Controllers
{
    public class HomeController : Controller
    {
        private HttpClient client = new HttpClient();
        private HttpResponseMessage response = null;

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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
