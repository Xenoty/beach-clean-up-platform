using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SereneMarine_Web.Models;
using SereneMarine_Web.Helpers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using SereneMarine_Web.ViewModels.Threads;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text;
using System;
using SereneMarine_Web.ViewModels.Events;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SereneMarine_Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ThreadsController : Controller
    {
        //
        HttpClient client = new HttpClient();
        HttpResponseMessage response = null;
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ThreadsModel model = new ThreadsModel();

            string baseUrl = SD.ThreadsPath;
            string url = baseUrl;

            response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode == false)
            {
                //create alert for error
                //api error
                return View();
            }

            //read data from json response
            var jsonString = await response.Content.ReadAsStringAsync();

            model.ThreadsViewModel = JsonConvert.DeserializeObject<List<ThreadsModel>>(jsonString);

            //sort threads by created date
            model.ThreadsViewModel.OrderBy(a => a.created_date);

            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ThreadUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //create api url request
            string url = SD.ThreadsPath + "create/";

            //assign header with token for validation
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //LOADING DATA  
            //create <upload contents>
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            //</upload contents>

            //check response
            response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode == false)
            {
                //display api error message
                //create alert for error
                ApiException exception = new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };
                ViewBag.ApiError = JsonConvert.SerializeObject(exception);

                return View();
            }

            //insert in api is successful
            //redirect to details page with succes message
            ViewBag.Response = $"Thread \"{model.thread_topic}\" successfully created on {DateTime.Now}";
            //clear the model so they can create a new event
            ModelState.Clear();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            if (id == null)
            {
                //redirect with error 
                TempData["RoutingError"] = "An error occured, please make sure the url is correct.";
                RedirectToAction("Index", "Threads");
            }

            //validate token
            //assign token to session
            string url = SD.apiBaseURL;
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode == false)
            {
                //create new api error code
                //create tempdata to store and display
                RedirectToAction("Index", "Threads");
            }

            //now get event details by id and add to model to display
            //build the url for request
            string baseUrl = SD.ThreadsPath;
            string apiUrl = baseUrl + id;

            response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode == false)
            {
                //create alert for error
                ApiException exception = new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };

                TempData["ApiError"] = JsonConvert.SerializeObject(exception);
                return View();
            }
            //read data from json response
            var jsonString = await response.Content.ReadAsStringAsync();

            ThreadUpdateViewModel threadUpdate = new ThreadUpdateViewModel();
            threadUpdate = JsonConvert.DeserializeObject<ThreadUpdateViewModel>(jsonString);

            return View(threadUpdate);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ThreadUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //create api url request
            string url = SD.ThreadsPath + "Update/" + model.thread_id;

            //assign header with token for validation
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //LOADING DATA  
            //create <upload contents>
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            //</upload contents>

            //check response
            response = await client.PutAsync(url, content);

            if (response.IsSuccessStatusCode == false)
            {
                //display api error message
                //create alert for error
                ApiException exception = new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };
                ViewBag.ApiError = JsonConvert.SerializeObject(exception);

                return View();
            }

            //insert in api is successful
            //redirect to details page with succes message
            TempData["response"] = $"Thread \"{model.thread_topic}\" successfully updated on {DateTime.Now}";

            return RedirectToAction("Index", "Threads");
        }

        //VIEW
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                //redirect with error 
                TempData["RoutingError"] = "An error occured, please make sure the url is correct.";
                RedirectToAction("Index", "Threads");
            }

            string baseUrl = SD.ThreadsPath;
            string apiUrl = baseUrl + id;

            response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode == false)
            {
                //create alert for error
                ApiException exception = new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };

                TempData["ApiError"] = JsonConvert.SerializeObject(exception);
                return View();
            }

            //read data from json response
            var jsonString = await response.Content.ReadAsStringAsync();

            ThreadDeleteViewModel model = new ThreadDeleteViewModel();
            model = JsonConvert.DeserializeObject<ThreadDeleteViewModel>(jsonString);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id, string thread_topic)
        {
            //build the url for request
            string baseUrl = SD.ThreadsPath;
            string apiUrl = baseUrl + "Delete/thread/" + id;
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            response = await client.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode == false)
            {
                //create alert for error
                ApiException exception = new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };

                TempData["ApiError"] = JsonConvert.SerializeObject(exception);
                return RedirectToAction("Delete", "Threads", new { id = id});
            }

            TempData["response"] = $"Thread \"{thread_topic}\" successfully deleted on {DateTime.Now}";

            return RedirectToAction("Index");
        }



    }
}
