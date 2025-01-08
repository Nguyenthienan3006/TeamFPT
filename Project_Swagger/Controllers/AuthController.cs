using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_Swagger.DTO;
using Project_Swagger.Models;
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
            if (!ModelState.IsValid) return BadRequest(ModelState);
            string cachedToken = RedisService.GetAccessToken(userLogin.UserName);
            if (!string.IsNullOrEmpty(cachedToken)) return Ok(new { token = cachedToken });

            Account user = _authService.Authenticate(userLogin);
            if (user == null) return NotFound("User not exist or not verify");

            string token = _authService.GenerateToken(user);
            RedisService.SetAccessToken(user.Id, token, TimeSpan.FromMinutes(5));

            return Ok(new { token });            
        }

        [HttpGet("get-token/{username}")]
        public IActionResult GetToken(string username)
        {
            string redisKey = username;
            string token = RedisService.GetAccessToken(redisKey);
            if (string.IsNullOrEmpty(token)) return NotFound("Token not found.");
            return Ok(new { Token = token });
        }

        [HttpDelete("delete-token/{username}")]
        public IActionResult DeleteToken(string username)
        {
            string redisKey = username;
            RedisService.DeleteAccessToken(redisKey);
            return Ok("Token deleted.");
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] UserRegisterDTO userRegister)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (userRegister == null) return BadRequest();
            if (!_userService.RegisterUser(userRegister)) return Conflict();

            var verificationLink = Url.Action("ConfirmEmail", "Account", new { Email = userRegister.Email , OTP = userRegister.OTP }, Request.Scheme);
            _emailService.SendEmailAsync(userRegister.Email, "Your code", userRegister.OTP);
            
            return Ok(new { Message = "Registration successful. Please check your email for the verification code." });
        }

        [HttpPost("Verify")]
        [AllowAnonymous]
        public IActionResult Verify([FromBody] VerifyEmailDTO verifyEmailDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (_userService.VerifyAccount(verifyEmailDTO.OTP, verifyEmailDTO.Email)) return Ok();
            return BadRequest();
        }
        
        [HttpPost("Resend-OTP")]
        [AllowAnonymous]
        public IActionResult Resend([FromBody] ResendOTPDTO resendOTPDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var verificationLink = Url.Action("ConfirmEmail", "Account", new { Email = resendOTPDTO.email, OTP = resendOTPDTO.OTP }, Request.Scheme);
            _emailService.SendEmailAsync(resendOTPDTO.email, "Your code", resendOTPDTO.OTP);
            if (!_userService.ResendOTP(resendOTPDTO)) return BadRequest();
            return Ok("Success");
        }
    }
}
