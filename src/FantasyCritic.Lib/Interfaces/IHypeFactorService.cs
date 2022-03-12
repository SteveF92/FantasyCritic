using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IHypeFactorService
    {
        Task<HypeConstants> GetHypeConstants(IEnumerable<MasterGameYear> allMasterGameYears);
    }
}
