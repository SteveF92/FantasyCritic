using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Services
{
    public class SMSSender : ISMSSender
    {
        public Task SendSmsAsync(string number, string message)
        {
            throw new NotImplementedException();
        }
    }
}
