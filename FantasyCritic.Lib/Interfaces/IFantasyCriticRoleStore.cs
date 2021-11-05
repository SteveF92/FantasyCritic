using System;
using System.Collections.Generic;
using System.Text;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IFantasyCriticRoleStore : IRoleStore<FantasyCriticRole>
    {
    }
}
