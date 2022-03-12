using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    public class SystemWideSettingsEntity
    {
        public bool ActionProcessingMode { get; set; }
        public bool RefreshOpenCritic { get; set; }

        public SystemWideSettings ToDomain()
        {
            return new SystemWideSettings(ActionProcessingMode, RefreshOpenCritic);
        }
    }
}
