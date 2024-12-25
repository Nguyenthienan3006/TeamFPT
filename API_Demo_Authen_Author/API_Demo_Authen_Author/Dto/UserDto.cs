namespace API_Demo_Authen_Author.Dto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
