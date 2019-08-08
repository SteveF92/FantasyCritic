using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Xml.XPath;
using RDotNet;

namespace FantasyCritic.Stats
{
    class Program
    {
        static void Main(string[] args)
        {
            REngine.SetEnvironmentVariables();
            var engine = REngine.GetInstance();
            string rscript = File.ReadAllText("critic_score_4.R");

            var args_r = new string[1] { "CleanMasterGames2.csv" };
            engine.SetCommandLineArguments(args_r);

            CharacterVector vector = engine.Evaluate(rscript).AsCharacter();
            string result = vector[0];
            Console.WriteLine(result);
        }
    }
}
