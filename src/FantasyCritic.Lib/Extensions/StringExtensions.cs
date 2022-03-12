using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace FantasyCritic.Lib.Extensions
{
    public static class StringExtensions
    {
        public static string TrimStart(this string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString)) return target;

            string result = target;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }

        public static string CamelCaseToSpaces(this string value)
        {
            return Regex.Replace(value, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }

        public static string TrimStartingFromFirstInstance(this string source, string startFrom)
        {
            int index = source.IndexOf(startFrom);
            if (index > 0)
            {
                source = source.Substring(0, index);
            }

            return source;
        }
    }
}
