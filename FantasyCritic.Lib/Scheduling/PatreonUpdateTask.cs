﻿using System;
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
    public class PatreonUpdateTask : IScheduledTask
    {
        private readonly IServiceProvider _serviceProvider;
        public string Schedule => "0 */1 * * *";

        public PatreonUpdateTask(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
#if DEBUG
            Console.WriteLine("Not updating Patreon - DEBUG version");
            return;
#endif
            var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var adminService = scope.ServiceProvider.GetRequiredService<AdminService>();
                await adminService.UpdatePatreonRoles();
            }
        }
    }
}
