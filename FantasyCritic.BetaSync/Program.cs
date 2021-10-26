using System;
using System.Threading.Tasks;
using System.Configuration;

namespace FantasyCritic.BetaSync
{
    class Program
    {
        private static string _productionReadOnlyConnectionString;
        private static string _betaConnectionString;

        static Task Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new Exception("Unknown mode. Pass --clean or --sync.");
            }

            _productionReadOnlyConnectionString = ConfigurationManager.AppSettings["productionConnectionString"];
            _betaConnectionString = ConfigurationManager.AppSettings["betaConnectionString"];

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
