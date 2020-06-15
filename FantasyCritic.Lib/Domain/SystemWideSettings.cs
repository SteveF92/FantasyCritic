using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class SystemWideSettings
    {
        public SystemWideSettings(bool bidProcessingMode)
        {
            BidProcessingMode = bidProcessingMode;
        }

        public bool BidProcessingMode { get; }
    }
}
