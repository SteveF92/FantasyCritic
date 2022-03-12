namespace FantasyCritic.Lib.Interfaces
{
    public interface ISMSSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
