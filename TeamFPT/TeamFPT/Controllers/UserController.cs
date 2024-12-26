using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeamFPT.Model;
using TeamFPT.Repositories;

namespace TeamFPT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly UserRepository _userRepository;
        public UserController(UserRepository userRepository) { _userRepository = userRepository; }

        [HttpPut("update")]
        [Authorize]
        public IActionResult UpdateUser([FromBody] User updatedUser)
        {
            // Retrieve the username from the token
            var usernameFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the username from the token is null or mismatched with the one in the request
            if (usernameFromToken == null || usernameFromToken != updatedUser.Username)
            {
                return Unauthorized("Bạn không có quyền chỉnh sửa thông tin này.");
            }

            // Fetch the existing user from the database
            var existingUser = _userRepository.GetUserByUsername(usernameFromToken);

            // Check if the user exists
            if (existingUser == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            // Update the fields with the provided values from the request body
            existingUser.Email = updatedUser.Email ?? existingUser.Email;
            existingUser.Role = updatedUser.Role ?? existingUser.Role;

            // Call the repository to update the user in the database
            var result = _userRepository.UpdateUser(existingUser);

            if (result)
            {
                return Ok("Thông tin người dùng đã được cập nhật.");
            }

            return BadRequest("Cập nhật không thành công.");
        }


    }
}
