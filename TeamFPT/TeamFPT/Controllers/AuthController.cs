using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeamFPT.DTO;
using TeamFPT.Model;
using TeamFPT.Repositories;
using TeamFPT.Services;

namespace TeamFPT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly UserRepository _userRepositories;
        private readonly IConfiguration _configuration;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly EmailService _emailService;

        public AuthController(UserRepository userRepositories, IConfiguration configuration, JwtTokenGenerator jwtTokenGenerator, EmailService emailService)
        {
            _userRepositories = userRepositories;
            _configuration = configuration;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
        }
        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserRequest request)
        {
            var userAuth = _userRepositories.Login(request.Username, request.Password);
            if (userAuth == null) return Unauthorized("Invalid credentials.");
            var token = _jwtTokenGenerator.GenerateToken(userAuth);
            if (!userAuth.IsVerified)
            {
                var otp = _userRepositories.GenerateOtp();
                _userRepositories.SaveOtp(userAuth.Email, otp);
                _emailService.SendOtpEmailAsync(userAuth.Email, otp);
                return Unauthorized("You must verify this account");
                
            }
            return Ok(new
            {
                token,
                user = new
                {
                    userAuth.Username,
                    userAuth.Email,
                    userAuth.UserRole,
                    userAuth.User.FirstName,
                    userAuth.User.LastName
                }
            });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] DTO.RegisterRequest user)
        {
            var validationResult = _userRepositories.ValidateUser(user);
            if (validationResult.IsValid)
            {
                _userRepositories.Register(user);
                var otp = _userRepositories.GenerateOtp();
                _userRepositories.SaveOtp(user.Email, otp);
                _emailService.SendOtpEmailAsync(user.Email, otp);
                return Ok("User registered successfully. Please verify your email with the OTP sent.");
            }
            return BadRequest(new { Errors = validationResult.Errors });
        }
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp(string email, string otp)
        {
            var result = _userRepositories.VerifyOtp(email, otp);
            if (!result)
                return BadRequest("Invalid or expired OTP.");

            return Ok("User verified successfully.");
        }
        [Authorize]
        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));  // Lấy userId từ JWT token

            try
            {
                _userRepositories.ChangePassword(userId, request.OldPassword, request.NewPassword);
                return Ok("Password changed successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to change password.");
            }
        }
        [HttpPost("request-reset-password")]
        public IActionResult RequestResetPassword([FromBody] ResetPasswordRequestcs request)
        {
            if (!_userRepositories.CheckEmailExists(request.Email)) return NotFound("Email does not exist.");
            var otp = _userRepositories.GenerateOtp();
            _userRepositories.SaveResetPasswordOtp(request.Email, otp);
            _emailService.SendOtpEmailAsync(request.Email, otp);
            return Ok("Reset password OTP sent to your email.");
        }

        [HttpPost("verify-reset-password")]
        public IActionResult VerifyResetPassword([FromBody] VerifyResetPasswordRequest request)
        {
            if (!_userRepositories.VerifyResetPasswordOtp(request.Email, request.Otp))return BadRequest("Invalid or expired OTP.");
            _userRepositories.UpdatePassword(request.Email, request.NewPassword);
            return Ok("Password reset successfully.");
        }

    }
}
