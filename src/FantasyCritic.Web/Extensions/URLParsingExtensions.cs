using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Web.Extensions
{
    public static class URLParsingExtensions
    {
        public static int? GetOpenCriticIDFromURL(string openCriticLink)
        {
            if (string.IsNullOrWhiteSpace(openCriticLink))
            {
                return null;
            }

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

        public static Maybe<string> GetGGTokenFromURL(string ggLink)
        {
            if (string.IsNullOrWhiteSpace(ggLink))
            {
                return Maybe<string>.None;
            }
            var result = SubstringSearching.GetBetween(ggLink, "/games/", "/");
            if (result.IsFailure)
            {
                return Maybe<string>.None;
            }

            if (result.Value.Length != 6)
            {
                return Maybe<string>.None;
            }

            return result.Value;
        }
    }
}
