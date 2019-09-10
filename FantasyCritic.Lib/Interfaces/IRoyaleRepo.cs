using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IRoyaleRepo
    {
        Task CreatePublisher(RoyalePublisher publisher);
        Task<Maybe<RoyalePublisher>> GetPublisher(RoyaleYearQuarter yearQuarter, FantasyCriticUser user);
    }
}
