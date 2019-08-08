using System;
using System.ComponentModel.DataAnnotations;
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
            var rscriptData = Resources.critic_score_4;
            string rscript = System.Text.Encoding.UTF8.GetString(rscriptData);
            CharacterVector vector = engine.Evaluate(rscript).AsCharacter();
            string result = vector[0];
            Console.WriteLine(result);
        }
    }
}
