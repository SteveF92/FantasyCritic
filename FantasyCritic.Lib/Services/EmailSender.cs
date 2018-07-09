using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            throw new NotImplementedException();
        }
    }
}
