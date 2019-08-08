using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.XPath;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL;
using NodaTime;
using RDotNet;

namespace FantasyCritic.Stats
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            REngine.SetEnvironmentVariables();
            var engine = REngine.GetInstance();
            string rscript = File.ReadAllText("critic_score_4.R");

            string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            IMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(connectionString, new MySQLFantasyCriticUserStore(connectionString, SystemClock.Instance));

            var masterGames = await masterGameRepo.GetMasterGameYears(2019, true);

            var gamesToOutput = masterGames
                .Where(x => x.Year == 2019)
                .Where(x => x.DateAdjustedHypeFactor > 0)
                .Where(x => !x.WillRelease() || x.MasterGame.CriticScore.HasValue);

            var outputModels = masterGames.Select(x => new MasterGameYearRInput(x));

            var args_r = new string[1] { "CleanMasterGames2.csv" };
            engine.SetCommandLineArguments(args_r);

            CharacterVector vector = engine.Evaluate(rscript).AsCharacter();
            string result = vector[0];
            Console.WriteLine(result);
        }
    }
}
