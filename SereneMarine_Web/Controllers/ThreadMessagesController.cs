using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SereneMarine_Web.Helpers;
using SereneMarine_Web.Models;
using SereneMarine_Web.ViewModels.ThreadMessages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SereneMarine_Web.Controllers
{
    public class ThreadMessagesController : BaseController
    {
        #region Tasks

        public async Task<IActionResult> Index(string id)
        {
            //thread no longer exists
            if (id == null)
            {
                return View();
            }

            ThreadMessagesIndexVM model = new ThreadMessagesIndexVM();
            //get thread + messages by id
            //1. get thread details
            //2. get thread Messages details
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
            //get thread details
            ThreadsModel threadDetails = JsonConvert.DeserializeObject<ThreadsModel>(jsonString);
            //assign thread details to threadmessages
            model.threadsModel = threadDetails;

            //now get thread messages details
            string messageBase = SD.ThreadsMessagesPath;
            string messagesUrl = messageBase + "thread/" + id;

            response = await client.GetAsync(messagesUrl);

            if (!response.IsSuccessStatusCode)
            {
                //create alert for error
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

                return View();
            }

            //assign response to model
            //read data from json response
            var jsonString2 = await response.Content.ReadAsStringAsync();
            model.threadMsgsList = JsonConvert.DeserializeObject<List<ThreadMessagesModel>>(jsonString2);

            //check if there are any other api errors
            if (TempData["ApiError"] != null)
            {
                ViewBag.ApiError = JsonConvert.DeserializeObject(TempData["ApiError"].ToString());
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ThreadMessagesIndexVM model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //create api url request
            string url = SD.ThreadsMessagesPath + "Create/";

            //assign header with token for validation
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //LOADING DATA  
            //create <upload contents>
            //create new model to send values to api
            ThreadMessagesModel messagesModel = new ThreadMessagesModel();
            messagesModel.thread_id = model.threadsModel.thread_id;
            messagesModel.thread_message = model.ThreadMessages.thread_message;

            var json = JsonConvert.SerializeObject(messagesModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            //</upload contents>

            //check response
            response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

                return RedirectToAction("Index", new { id = messagesModel.thread_id });
            }

            return RedirectToAction("Index", new { id = messagesModel.thread_id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string message_id, string message, string thread_id)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //create api url request
            string url = SD.ThreadsMessagesPath + "update/" + message_id;

            //assign header with token for validation
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //LOADING DATA  
            //create <upload contents>
            //create new model to send values to api
            ThreadMessagesModel messagesModel = new ThreadMessagesModel();
            messagesModel.thread_message = message;
            messagesModel.replied_date = DateTime.Now;

            var json = JsonConvert.SerializeObject(messagesModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            //</upload contents>

            //check response
            response = await client.PutAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception;

                return RedirectToAction("Index", new { id = thread_id });
            }

            //add response message update
            TempData["response"] = $"Thread message \"{message_id}\" successfully updated on {DateTime.Now}";

            return RedirectToAction("Index", new { id = thread_id });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(string message_id, string thread_id)
        {
            if (message_id == null)
            {
                //display error message
                return RedirectToAction("Index", new { id = thread_id });
            }

            string baseUrl = SD.ThreadsMessagesPath;
            string url = baseUrl + "delete/message/" + message_id;

            //assign jwt token with header
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //get response
            response = await client.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = JsonConvert.SerializeObject(exception);

                return RedirectToAction("Index", new { id = thread_id });
            }

            //create resposne message
            TempData["response"] = $"Thread message {message_id} successfully deleted on {DateTime.Now}.";
            return RedirectToAction("Index", new { id = thread_id });
        }

        #endregion
    }
}
