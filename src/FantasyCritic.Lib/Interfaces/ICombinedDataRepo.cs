using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Interfaces;

public interface ICombinedDataRepo
{
    Task<BasicData> GetBasicData();
    Task<HomePageData> GetHomePageData(FantasyCriticUser currentUser);
}
