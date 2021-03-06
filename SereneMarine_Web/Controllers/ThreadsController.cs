using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SereneMarine_Web.Helpers;
using SereneMarine_Web.Models;
using SereneMarine_Web.ViewModels.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SereneMarine_Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ThreadsController : BaseController
    {
        #region Views

        public ActionResult Create() => View();

        #endregion

        #region Tasks

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ThreadsModel model = new ThreadsModel();

            string baseUrl = SD.ThreadsPath;
            string url = baseUrl;

            response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

                return View();
            }

            //read data from json response
            var jsonString = await response.Content.ReadAsStringAsync();

            model.ThreadsViewModel = JsonConvert.DeserializeObject<List<ThreadsModel>>(jsonString);

            //sort threads by created date
            model.ThreadsViewModel = model.ThreadsViewModel.OrderByDescending(a => a.created_date).ToList();

            return View(model);
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

            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

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

            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

                RedirectToAction("Index", "Threads");
            }

            //now get event details by id and add to model to display
            //build the url for request
            string baseUrl = SD.ThreadsPath;
            string apiUrl = baseUrl + id;

            response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                //create alert for error
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

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

            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

                return View();
            }

            //insert in api is successful
            //redirect to details page with succes message
            TempData["response"] = $"Thread \"{model.thread_topic}\" successfully updated on {DateTime.Now}";

            return RedirectToAction("Index", "Threads");
        }

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

            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

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

            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();
                return RedirectToAction("Delete", "Threads", new { id = id });
            }

            TempData["response"] = $"Thread \"{thread_topic}\" successfully deleted on {DateTime.Now}";

            return RedirectToAction("Index");
        }

        #endregion
    }
}