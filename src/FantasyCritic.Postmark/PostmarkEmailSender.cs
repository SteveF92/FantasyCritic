using FantasyCritic.Lib.Interfaces;
using PostmarkDotNet;

namespace FantasyCritic.Postmark;

public class PostmarkEmailSender : IEmailSender
{
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

        var client = new PostmarkClient(_apiKey);
        var sendResult = await client.SendMessageAsync(postmarkMessage);

        if (sendResult.Status != PostmarkStatus.Success)
        {
            throw new Exception($"Mail failed to send: {sendResult.ErrorCode}");
        }
    }
}
