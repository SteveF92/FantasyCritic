using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Interfaces
{
    public interface ISMSSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
