using System;
using System.Collections.Generic;
using System.Text;
using FantasyCritic.Lib.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Services
{
    public class FantasyCriticService
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly IFantasyCriticRepo _fantasyCriticRepo;

        public FantasyCriticService(FantasyCriticUserManager userManager, IFantasyCriticRepo fantasyCriticRepo)
        {
            _userManager = userManager;
            _fantasyCriticRepo = fantasyCriticRepo;
        }
    }
}
