using System.ComponentModel.DataAnnotations;

namespace WebApplication3
{
    public class RefreshTokenRequest
    {
        [Required]
        public string? refreshToken { get; set; }
    }
}
