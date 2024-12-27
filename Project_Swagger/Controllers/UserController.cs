using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_Swagger.Models;
using Project_Swagger.Services;
using Microsoft.IdentityModel.Tokens;
using Project_Swagger.DTO;

namespace Project_Swagger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly EmailService _emailService;

        public UserController(UserService userService, EmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }

        [HttpGet("ChangePassword")]
        [Authorize]
        public IActionResult ChangePassword()
        {
            string email = User?.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return BadRequest("Email not exist");
            }

            var verificationCode = Guid.NewGuid().ToString();
            var user = _userService.GetUserByEmail(email);
            bool result = _userService.UpdateUserOTP(user.UserId, verificationCode);

            if (!result)
            {
                return BadRequest();
            }

            if (user != null)
            {

                var verificationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.UserId, token = verificationCode }, Request.Scheme);

                _emailService.SendEmailAsync(user.Email, "Your code", verificationCode);
            }
            return Ok("Please check your email for the verification code.");
        }

        [HttpPost("InputPassword")]
        [Authorize]
        public IActionResult InputPassword([FromBody] UserChangerPasswordDTO userChangerPasswordDTO)
        {
            if (!_userService.IsValidChangePassword(userChangerPasswordDTO))
            {
                return BadRequest("Password not valid");
            }

            string email = User?.FindFirstValue(ClaimTypes.Email);
            bool result = _userService.UpdatePassword(userChangerPasswordDTO, email);

            if (!result)
            {
                return BadRequest("Unsuccessfully");
            }
            return Ok("Password change page");
        }



        // GET: api/<UserController>
        [HttpGet("Admin")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<User>>> AdminGetInf()
        {
            var _user = User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (_user.IsNullOrEmpty())
            {
                return Unauthorized("User not authenticated.");
            }

            if (_user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                name = User?.FindFirstValue(ClaimTypes.NameIdentifier),
                role = User?.FindFirstValue(ClaimTypes.Role),
                email = User?.FindFirstValue(ClaimTypes.Email)
            });
        }

        [HttpGet("User")]
        [Authorize(Roles = "user")]
        public async Task<ActionResult<IEnumerable<User>>> UserGetInf()
        {
            var _user = User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (_user.IsNullOrEmpty())
            {
                return Unauthorized("User not authenticated.");
            }

            if (_user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                name = ClaimTypes.NameIdentifier.ToString(),
                role = ClaimTypes.Role.ToString(),
                email = ClaimTypes.Email.ToString()
            });
        }

        [HttpGet("Publice")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Public()
        {
            var _user = "Must login";

            return Ok(_user);
        }
    }
}
