using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Scheduling
{
    public class RefreshDataTask : IScheduledTask
    {
        private readonly AdminService _adminService;
        //public string Schedule => "0 */2 * * *";
        public string Schedule => "*/2 * * * *";

        public RefreshDataTask(AdminService adminService)
        {
            _adminService = adminService;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _adminService.UpdateFantasyPoints();
        }
    }
}
