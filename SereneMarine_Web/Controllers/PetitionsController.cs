using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SereneMarine_Web.Helpers;
using SereneMarine_Web.Models;
using SereneMarine_Web.ViewModels.Petitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SereneMarine_Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PetitionsController : BaseController
    {
        #region Views

        public ActionResult Create() => View();

        #endregion

        #region Tasks

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            PetitionsModel model = new PetitionsModel();

            string baseUrl = SD.PetitionsPath;
            string url = baseUrl;

            response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode == false)
            {
                //create alert for error
                return View();
            }

            //read data from json response
            var jsonString = await response.Content.ReadAsStringAsync();

            model.PetitionsViewModel = JsonConvert.DeserializeObject<List<PetitionsModel>>(jsonString);

            model.PetitionsViewModel.OrderBy(a => a.created_date);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePetitionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //create api url request
            string url = SD.PetitionsPath + "create/";

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
                ViewBag.ApiError = exception;
                return View();
            }

            //insert in api is successful
            //redirect to details page with succes message
            ViewBag.Response = $"Petition \"{model.name}\" successfully created on {DateTime.Now}";
            //clear the model so they can create a new petition
            ModelState.Clear();

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            if (TempData["ApiError"] != null)
            {
                ApiException exception = JsonConvert.DeserializeObject<ApiException>((string)TempData["ApiError"]);
                ViewBag.ApiError = exception;
            }
            //initalize variables petitions table
            PetitionDetailViewModel model = new PetitionDetailViewModel();



            //build the url for petitions table request
            string baseUrl = SD.PetitionsPath;
            string url = baseUrl + id;

            response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode == false)
            {
                //create alert for error
                return View();
            }

            //read data from json response
            var jsonString = await response.Content.ReadAsStringAsync();
            //deserialize it and assign to list model
            model = JsonConvert.DeserializeObject<PetitionDetailViewModel>(jsonString);

            //get all petitions signed by petition_id
            string baseUrlPs = SD.PetitionsSignedPath;
            string urlPs = baseUrlPs + "signature/" + id;

            response = await client.GetAsync(urlPs);

            if (response.IsSuccessStatusCode == false)
            {
                return View();
            }

            jsonString = await response.Content.ReadAsStringAsync();
            model.petitionsSigned = JsonConvert.DeserializeObject<List<PetitionsSignedViewModel>>(jsonString);
            model.petitionsSigned = model.petitionsSigned.Take(5).OrderBy(x => x.signed_date).ToList();

            ViewBag.Percent = Convert.ToString(Math.Round(((double)model.current_signatures / (double)model.required_signatures) * 100, 2)).Replace(",", ".");

            return View(model);
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Details(PetitionDetailViewModel model, string email)
        {
            model.Email = email;
            var user_id = "";
            //check if user is logged in or not
            if (User.Identity.IsAuthenticated)
            {
                //Get user_id through claim
                var principal = (ClaimsIdentity)User.Identity;
                user_id = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
            else
            {
                //use the userdetails in the form
                //need to add if statement for onclick of Join button in view
                user_id = model.Email;
            }

            //assign variables to model
            model.petition_id = model.petition_id;
            model.User_Id = user_id;

            //build the url for request
            string baseUrl = SD.PetitionsSignedPath;
            string url = baseUrl + "create";

            //LOADING DATA TO JSON OBJECT
            //create <upload contents>
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            //</upload contents>

            //<upload>
            //Http Response
            response = await client.PostAsync(url, content);
            //</upload>
            if (response.IsSuccessStatusCode == false)
            {
                ApiException exception = new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };

                TempData["ApiError"] = JsonConvert.SerializeObject(exception);

                return RedirectToAction("Details", "Petitions", new { model.petition_id });
            }

            TempData["response"] = $"Successfully signed Petition {model.name} on {DateTime.Now}";

            return RedirectToAction("Index", "Petitions");
        }

        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            if (id == null)
            {
                //redirect with error 
                TempData["RoutingError"] = "An error occured, please make sure the url is correct.";
                RedirectToAction("Index", "Petitions");
            }

            //validate token
            //assign token to session
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //now get event details by id and add to model to display
            //build the url for request
            string baseUrl = SD.PetitionsPath;
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

            CreatePetitionViewModel model = new CreatePetitionViewModel();
            model = JsonConvert.DeserializeObject<CreatePetitionViewModel>(jsonString);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CreatePetitionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //create api url request
            string url = SD.PetitionsPath + "Update/" + model.petition_id;

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
            TempData["response"] = $"Petition \"{model.name}\" successfully updated on {DateTime.Now}";

            return RedirectToAction("Index", "Petitions");

        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                //redirect with error 
                TempData["RoutingError"] = "An error occured, please make sure the url is correct.";
                RedirectToAction("Index", "Petitions");
            }

            string baseUrl = SD.PetitionsPath;
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

            PetitionsModel model = new PetitionsModel();
            model = JsonConvert.DeserializeObject<PetitionsModel>(jsonString);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id, string name)
        {
            //build the url for request
            string baseUrl = SD.PetitionsPath;
            string apiUrl = baseUrl + "Delete/" + id;
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
                return RedirectToAction("Delete", "Petitions", new { id });
            }

            TempData["response"] = $"Petition \"{name}\" successfully deleted on {DateTime.Now}";

            return RedirectToAction("Index");
        }

        #endregion

    }
}
