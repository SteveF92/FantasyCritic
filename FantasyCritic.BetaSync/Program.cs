using System;
using System.Threading.Tasks;

namespace FantasyCritic.BetaSync
{
    class Program
    {
        static Task Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new Exception("Unknown mode. Pass --clean or --sync.");
            }

            if (args[0] == "--clean")
            {
                return CleanBetaServer();
            }
            if (args[0] == "--sync")
            {
                return SyncProductionAndBeta();
            }

            throw new Exception("Unknown mode. Pass --clean or --sync.");
        }

        static async Task CleanBetaServer()
        {
            return;
        }

        static async Task SyncProductionAndBeta()
        {
            return;
        }
    }
}
