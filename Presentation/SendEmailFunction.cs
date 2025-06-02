using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Presentation.Models;
using Presentation.Services;

namespace Presentation;

public class SendVerificationEmailFunction(ILogger<SendVerificationEmailFunction> logger, IEmailService emailService)
{
    private readonly ILogger<SendVerificationEmailFunction> _logger = logger;
    private readonly IEmailService _emailService = emailService;

    [Function(nameof(SendVerificationEmailFunction))]
    public async Task Run(
        [ServiceBusTrigger("emailqueue", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Processing message with ID: {MessageId}", message.MessageId);

        var messageBody = message.Body.ToString();
        var verificationData = JsonSerializer.Deserialize<SendVerificationCodeDto>(messageBody);

        if (verificationData == null)
        {
            _logger.LogError("Failed to deserialize message body: {MessageBody}", messageBody);
            // The problem is that DeadLetterMessageAsync expects the second argument to be a Dictionary<string, object>? for application properties, not a string reason.
            // To specify a dead-letter reason and description, use the overload:
            // DeadLetterMessageAsync(ServiceBusReceivedMessage, string? deadLetterReason, string? deadLetterErrorDescription, CancellationToken)
            // Example fix:
            await messageActions.DeadLetterMessageAsync(message,
                deadLetterReason: "DeserializationError",
                deadLetterErrorDescription: "Failed to deserialize the message body into SendVerificationCodeDto."
            );
            return;
        }

        var result = await _emailService.SendVerificationEmailAsync(verificationData);

        if (result.Succeeded){
            _logger.LogInformation("Email sent successfully to {Email}", verificationData.Email);
            await messageActions.CompleteMessageAsync(message);
        }
        else
        {
            _logger.LogError("Failed to send email to {Email}. Error: {Error}", verificationData.Email, result.Error);
            // Dead-letter the message if email sending fails
            await messageActions.DeadLetterMessageAsync(message,
                deadLetterReason: "EmailSendingFailed",
                deadLetterErrorDescription: result.Error ?? "Unknown error occurred while sending email."
            );
            return;
            
        }

    }
}