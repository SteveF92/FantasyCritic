using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IFantasyCriticFileRepository
    {
        Task UploadMasterGameYearStats(MemoryStream stream);
    }
}
