using FantasyCritic.Lib.Interfaces;
using PostmarkDotNet;
using Serilog;

namespace FantasyCritic.Postmark;

public class PostmarkEmailSender : IEmailSender
{
    private static readonly ILogger _logger = Log.ForContext<PostmarkEmailSender>();
    
    private readonly string _apiKey;
    private readonly string _fromEmail;

    public PostmarkEmailSender(string apiKey, string fromEmail)
    {
        _apiKey = apiKey;
        _fromEmail = fromEmail;
    }
    
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var postmarkMessage = new PostmarkMessage()
        {
            To = email,
            From = _fromEmail,
            TrackOpens = false,
            Subject = subject,
            HtmlBody = message
        };

        try
        {
            var client = new PostmarkClient(_apiKey);
            var sendResult = await client.SendMessageAsync(postmarkMessage);

            if (sendResult.Status != PostmarkStatus.Success)
            {
                _logger.Error($"Mail failed to send to {email}: {sendResult.ErrorCode}");
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, $"Mail failed to send to {email}");
        }
    }
}
