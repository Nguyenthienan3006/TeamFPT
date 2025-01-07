using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // Admin: Admin@123, Thien An: Ann@3006
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<object> LoginAsync([FromBody] LoginDto userLogin)
        {
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (!ModelState.IsValid) return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Lấy thông tin người dùng từ DB
            var user = _userService.Authenticate(userLogin);
            if (user == null) return NotFound(new { message = "User not found" });
            if (!user.IsEmailVerified) return BadRequest(new { message = "Email is not verified" });

            // Xác thực mật khẩu
            if(!_userService.VerifyPasswordHash(userLogin.PassWord, user.passwordHash, user.passwordSalt))
                return Unauthorized("Invalid username or password");

            try
            {
                // Tạo token
                var token = _tokenService.GenerateToken(user);

                _tokenService.SaveTokenToRedisAsync(token, user.Id);      // Lưu token vào Redis với TTL (thời gian sống)
                _tokenService.UpdateToken(user.Id, token, "Login", DateTime.Now.AddMinutes(30), false);     // Cập nhật token vào DB

                return Ok(new
                {
                    UserName = user.Username,
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
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (!ModelState.IsValid) return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Mã xác thực email
            var token = Guid.NewGuid().ToString();
            DateTime tokenExpiry = DateTime.Now.AddMinutes(30);
            var verificationLink = $"Your token is: {token}\nPlease note that your token will expire in 30 minutes at {tokenExpiry.ToString("HH:mm")}.";

            // Kiểm tra email đã tồn tại chưa
            var existingUser = _userService.GetUserByEmail(userRegister.Email);
            if (existingUser != null) return BadRequest(new { message = "User already exists" });

            //Mã hóa mật khẩu
            _userService.CreatePasswordHash(userRegister.PassWord, out byte[] passwordHash, out byte[] passwordSalt);

            // Đăng ký user           
            bool result = _userService.RegisterUser(token, userRegister, passwordHash, passwordSalt);

            if (result == null) return BadRequest("Registration failed.");

            // Gửi email
            bool isEmailSent = await _emailService.SendEmailAsync(userRegister.Email, "Email Verification", verificationLink);

            if (isEmailSent) return Ok("Registration successful. Please verify your email.");
            else return StatusCode(500, "Something went wrong");

        }

        [HttpPost("verifyEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailAsync([FromBody] VerifyEmailRequest request)
        {
            // Validate input
            if (!ModelState.IsValid) 
                return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Check email có tồn tại không
            var user = _userService.GetUserByEmail(request.email);
            if (user == null) return BadRequest(new { message = "Invalid credentials" });

            // Check token hết hạn chưa
            var isTokenValid = _tokenService.GetTokenInfo(user.Id, "EmailToken");
            if (isTokenValid == null || isTokenValid.expiredDate < DateTime.UtcNow)
            {
                if (await _emailService.ReSendTokenAsync(request.email, user.Id))
                    return BadRequest(new { message = "Token expired. A new token has been sent to your email." });

                return StatusCode(500, new { message = "Failed to send new token" });
            }

            // Verify token
            if (!_userService.VerifyEmail(request.token, user.Id, request.email))
                return BadRequest(new { message = "Email verification failed" });

            return Ok(new { message = "Email verification successful" });
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            if (!ModelState.IsValid) 
                return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Kiểm tra user có tồn tại không
            var userToChangePass = _userService.GetUserByEmail(request.Email);
            if (userToChangePass == null) return NotFound("User not found!");

            // Tạo mk mới và token 
            var token = Guid.NewGuid().ToString();
            DateTime tokenExpiry = DateTime.Now.AddMinutes(30);
            var tokenInfo = $"Your token is: {token}\nPlease note that your token will expire in 30 minutes at {tokenExpiry.ToString("HH:mm")}.";
            var body = $"{tokenInfo}";

            //lưu token vào DB
            _tokenService.UpdateToken(userToChangePass.Id, token, "ForgotPassToken", DateTime.Now.AddMinutes(30), false);

            // Gửi mail
            bool isEmailSent = await _emailService.SendEmailAsync(userToChangePass.Email, "Email Verification", body);

            if (!isEmailSent) return StatusCode(500, "Failed to send email!");

            return Ok("Password reset successfully. Please check your email.");
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            // Kiểm tra đầu vào
            if (!ModelState.IsValid) 
                return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Kiểm tra token hợp lệ
            var tokenInfo = _tokenService.GetTokenInfo(resetPasswordDto.UserId, "ForgotPassToken");
            if (tokenInfo == null || tokenInfo.expiredDate < DateTime.UtcNow)
                return BadRequest(new { message = "Token has expired. Please initiate the Forgot Password process again." });

            // Cập nhật mật khẩu
            if (_userService.UpdateUserPassword(resetPasswordDto.UserId, resetPasswordDto.NewPassword))
                return Ok("Password reset successfully.");

            return BadRequest("Failed to change password.");
        }


        [HttpPost("changePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            if(!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Lấy userId từ token JWT
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Unauthorized("User is not logged in.");

            // Cập nhật mật khẩu
            if (_userService.UpdateUserPassword(userId, request.newPass))
                return Ok("Password changed successfully.");

            return BadRequest("Failed to change password.");
        }


    }
}
