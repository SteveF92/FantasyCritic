using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Identity;

public class FantasyCriticRoleManager : RoleManager<FantasyCriticRole>
{
    public FantasyCriticRoleManager(IRoleStore<FantasyCriticRole> store, IEnumerable<IRoleValidator<FantasyCriticRole>> roleValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<FantasyCriticRole>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
    {
    }
}
