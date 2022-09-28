using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Email.EmailModels;

public class PublicBidEmailModel
{
    public PublicBidEmailModel(FantasyCriticUser user, IReadOnlyList<EmailPublicBiddingSet> publicBiddingSets, string baseAddress, bool isProduction)
    {
        User = user;
        PublicBiddingSets = publicBiddingSets;
        BaseAddress = baseAddress;
        PublicBiddingSetsByYear = publicBiddingSets.GroupToDictionary(x => x.LeagueYear.Year);
        IsProduction = isProduction;
    }

    public FantasyCriticUser User { get; }
    public IReadOnlyList<EmailPublicBiddingSet> PublicBiddingSets { get; }
    public string BaseAddress { get; }
    public bool IsProduction { get; }

    public IReadOnlyDictionary<int, IReadOnlyList<EmailPublicBiddingSet>> PublicBiddingSetsByYear { get; }
    public bool ShowMultiYear => PublicBiddingSets.Count > 1;

    public string GetLeagueLink(LeagueYearKey key) => $"{BaseAddress}/league/{key.LeagueID}/{key.Year}";
    public string GetManageAccountLink() => $"{BaseAddress}/Account/Manage/Email";
}
