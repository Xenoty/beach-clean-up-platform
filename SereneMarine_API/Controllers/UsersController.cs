using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Services;

namespace WebApi.Controllers
{
    //[Authorize]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController(
            IUserService userService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /users/authenticate
        ///     {
        ///        "email_address": "example@domain.com",
        ///        "password" : "1234"
        ///     }
        ///
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>user info and token</returns>
        /// <response code="200">returns status of 200</response>
        /// <response code="400">authentication failed</response>  
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateModel model)
        {
            try
            {
                var user = _userService.Authenticate(model.Email_address, model.Password);

                if (user == null)
                    return BadRequest(new { message = "Username or password is incorrect" });

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, user.User_Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role),
                    }),
                    Expires = DateTime.UtcNow.AddHours(3),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // return basic user info and authentication token
                return Ok(new
                {
                    Id = user.Id,
                    User_Id = user.User_Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    email_address = user.Email_address,
                    role = user.Role,
                    ContactNo = user.ContactNo,
                    Address = user.Address,
                    Joined = user.Joined,
                    Token = tokenString
                });
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.ToString() });
            }
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /users/register
        ///     {
        ///        "firstname": "John",
        ///        "lastname": "Wick",
        ///        "email_address": "john@gmail.com",
        ///        "username": "johnny",
        ///        "contactno": 0767679334,
        ///        "address": "string"
        ///     }
        ///
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>A newly created TodoItem</returns>
        /// <response code="200">returns status of 200</response>
        /// <response code="400">if item is null or required</response>  
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            // map model to entity
            var user = _mapper.Map<User>(model);

            try
            {
                //initalise values and assign to model value
                user.Joined = DateTime.Now;
                user.User_Id = Guid.NewGuid().ToString();
                user.Role = "User";
                // create user
                _userService.Create(user, model.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var users = _userService.GetAll();
                var model = _mapper.Map<IList<UserModel>>(users);
                return Ok(model);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets user by specific id
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            try
            {
                var user = _userService.GetById(id);
                var model = _mapper.Map<UserModel>(user);
                return Ok(model);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates user by id
        /// </summary>
        [HttpPut("update/{id}")]
        public IActionResult Update(string id, [FromBody] UpdateModel model)
        {
            // map model to entity and set id
            var user = _mapper.Map<User>(model);
            user.User_Id = id;

            try
            {
                // update user 
                _userService.Update(user, model.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes user by id
        /// </summary>
        [HttpDelete("delete/{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                _userService.Delete(id);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}