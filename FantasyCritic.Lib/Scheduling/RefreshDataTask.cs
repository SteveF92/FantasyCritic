using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyCritic.Lib.Scheduling
{
    public class RefreshDataTask : IScheduledTask
    {
        private readonly IServiceProvider _serviceProvider;
        public string Schedule => "0 */2 * * *";

        public RefreshDataTask(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var adminService = scope.ServiceProvider.GetRequiredService<AdminService>();
                await adminService.RefreshCriticInfo();
            }
        }
    }
}
