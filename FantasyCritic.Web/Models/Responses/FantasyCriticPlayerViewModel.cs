using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class FantasyCriticPlayerViewModel
    {
        public FantasyCriticPlayerViewModel(FantasyCriticUser user)
        {
            UserName = user.UserName;
        }

        public string UserName { get; }
    }
}
