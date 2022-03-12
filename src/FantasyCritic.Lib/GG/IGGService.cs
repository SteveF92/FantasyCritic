using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.GG
{
    public interface IGGService
    {
        Task<Maybe<GGGame>> GetGGGame(string ggToken);
    }
}
