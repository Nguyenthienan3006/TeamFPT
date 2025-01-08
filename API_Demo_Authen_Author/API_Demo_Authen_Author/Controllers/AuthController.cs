using API_Demo_Authen_Author.DataAccess;
using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Models;
using API_Demo_Authen_Author.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
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
        private readonly DBM _dbm;

        public AuthController(ITokenService tokenService, IUserService userService, IEmailService emailService, DBM dbm)
        {
            _tokenService = tokenService;
            _userService = userService;
            _emailService = emailService;
            _dbm = dbm;
        }

        // Admin: Admin@123, Thien An: Ann@3006
        [HttpPost("login")]
        [AllowAnonymous]
        public object Login([FromBody] LoginDto userLogin)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Lấy thông tin người dùng từ DB
            var user = _userService.Authenticate(userLogin);
            if (user == null) return NotFound(new { message = "User not found" });
            if (!user.IsEmailVerified) return BadRequest(new { message = "Email is not verified" });

            // Xác thực mật khẩu
            if (!_userService.VerifyPasswordHash(userLogin.PassWord, user.passwordHash, user.passwordSalt)) return Unauthorized("Invalid username or password");

            // Tạo token
            var token = _tokenService.GenerateToken(user);
            _tokenService.SaveTokenToRedisAsync(token, user.UserId);      // Lưu token vào Redis với TTL (thời gian sống)

            // Sử dụng DMB để ghi log mỗi khi người dùng đăng nhập
            _dbm.InsertLoginLog(user.UserId);

            return Ok(new { UserName = user.Username, accessToken = token });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterDto userRegister)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Kiểm tra email đã tồn tại chưa
            var existingUser = _userService.GetUserByEmail(userRegister.Email);
            if (existingUser != null) return BadRequest(new { message = "User already exists" });

            //Mã hóa mật khẩu
            _userService.CreatePasswordHash(userRegister.PassWord, out byte[] passwordHash, out byte[] passwordSalt);
            // Đăng ký user           
            bool result = _userService.RegisterUser(Guid.NewGuid().ToString(), userRegister, passwordHash, passwordSalt);
            if (result == null) return BadRequest("Registration failed.");

            // Mã xác thực email và gửi mail
            string verificationLink = $"Your token is: {Guid.NewGuid().ToString()}\nPlease note that your token will expire in 30 minutes.";
            bool isEmailSent = _emailService.SendEmail(userRegister.Email, "Email Verification", verificationLink);

            return isEmailSent ? Ok("Registration successful. Please verify your email.") : StatusCode(500, "Something went wrong");

        }

        [HttpPost("verifyEmail")]
        [AllowAnonymous]
        public IActionResult VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Check email có tồn tại không
            var user = _userService.GetUserByEmail(request.email);
            if (user == null) return BadRequest(new { message = "Invalid credentials" });

            // Check token hết hạn chưa
            var isTokenValid = _tokenService.GetTokenInfo(user.UserId, "EmailToken");
            if (isTokenValid == null || isTokenValid.expiredDate < DateTime.UtcNow)
            {
                if (_emailService.ReSendToken(request.email, user.UserId))
                    return BadRequest(new { message = "Token expired. A new token has been sent to your email." });

                return StatusCode(500, new { message = "Failed to send new token" });
            }

            if (!_userService.VerifyEmail(request.token, user.UserId, request.email))
                return BadRequest(new { message = "Email verification failed" });

            return Ok(new { message = "Email verification successful" });
        }

        [HttpPost("forgotPassword")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Kiểm tra user có tồn tại không
            var userToChangePass = _userService.GetUserByEmail(request.Email);
            if (userToChangePass == null) return NotFound("User not found!");

            var token = Guid.NewGuid().ToString();
            _tokenService.UpdateToken(userToChangePass.UserId, token, "ForgotPassToken", DateTime.Now.AddMinutes(30), false);

            // Gửi mail
            bool isEmailSent = _emailService.SendEmail(userToChangePass.Email, "Email Verification", $"Your token is: {token}\nIt will expire in 30 minutes.");

            return isEmailSent ? Ok("Password reset successfully. Please check your email.") : StatusCode(500, "Failed to send email!");
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var tokenInfo = _tokenService.GetTokenInfo(resetPasswordDto.UserId, "ForgotPassToken");
            if (tokenInfo == null || tokenInfo.expiredDate < DateTime.UtcNow)
                return BadRequest(new { message = "Token has expired. Please initiate the Forgot Password process again." });

            if (_userService.UpdateUserPassword(resetPasswordDto.UserId, resetPasswordDto.NewPassword)) return Ok("Password reset successfully.");

            return BadRequest("Failed to change password.");
        }


        [HttpPost("changePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Unauthorized("User is not logged in.");

            if (_userService.UpdateUserPassword(userId, request.newPass))
                return Ok("Password changed successfully.");

            return BadRequest("Failed to change password.");
        }


    }
}
