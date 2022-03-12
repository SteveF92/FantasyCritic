using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IFantasyCriticRoleStore : IRoleStore<FantasyCriticRole>
    {
    }
}
