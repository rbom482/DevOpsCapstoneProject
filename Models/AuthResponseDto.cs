namespace LogiTrack.Models
{
    public class AuthResponseDto
    {
        public required string Token { get; set; }
        public required string UserId { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime Expiration { get; set; }
    }
}