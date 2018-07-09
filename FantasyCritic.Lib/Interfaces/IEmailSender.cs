using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
