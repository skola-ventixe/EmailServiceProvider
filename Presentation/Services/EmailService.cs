using System.Diagnostics;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Presentation.Models;

namespace Presentation.Services;



public class EmailService(IConfiguration configuration) : IEmailService
{
    private readonly IConfiguration _configuration = configuration;

    public async Task<EmailServiceResponse> SendVerificationEmailAsync(SendVerificationCodeDto request)
    {
        try
        {
            var connectionString = _configuration["EmailConfig:Email"];
            var emailClient = new EmailClient(connectionString);


            var emailMessage = new EmailMessage(
                senderAddress: _configuration["EmailConfig:SenderAddress"],
                content: new EmailContent($"Your Verification Code is {request.Code}")
                {
                    PlainText = $@"
                        Your Verification Code

                        Hello,

                        Your verification code is: {request.Code}

                        Please enter this code to complete your verification. If you did not request this, please ignore this email.

                        Thank you,
                        The Ventixe Team
                    ",
                    Html = $@"
                        <!DOCTYPE html>
                        <html>
                          <body style=""font-family: Arial, sans-serif; color: #222;"">
                            <h2>Your Verification Code</h2>
                            <p>Hello,</p>
                            <p>
                              Your verification code is:
                              <span style=""font-size: 1.5em; font-weight: bold; color: #2d7ff9;"">{request.Code}</span>
                            </p>
                            <p>
                              Please enter this code to complete your verification.<br>
                              If you did not request this, please ignore this email.
                            </p>
                            <p>Thank you,<br>The Ventixe Team</p>
                          </body>
                        </html>
                    "
                },
                recipients: new EmailRecipients(new
                List<EmailAddress>
                {
                    new EmailAddress(request.Email)
                }));
            EmailSendOperation emailSendOperation = await emailClient.SendAsync(WaitUntil.Completed, emailMessage);
            return new EmailServiceResponse
            {
                Succeeded = true,
                Message = "Verification Code was sent successfully",
            };

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Something went wrong: {ex}");
            return new EmailServiceResponse
            {
                Succeeded = false,
                Error = $"Something went wrong when trying to send verification code to email: {request.Email}"
            };
        }
    }
}
