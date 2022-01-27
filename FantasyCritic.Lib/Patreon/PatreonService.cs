using FantasyCritic.Lib.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Patreon
{
    public class PatreonService : IPatreonService
    {
        private readonly HttpClient _client;
        private readonly ILogger<PatreonService> _logger;

        public PatreonService(HttpClient client, ILogger<PatreonService> logger)
        {
            _client = client;
            _logger = logger;
        }

        Task<IReadOnlyList<PatronInfo>> IPatreonService.GetPatronInfo(IReadOnlyList<FantasyCriticUserWithExternalLogins> patreonUsers)
        {
            throw new NotImplementedException();
        }
    }
}
