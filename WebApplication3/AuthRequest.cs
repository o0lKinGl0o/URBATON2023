using System.ComponentModel.DataAnnotations;

namespace WebApplication3
{
    public class AuthRequest
    {
        [Required]
        public string? email { get; set; }

        [Required]
        public string? password { get; set; }
    }
}
