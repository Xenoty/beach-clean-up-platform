using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SereneMarine_Web.Helpers;
using SereneMarine_Web.ViewModels.EventAttendace;
using SereneMarine_Web.ViewModels.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SereneMarine_Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventsController : Controller
    {
        //no authorize
        //anyone can see all events
        HttpClient client = new HttpClient();
        HttpResponseMessage response = null;
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(EventsIndexViewModel filter, string submit, string clear)
        {

            if (!string.IsNullOrEmpty(clear))
            {
                ModelState.Clear();
            }
            //initalize variables
            EventsIndexViewModel eventsIndexViewModel = new EventsIndexViewModel();

            //build the url for request
            string baseUrl = SD.EventsPath;
            string url = baseUrl;

            response = await client.GetAsync(url);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                //create alert for error
                return View();
            }

            //read data from json response
            var jsonString = await response.Content.ReadAsStringAsync();
            //deserialize it and assign to list model
            eventsIndexViewModel.EventsViewModel = JsonConvert.DeserializeObject<List<EventsViewModel>>(jsonString);

   

            //build url for event_attendance
            if (User.Identity.IsAuthenticated)
            {
                //get claim of user_id
                var principal = (ClaimsIdentity)User.Identity;
                //get user_id through claimtype
                string user_id = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
                //build url for api request
                string baseAttendanceUrl = SD.EventAttendancePath;
                bool eventAttended = false;
                string attUrl = baseAttendanceUrl + "user/" + user_id + "&" + eventAttended;

                //RETRIEVE Response from api
                response = await client.GetAsync(attUrl);

                if (response.IsSuccessStatusCode == false)
                {
                    return View(eventsIndexViewModel);
                }

                //read data from json response
                var jsonString2 = await response.Content.ReadAsStringAsync();
                //deserialize it and assign to list model
                eventsIndexViewModel.EventAttendanceViewModel = JsonConvert.DeserializeObject<List<EventAttendanceViewModel>>(jsonString2);

                //perform logic to assign if user has attended the event or not
                for (int i = 0; i < eventsIndexViewModel.EventsViewModel.Count; i++)
                {
                    string event_id = eventsIndexViewModel.EventsViewModel[i].event_id;
                    for (int j = 0; j < eventsIndexViewModel.EventAttendanceViewModel.Count; j++)
                    {
                        string attendance_id = eventsIndexViewModel.EventAttendanceViewModel[j].event_id;
                        if (event_id == attendance_id)
                        {
                            eventsIndexViewModel.EventsViewModel[i].matching_user = true;
                            break;
                        }
                    }
                }

            }

            if (string.IsNullOrEmpty(submit))
            {
                //only return events that haven't been completed and not < current date
                eventsIndexViewModel.EventsViewModel = eventsIndexViewModel.EventsViewModel.Where(x => x.event_enddate >= DateTime.Now && x.event_completed == false).OrderBy(x => x.event_startdate).ToList();
            }
            else
            {
                bool upcomming = false;
                bool userCompleted = false;
                bool end_date = false;

                if (filter.filterEvents.UserCompleted == true)
                {
                    userCompleted = true;
                    eventsIndexViewModel.EventsViewModel = eventsIndexViewModel.EventsViewModel.Where(x => true && x.matching_user == true).OrderBy(x => x.event_startdate).ToList();
                }
                if (filter.filterEvents.Completed == true && userCompleted == false)
                {
                    if (filter.filterEvents.Current == true)
                    {
                        upcomming = true;
                        eventsIndexViewModel.EventsViewModel = eventsIndexViewModel.EventsViewModel.Where(x => true).OrderBy(x => x.event_startdate).ToList();
                    }
                    else
                    {
                        //only return events that haven't been completed and not < current date
                        eventsIndexViewModel.EventsViewModel = eventsIndexViewModel.EventsViewModel.Where(x => x.event_enddate <= DateTime.Now /*|| x.event_completed == false*/).OrderBy(x => x.event_startdate).ToList();
                    }
                }
                if (filter.filterEvents.Current == true && upcomming == false && userCompleted == false)
                {
                    eventsIndexViewModel.EventsViewModel = eventsIndexViewModel.EventsViewModel.Where(x => x.event_enddate >= DateTime.Now || x.event_completed == true).OrderBy(x => x.event_startdate).ToList();
                }

                if (filter.filterEvents.event_startdate != null && filter.filterEvents.event_startdate != default(DateTime))
                {
                    if (filter.filterEvents.event_enddate != null)
                    {
                        end_date = true;
                        eventsIndexViewModel.EventsViewModel = eventsIndexViewModel.EventsViewModel.Where(x => x.event_startdate >= filter.filterEvents.event_startdate
                        && x.event_enddate <= filter.filterEvents.event_enddate).OrderBy(x => x.event_startdate).ToList();
                    }
                    else
                    {
                        eventsIndexViewModel.EventsViewModel = eventsIndexViewModel.EventsViewModel.Where(x => x.event_startdate >= filter.filterEvents.event_startdate).OrderBy(x => x.event_startdate).ToList();
                    }
                }
                if (filter.filterEvents.event_enddate != null && filter.filterEvents.event_enddate != default(DateTime) && end_date == false)
                {
                    eventsIndexViewModel.EventsViewModel = eventsIndexViewModel.EventsViewModel.Where(x => x.event_enddate <= filter.filterEvents.event_enddate).OrderBy(x => x.event_startdate).ToList();
                }
                if (filter.filterEvents.max_attendance != 0 && filter.filterEvents.max_attendance != null)
                {
                    eventsIndexViewModel.EventsViewModel = eventsIndexViewModel.EventsViewModel.Where(x => x.max_attendance <= filter.filterEvents.max_attendance).OrderBy(x => x.event_startdate).ToList();
                }
            }


            return View(eventsIndexViewModel);
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
            //initalize variables
            EventsViewModel evm = new EventsViewModel();

            //build the url for request
            string baseUrl = SD.EventsPath;
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
            evm = JsonConvert.DeserializeObject<EventsViewModel>(jsonString);

            ViewBag.Attending = Math.Round((((double)evm.current_attendance / (double)evm.max_attendance) * 100), 1);


            return View(evm);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Details(string id, string email, string event_name)
        {
            EventAttendaceRegisterViewModel model = new EventAttendaceRegisterViewModel();
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
                user_id = email;            
            }

            //assign variables to model
            model.event_id = id;
            model.user_id = user_id;

            //build the url for request
            string baseUrl = SD.EventAttendancePath;
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

                return RedirectToAction("Details", "Events", new {id});
            }
          
            //maybe add tempdata or return url 
            //maybe redirect to EventsPage with tempdata showing joining event was successful
            TempData["response"] = $"Successfully joined Event {event_name} on {DateTime.Now}";
      
            return RedirectToAction("Index", "Events");
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EventUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            //lets convert comma to decimal 
            model.longitude = Math.Round(Convert.ToDouble(model.longitude, new CultureInfo("en-GB")), 7);
            model.latitude = Math.Round(Convert.ToDouble(model.latitude, new CultureInfo("en-GB")), 7);

            //create api url request
            string url = SD.EventsPath + "create/";

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
            ViewBag.Response = $"Event \"{model.event_name}\" successfully created on {DateTime.Now}";
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
                RedirectToAction("Index", "Events");
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
                RedirectToAction("Details", "Events", new {id});
            }

            //now get event details by id and add to model to display
            //build the url for request
            string baseUrl = SD.EventsPath;
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

            EventUpdateModel eventUpdate = new EventUpdateModel();
            eventUpdate = JsonConvert.DeserializeObject<EventUpdateModel>(jsonString);

            return View(eventUpdate);
        }

  
        [HttpPost]
        public async Task<IActionResult> Update(EventUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            //lets convert comma to decimal 
            model.longitude = Math.Round(Convert.ToDouble(model.longitude, new CultureInfo("en-GB")), 7);
            model.latitude = Math.Round(Convert.ToDouble(model.latitude, new CultureInfo("en-GB")), 7);
            //create api url request
            string url = SD.EventsPath + "Update/" + model.event_id;

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
            TempData["response"] = $"Event \"{model.event_name}\" successfully updated on {DateTime.Now}";

            return RedirectToAction("Details", "Events", new { id = model.event_id });
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

            string baseUrl = SD.EventsPath;
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

            EventUpdateModel model = new EventUpdateModel();
            model = JsonConvert.DeserializeObject<EventUpdateModel>(jsonString);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id, string event_name)
        {
            //build the url for request
            string baseUrl = SD.EventsPath;
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
                return RedirectToAction("Delete", "Events", new { id });
            }

            TempData["response"] = $"Event \"{event_name}\" successfully deleted on {DateTime.Now}";

            return RedirectToAction("Index");
        }


    }
}
