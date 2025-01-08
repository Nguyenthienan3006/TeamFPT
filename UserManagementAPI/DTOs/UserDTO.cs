namespace UserManagementAPI.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool IsEmailVerified { get; set; }
    }
}
