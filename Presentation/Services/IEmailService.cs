using Presentation.Models;

namespace Presentation.Services
{
    public interface IEmailService
    {
        Task<EmailServiceResponse> SendVerificationEmailAsync(SendVerificationCodeDto request);
    }
}