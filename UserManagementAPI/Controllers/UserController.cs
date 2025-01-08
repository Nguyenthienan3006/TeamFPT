using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementAPI.Data;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserStore _userStore;

        public UserController(UserStore userStore)
        {
            _userStore = userStore;
        }
                
        
        [HttpGet("Admin")]
        [Authorize(Roles = "admin")]
        public IActionResult AdminGetUser()
        {
            if (!User.IsInRole("admin")) return BadRequest("You must be admin to view");
         
            var allUser = _userStore.GetUsers();
            return Ok(allUser);
        }

        [HttpGet("User")]
        [Authorize]
        public IActionResult GetUsers()
        {
            var users = _userStore.GetUsers();
            return Ok(users);
        }


    }
}
