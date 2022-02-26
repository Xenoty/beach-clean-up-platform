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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SereneMarine_Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventsController : BaseController
    {
        #region Views

        public ActionResult Create() => View();

        #endregion

        private EventsIndexViewModel previousEventsIndexViewModel;

        #region Task Methods

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(EventsIndexViewModel filter, string submit, string clear)
        {
            bool clearFitler  = !string.IsNullOrEmpty(clear);
            if (clearFitler)
            {
                ModelState.Clear();
                filter = null;
                submit = string.Empty;
            }
            //initalize variables
            EventsIndexViewModel eventsIndexViewModel = new EventsIndexViewModel();

            //build the url for request
            string baseUrl = SD.EventsPath;
            response = await client.GetAsync(baseUrl);

            if (!response.IsSuccessStatusCode)
            {
                //create alert for error
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

                return View(previousEventsIndexViewModel);
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

                if (!response.IsSuccessStatusCode)
                {
                    //create alert for error
                    ApiException exception = new ApiException(response);
                    TempData["ApiError"] = exception.GetApiErrorMessage();

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

            IEnumerable<EventsViewModel> eventsViewModelEnumerable = null;

            if (string.IsNullOrEmpty(submit))
            {
                eventsViewModelEnumerable = eventsIndexViewModel.EventsViewModel.Where(x => x.event_startdate >= DateTime.Now);
                if (eventsViewModelEnumerable.Count() == 0)
                {
                    // If there are no upcomming events, show all events
                    eventsViewModelEnumerable = eventsIndexViewModel.EventsViewModel.Where(x => true);
                }
            }
            else
            {
                eventsViewModelEnumerable = GetFilteredEventsAsIEnumerable(eventsIndexViewModel, filter);
            }

            // Order by descending by default
            eventsIndexViewModel.EventsViewModel = eventsViewModelEnumerable.OrderByDescending(x => x.event_startdate).ToList();
            if (previousEventsIndexViewModel != eventsIndexViewModel)
            {
                previousEventsIndexViewModel = eventsIndexViewModel;
            }

            if (clearFitler)
            {
                return RedirectToAction("Index", "Events");
            }

            return View(eventsIndexViewModel);
        }

        private IEnumerable<EventsViewModel> GetFilteredEventsAsIEnumerable(EventsIndexViewModel eventsIndexViewModel, EventsIndexViewModel filter)
        {
            IEnumerable<EventsViewModel> eventsViewModelEnumerable = null;

            bool userHasParticpatedInEvent = filter.filterEvents.UserParticipatedEvents;
            bool getUpcommingEvents = filter.filterEvents.GetUpCommingEvents;
            bool getPastEvents = filter.filterEvents.GetPastEvents;
            DateTime? startDate = filter.filterEvents.EventStartDate;
            DateTime? endDate = filter.filterEvents.EventEndDate;
            int? maxAttendance = filter.filterEvents.MaxAttendance;

            bool getStartDate = startDate.HasValue;
            bool getEndDate = endDate.HasValue;
            bool getMaxAttendance = maxAttendance.HasValue;

            eventsViewModelEnumerable = eventsIndexViewModel.EventsViewModel.Where(x => x.matching_user == userHasParticpatedInEvent);

            if (getUpcommingEvents)
            {
                eventsViewModelEnumerable = eventsViewModelEnumerable.Where(x => x.event_startdate >= DateTime.Now);
            }
            else if (getPastEvents)
            {
                eventsViewModelEnumerable = eventsViewModelEnumerable.Where(x => x.event_startdate <= DateTime.Now);
            }

            if (getStartDate)
            {
                eventsViewModelEnumerable = eventsViewModelEnumerable.Where(x => x.event_startdate > startDate);
            }

            if (getEndDate)
            {
                eventsViewModelEnumerable = eventsViewModelEnumerable.Where(x => x.event_enddate < endDate);
            }

            if (getMaxAttendance)
            {
                eventsViewModelEnumerable = eventsViewModelEnumerable.Where(x => x.max_attendance <= maxAttendance);
            }

            return eventsViewModelEnumerable;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            //initalize variables
            EventsViewModel evm = new EventsViewModel();

            //build the url for request
            string baseUrl = SD.EventsPath;
            string url = baseUrl + id;

            response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

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
            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

                return RedirectToAction("Details", "Events", new { id });
            }

            TempData["response"] = $"Successfully joined Event {event_name} on {DateTime.Now}";

            return RedirectToAction("Index", "Events");
        }

        [HttpPost]
        public async Task<IActionResult> Create(EventUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

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

            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

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

            if (!response.IsSuccessStatusCode)
            {
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();
                //create tempdata to store and display
                RedirectToAction("Details", "Events", new { id });
            }

            //now get event details by id and add to model to display
            //build the url for request
            string baseUrl = SD.EventsPath;
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

            if (!response.IsSuccessStatusCode)
            {
                //display api error message
                //create alert for error
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

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

            if (!response.IsSuccessStatusCode)
            {
                //create alert for error
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

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

            if (!response.IsSuccessStatusCode)
            {
                //create alert for error
                ApiException exception = new ApiException(response);
                TempData["ApiError"] = exception.GetApiErrorMessage();

                return RedirectToAction("Delete", "Events", new { id });
            }

            TempData["response"] = $"Event \"{event_name}\" successfully deleted on {DateTime.Now}";

            return RedirectToAction("Index");
        }

        #endregion   
    }
}