using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Demo_Authen_Author.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public AuthController(ITokenService tokenService, IUserService userService, IEmailService emailService)
        {
            _tokenService = tokenService;
            _userService = userService;
            _emailService = emailService;
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public Object Login([FromBody] LoginDto userLogin)
        {
            var user = _userService.Authenticate(userLogin);

            if (user != null)
            {
                var token = _tokenService.GenerateToken(user);
                return Ok(new 
                {
                    UserName = user.Username,
                    accessToken = token
                });
            }

            return NotFound();
        }
        
        
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto userRegister)
        {
            var token = Guid.NewGuid().ToString(); // Mã xác thực email
            //gửi mã về email
            var verificationLink = $"Your token = {token}";

            bool isEmailSent = await _emailService.SendEmailAsync(userRegister.Email, "Email Verification", verificationLink);

            if (isEmailSent)
            {
                bool result = await _userService.RegisterUserAsync(userRegister, token);
                if (result)
                {
                    //nếu gửi mail thành công thì thêm người dùng vào DB
                    return Ok("Registration successful. Please verify your email.");
                }
                else
                {
                    return StatusCode(500, "Something went wrong");
                }

            }            
            

            return BadRequest("Registration failed.");
        }
        
        
        [HttpPost("verifyEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] string token, string email)
        {
            var result = _userService.VerifyEmailAsync(token);

            if(result != null)
            {
                return Ok(result);
            }

            return BadRequest("Registration failed.");
        }

    }
}
