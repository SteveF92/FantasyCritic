using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Utilities
{
    public class SubstringSearching
    {
        public static Result<string> GetBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                var start = strSource.IndexOf(strStart, 0) + strStart.Length;
                var end = strSource.IndexOf(strEnd, start);
                return Result.Ok(strSource.Substring(start, end - start));
            }

            return Result.Fail<string>("Can't parse string");
        }
    }
}
