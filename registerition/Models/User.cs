namespace registerition.Models
{
    public class User
    {

        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "patient"; // Default role is "patient"
        public string? ImageUrl { get; set; }

    }
}
