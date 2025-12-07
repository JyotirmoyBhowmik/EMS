using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AlarmManagement.Services;

public class SMSNotificationService
{
    private readonly ILogger<SMSNotificationService> _logger;
    private readonly string? _accountSid;
    private readonly string? _authToken;
    private readonly string? _fromNumber;

    public SMSNotificationService(ILogger<SMSNotificationService> logger)
    {
        _logger = logger;
        _accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        _authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
        _fromNumber = Environment.GetEnvironmentVariable("TWILIO_FROM_NUMBER");

        if (!string.IsNullOrEmpty(_accountSid) && !string.IsNullOrEmpty(_authToken))
        {
            TwilioClient.Init(_accountSid, _authToken);
        }
    }

    public async Task SendAlarmSMSAsync(string toPhoneNumber, string message)
    {
        if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken) || string.IsNullOrEmpty(_fromNumber))
        {
            _logger.LogWarning("Twilio credentials not configured, skipping SMS");
            return;
        }

        try
        {
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromNumber),
                to: new PhoneNumber(toPhoneNumber)
            );

            _logger.LogInformation("Sent SMS notification, SID: {Sid}", messageResource.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS notification");
        }
    }
}
