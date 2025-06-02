using System.ComponentModel.DataAnnotations;

namespace Presentation.Models
{
    public class SendVerificationCodeDto
    {
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Code { get; set; } = null!;
    }
}
