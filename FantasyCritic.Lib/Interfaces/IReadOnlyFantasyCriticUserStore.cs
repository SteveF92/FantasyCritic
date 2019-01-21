using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IReadOnlyFantasyCriticUserStore
    {
        Task<FantasyCriticUser> FindByIdAsync(string userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<FantasyCriticUser>> GetAllUsers();
    }
}
