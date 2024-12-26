using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_Swagger.DTO;
using Project_Swagger.Services;

namespace Project_Swagger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;
        private readonly EmailService _emailService;

        public AuthController(AuthService authService, UserService userService, EmailService emailService)
        {
            _authService = authService;
            _userService = userService;
            _emailService = emailService;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] UserDTO userLogin)
        {
            var user = _authService.Authenticate(userLogin);

            if (user != null)
            {
                var token = _authService.GenerateToken(user);
                return Ok(token);
            }

            return NotFound("User not exist or not verify");
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] UserRegisterDTO userRegister)
        {
            if (!_authService.IsValidPassword(userRegister))
            {
                return UnprocessableEntity();
            }

            if (userRegister != null)
            {
                bool action = _userService.AddAnUser(userRegister);
                if (action == false)
                {
                    return Conflict();
                }
            }
            
            var verificationCode = userRegister.OTP;
            var user = _userService.GetUserByEmail(userRegister.Email);
            if (user != null)
            {

                var verificationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.UserId, token = verificationCode }, Request.Scheme);

                _emailService.SendEmailAsync(user.Email, "Your code", verificationCode);
            }
            return Ok(new { Message = "Registration successful. Please check your email for the verification link." });
        }

        [HttpPost("Verify")]
        [AllowAnonymous]
        public IActionResult Verify([FromBody] string otp)
        {
            var user = _userService.GetUserByEmail(otp);
            if (user != null)
            {
                bool result = _userService.UpdateEmailVerified(user.UserId);
                return Ok(result);
            }
            return BadRequest();
        }
    }
}
