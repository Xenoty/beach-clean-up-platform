﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SereneMarine_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using SereneMarine_Web.Helpers;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace SereneMarine_Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private IConfiguration configuration;

        public AccountController(IConfiguration iConfig)
        {
            configuration = iConfig;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // <post web api>
            //httpPost /users/authenticate
            //string registerUserAPI_Url = configuration.GetSection("APIConnection").GetSection("URL").Value + "users/authenticate";
            string url = SD.UserPath + "authenticate" ;
            HttpResponseMessage response = null;       
            HttpClient client = new HttpClient();

            //LOADING DATA TO JSON OBJECT
            //create <upload contents>
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            //</upload contents>

            //<upload>
            //Http Response
            //client.DefaultRequestHeaders.Add("token", token);
            response = await client.PostAsync(url, content);
            //</upload>

            if (response.IsSuccessStatusCode == false)
            {
                ApiException exception = new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };

                ViewBag.ApiError = exception;

                return View();
            }

            //READING DATA FROM JSON RESPONSE 
            //assign content returned to viewModel
            UserViewModel uservm = new UserViewModel();
            var jsonString = await response.Content.ReadAsStringAsync();
            uservm = JsonConvert.DeserializeObject<UserViewModel>(jsonString);

            //if token is null return error
            if (uservm.Token == null)
            {
                return View();
            }

            //assigning claims identity for roles
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, uservm.Email_address));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, uservm.User_Id));
            identity.AddClaim(new Claim(ClaimTypes.Role, uservm.Role));
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            //assign token to session
            HttpContext.Session.SetString("JWToken", uservm.Token);
            //TempData["alert"] = "Welcome " + uservm.Email_address;

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // <post web api>
                //httpPost /users/register
                string url = SD.UserPath + "register";
                HttpResponseMessage response = null;
                HttpClient client = new HttpClient();

                //LOADING DATA TO JSON OBJECT
                //create <upload contents>
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //</upload contents>

                //<upload>
                //Http Response
                //client.DefaultRequestHeaders.Add("token", token);
                response = await client.PostAsync(url, content);
                //</upload>

                if (response.IsSuccessStatusCode == false)
                {
                    ApiException exception = new ApiException
                    {
                        StatusCode = (int)response.StatusCode,
                        Content = "Email is already taken or invalid, please try again"
                    };

                    ViewBag.ApiError = exception;

                    return View();
                }
                //if register is sucessful, require user to login with credentials
                //tempdata for profile successfully created
                TempData["registerResult"] = $"Your profile was succesfully created.\nPlease Login to your account";

                return RedirectToAction("Login", "Account", new {returnUrl});
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.SetString("JWToken", "");
            return RedirectToAction("Index", "Home");
        }
    }
}