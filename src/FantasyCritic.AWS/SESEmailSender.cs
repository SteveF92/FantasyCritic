using FantasyCritic.Lib.Interfaces;
using System.Threading.Tasks;
using Amazon.SimpleEmailV2;
using Amazon;
using Amazon.SimpleEmailV2.Model;
using System.Collections.Generic;

namespace FantasyCritic.AWS;
public class SESEmailSender : IEmailSender
{
    private readonly string _region;
    private readonly string _fromEmail;
    
    public SESEmailSender(string region, string fromEmail)
    {
        _region = region;
        _fromEmail = fromEmail;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        using var client = new AmazonSimpleEmailServiceV2Client(RegionEndpoint.GetBySystemName(_region));
        var request = new SendEmailRequest()
        {
            FromEmailAddress = _fromEmail,
            Destination = new Destination()
            {
                ToAddresses = new List<string>() { email }
            },
            Content = new EmailContent()
            {
                Simple = new Message()
                {
                    Subject = new Content()
                    {
                        Data = subject
                    },
                    Body = new Body()
                    {
                        Html = new Content()
                        {
                            Data = htmlMessage
                        }
                    }
                }
            }
        };
        
        await client.SendEmailAsync(request);
    }
}
