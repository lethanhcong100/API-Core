using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.API
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
       
        public UsersController(IUserService userService)
        {
            _userService = userService;
          
        }

        [AllowAnonymous]
        // [HttpPost("authenticate/{username}/{password}")]
        
        [Route("authenticate")]
        [HttpPost]
        public async Task<IActionResult> Authenticate(User users)
        {
            var user = _userService.Authentzicate(users.Username, users.Password);
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });
            return Ok(user);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }
    }
}