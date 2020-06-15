using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Web.Extensions
{
    public static class URLParsingExtensions
    {
        public static int? GetOpenCriticIDFromURL(string openCriticLink)
        {
            int? openCriticID = null;
            var openCriticGameIDString = SubstringSearching.GetBetween(openCriticLink, "/game/", "/");
            if (openCriticGameIDString.IsSuccess)
            {
                bool parseResult = int.TryParse(openCriticGameIDString.Value, out int openCriticIDResult);
                if (parseResult)
                {
                    openCriticID = openCriticIDResult;
                }
            }

            return openCriticID;
        }
    }
}
