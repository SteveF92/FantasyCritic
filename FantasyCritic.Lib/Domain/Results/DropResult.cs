using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Domain.Results
{
    public class DropResult
    {
        public DropResult(Result result, bool willNotRelease)
        {
            Result = result;
            WillNotRelease = willNotRelease;
        }

        public Result Result { get; }
        public bool WillNotRelease { get; }
    }
}
