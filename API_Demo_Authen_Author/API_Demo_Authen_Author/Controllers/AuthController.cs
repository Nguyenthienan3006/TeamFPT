﻿using API_Demo_Authen_Author.Dto;
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
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (!ModelState.IsValid) return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            //kiểm tra user có tồn tại không
            var isUserExist = _userService.Authenticate(userLogin);
            if (isUserExist == null) return NotFound(new { message = "User not found" });
            else if (isUserExist.IsEmailVerified == false) return BadRequest(new { message = "Email is not verified" });

            //Mỗi lần user login thì lại cập nhật token vào DB 1 lần
            try
            {
                var token = _tokenService.GenerateToken(isUserExist);

                // Cập nhật token vào DB
                _tokenService.UpdateToken(isUserExist.Id, token, "Login", DateTime.Now.AddMinutes(30), false);

                return Ok(new
                {
                    UserName = isUserExist.Username,
                    accessToken = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
            }

        }


        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto userRegister)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Mã xác thực email
            var token = Guid.NewGuid().ToString();
            DateTime tokenExpiry = DateTime.Now.AddMinutes(30);
            var verificationLink = $"Your token is: {token}\nPlease note that your token will expire in 30 minutes at {tokenExpiry.ToString("HH:mm")}.";

            // Kiểm tra email đã tồn tại trong DB
            var existingUser = _userService.GetUserByEmail(userRegister.Email);
            if (existingUser != null) return BadRequest(new { message = "User already exists" });

            //đăng ký user
            bool result = _userService.RegisterUser(token, userRegister);

            if (result == null) return BadRequest("Registration failed.");

            //gửi email
            bool isEmailSent = await _emailService.SendEmailAsync(userRegister.Email, "Email Verification", verificationLink);

            if (isEmailSent) return Ok("Registration successful. Please verify your email.");
            else return StatusCode(500, "Something went wrong");

        }

        [HttpPost("verifyEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailAsync([FromBody] VerifyEmailRequest request)
        {
            //validate input
            if (!ModelState.IsValid) return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            //check email có tồn tại không
            var user = _userService.GetUserByEmail(request.email);
            if (user == null) return BadRequest(new { message = "Invalid credentials" });

            //Check token hết hạn chưa
            var isTokenValid = _tokenService.GetTokenInfo(user.Id, "EmailToken");
            if (isTokenValid == null || isTokenValid.expiredDate < DateTime.UtcNow)
            {
                if (await _emailService.SendNewTokenAsync(request.email, user.Id))
                    return BadRequest(new { message = "Token expired. A new token has been sent to your email." });

                return StatusCode(500, new { message = "Failed to send new token" });
            }

            // Verify token
            if (await _userService.VerifyEmailAsync(request.token, user.Id, request.email))
                return Ok(new { message = "Email verification successful" });

            return BadRequest(new { message = "Email verification failed" });
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {

            //kiểm tra user có tồn tại không
            UserDto userToChangePass = _userService.GetUserByEmail(request.Email);

            if (userToChangePass == null) return NotFound();

            //tạo mật khẩu mới
            var newPass = _userService.GenerateRandomPassword(10);

            //gửi mk mới về email
            var body = $"Your new password = {newPass}";

            bool isEmailSent = await _emailService.SendEmailAsync(userToChangePass.Email, "Email Verification", body);

            if (isEmailSent == null) return StatusCode(500, "Something went wrong");

            //cập nhật mật khẩu mới
            var result = _userService.UpdateUserPasswordAsync(userToChangePass.Id, newPass);

            if (result != null) return StatusCode(500, "Something went wrong!");

            //nếu gửi mail thành công thì thêm người dùng vào DB
            return Ok("Sent successfull, please check your email");

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
