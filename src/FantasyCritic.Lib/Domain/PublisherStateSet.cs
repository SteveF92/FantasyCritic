namespace FantasyCritic.Lib.Domain;
public class PublisherStateSet
{
    private readonly Dictionary<Guid, Publisher> _publisherDictionary;
    private ILookup<LeagueYearKey, Publisher> _leagueLookup;

    public PublisherStateSet(IEnumerable<Publisher> publishers)
    {
        _publisherDictionary = publishers.ToDictionary(x => x.PublisherID);
        _leagueLookup = publishers.ToLookup(x => x.LeagueYearKey);
    }

    public IReadOnlyList<Publisher> Publishers => _publisherDictionary.Values.ToList();

    public Publisher GetPublisher(Guid publisherID)
    {
        var hasPublisher = _publisherDictionary.TryGetValue(publisherID, out var publisher);
        if (!hasPublisher)
        {
            throw new Exception($"Cannot find publisher: {publisherID}");
        }

        return publisher;
    }

    public LeagueYear GetUpdatedLeagueYear(LeagueYear leagueYear)
    {
        var updatedPublishersInLeague = _leagueLookup[leagueYear.Key];
        return new LeagueYear(leagueYear.League, leagueYear.SupportedYear, leagueYear.Options, leagueYear.PlayStatus,
            leagueYear.EligibilityOverrides,
            leagueYear.TagOverrides, leagueYear.DraftStartedTimestamp, leagueYear.WinningUser,
            updatedPublishersInLeague);
    }

    public void AcquireGameForPublisher(Publisher publisherToEdit, PublisherGame game, uint bidAmount)
    {
        UpdatePublisher(publisherToEdit, game, null, (int)bidAmount * -1, null);
    }

    public void SpendBudgetForPublisher(Publisher publisherToEdit, uint budget)
    {
        UpdatePublisher(publisherToEdit, null, null, (int)budget * -1, null);
    }

    public void ObtainBudgetForPublisher(Publisher publisherToEdit, uint budget)
    {
        UpdatePublisher(publisherToEdit, null, null, (int)budget, null);
    }

    public void DropGameForPublisher(Publisher publisherToEdit, PublisherGame publisherGame, LeagueOptions leagueOptions)
    {
        if (publisherGame.WillRelease())
        {
            if (leagueOptions.WillReleaseDroppableGames == -1 || leagueOptions.WillReleaseDroppableGames > publisherToEdit.WillReleaseGamesDropped)
            {
                UpdatePublisher(publisherToEdit, null, publisherGame, 0, DropType.WillRelease);
                return;
            }
            if (leagueOptions.FreeDroppableGames == -1 || leagueOptions.FreeDroppableGames > publisherToEdit.FreeGamesDropped)
            {
                UpdatePublisher(publisherToEdit, null, publisherGame, 0, DropType.Free);
                return;
            }
            throw new Exception("Publisher cannot drop any more 'Will Release' games");
        }

        if (leagueOptions.WillNotReleaseDroppableGames == -1 || leagueOptions.WillNotReleaseDroppableGames > publisherToEdit.WillNotReleaseGamesDropped)
        {
            UpdatePublisher(publisherToEdit, null, publisherGame, 0, DropType.WillNotRelease);
            return;
        }
        if (leagueOptions.FreeDroppableGames == -1 || leagueOptions.FreeDroppableGames > publisherToEdit.FreeGamesDropped)
        {
            UpdatePublisher(publisherToEdit, null, publisherGame, 0, DropType.Free);
            return;
        }
        throw new Exception("Publisher cannot drop any more 'Will Not Release' games");
    }

    private void UpdatePublisher(Publisher publisherToEdit, PublisherGame? addGame, PublisherGame? removeGame, int budgetChange, DropType? dropType)
    {
        var updatedPublisher = GetUpdatedPublisher(publisherToEdit, addGame, removeGame, budgetChange, dropType);
        _publisherDictionary[updatedPublisher.PublisherID] = updatedPublisher;
        _leagueLookup = _publisherDictionary.Values.ToLookup(x => x.LeagueYearKey);
    }

    private static Publisher GetUpdatedPublisher(Publisher publisherToEdit, PublisherGame? addGame, PublisherGame? removeGame, int budgetChange, DropType? dropType)
    {
        var newPublisherGames = publisherToEdit.PublisherGames.ToList();
        if (addGame is not null)
        {
            newPublisherGames = newPublisherGames.Concat(new[] { addGame }).ToList();
        }
        if (removeGame is not null)
        {
            newPublisherGames = newPublisherGames.Where(x => x.PublisherGameID != removeGame.PublisherGameID).ToList();
        }

        uint newBudget = (uint)(publisherToEdit.Budget + budgetChange);

        int freeGamesDropped = publisherToEdit.FreeGamesDropped;
        int willNotReleaseGamesDropped = publisherToEdit.WillNotReleaseGamesDropped;
        int willReleaseGamesDropped = publisherToEdit.WillReleaseGamesDropped;

        switch (dropType)
        {
            case DropType.Free:
                freeGamesDropped++;
                break;
            case DropType.WillNotRelease:
                willNotReleaseGamesDropped++;
                break;
            case DropType.WillRelease:
                willReleaseGamesDropped++;
                break;
        }

        return new Publisher(publisherToEdit.PublisherID, publisherToEdit.LeagueYearKey, publisherToEdit.User, publisherToEdit.PublisherName, publisherToEdit.PublisherIcon, publisherToEdit.DraftPosition,
            newPublisherGames, publisherToEdit.FormerPublisherGames, newBudget, freeGamesDropped, willNotReleaseGamesDropped,
            willReleaseGamesDropped, publisherToEdit.AutoDraft);
    }

    private enum DropType
    {
        Free,
        WillNotRelease,
        WillRelease
    }
}
