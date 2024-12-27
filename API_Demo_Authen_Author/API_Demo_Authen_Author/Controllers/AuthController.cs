using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using System.Security.Claims;

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
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            UserDto userToVerify = await _userService.GetUserByEmailAsync(request.email);

            if (userToVerify != null)
            {
                var result = _userService.VerifyEmailAsync(request.token, userToVerify.Id, request.email);

                if (result != null)
                {
                    return Ok("Verify successfull");
                }
            }

            return BadRequest("Registration failed.");
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {

            //kiểm tra user có tồn tại không
            UserDto userToChangePass = await _userService.GetUserByEmailAsync(request.Email);

            if (userToChangePass != null)
            {
                //tạo mật khẩu mới
                var newPass = _userService.GenerateRandomPassword(10);

                //gửi mã về email
                var body = $"Your new password = {newPass}";

                bool isEmailSent = await _emailService.SendEmailAsync(userToChangePass.Email, "Email Verification", body);

                if (isEmailSent)
                {
                    //cập nhật mật khẩu mới
                    var result = _userService.UpdateUserPasswordAsync(userToChangePass.Id, newPass);

                    if (result != null)
                    {
                        //nếu gửi mail thành công thì thêm người dùng vào DB
                        return Ok("Sent successfull, please check your email");
                    }
                    return StatusCode(500, "Something went wrong!");

                }

                return StatusCode(500, "Something went wrong");


            }
            return NotFound();
        }

        [HttpPost("changePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] string newPass)
        {
            //lấy ra userId
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //cập nhật pass mới
            var result = _userService.UpdateUserPasswordAsync(userId, newPass);

            if (result != null)
            {
                return Ok("Change pass successfull");
            }

            return BadRequest("Change pass fail");
        }

    }
}
