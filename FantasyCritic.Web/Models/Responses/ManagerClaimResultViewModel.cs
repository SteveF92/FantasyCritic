using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class ManagerClaimResultViewModel
    {
        public ManagerClaimResultViewModel(ClaimResult domain)
        {
            Success = domain.Success;
            Error = domain.Error;
            Overridable = domain.Overridable;
        }

        public bool Success { get; }
        public string Error { get; }
        public bool Overridable { get; }
    }
}
