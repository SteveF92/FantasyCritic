using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Controllers.API;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace FantasyCritic.Web.Controllers
{
    public abstract class FantasyCriticController : ControllerBase
    {
        private readonly FantasyCriticUserManager _userManager;

        protected FantasyCriticController(FantasyCriticUserManager userManager)
        {
            _userManager = userManager;
        }

        protected async Task<Result<FantasyCriticUser>> GetCurrentUser()
        {
            var userID = User.Claims.SingleOrDefault(x => x.Type == "sub")?.Value;
            if (userID is null)
            {
                return Result.Failure<FantasyCriticUser>("Can't get User ID");
            }

            var currentUser = await _userManager.FindByIdAsync(userID);
            if (currentUser is null)
            {
                return Result.Failure<FantasyCriticUser>("User does not exist.");
            }

            return Result.Success(currentUser);
        }
    }
}
