using FantasyCritic.Lib.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RoyaleController
    {
        private readonly RoyaleService _royaleService;
        private readonly IClock _clock;
        private readonly ILogger<RoyaleController> _logger;

        public RoyaleController(RoyaleService royaleService, IClock clock, ILogger<RoyaleController> logger)
        {
            _royaleService = royaleService;
            _clock = clock;
            _logger = logger;
        }
    }
}
