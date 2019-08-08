using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.XPath;
using CsvHelper;
using FantasyCritic.Lib.Domain;
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

            var outputModels = gamesToOutput.Select(x => new MasterGameYearRInput(x));

            string fileName = Guid.NewGuid() + ".csv";
            using (var writer = new StreamWriter(fileName))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(outputModels);
            }

            var args_r = new string[1] { fileName };
            engine.SetCommandLineArguments(args_r);

            CharacterVector vector = engine.Evaluate(rscript).AsCharacter();
            string result = vector[0];
            Console.WriteLine(result);

            var splitString = result.Split(' ');

            double baseScore = double.Parse(splitString[2]);
            double counterPickConstant = double.Parse(splitString[4]);
            double bidPercentileConstant = double.Parse(splitString[8]);
            double hypeFactorConstant = double.Parse(splitString[12]);

            HypeConstants hypeConstants = new HypeConstants(baseScore, counterPickConstant, bidPercentileConstant, hypeFactorConstant);

            File.Delete(fileName);
        }
    }
}
