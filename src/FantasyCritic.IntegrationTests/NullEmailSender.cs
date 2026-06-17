using System.Threading.Tasks;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.IntegrationTests;

internal sealed class NullEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
        => Task.CompletedTask;
}
