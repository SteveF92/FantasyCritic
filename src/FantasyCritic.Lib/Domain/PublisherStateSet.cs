namespace FantasyCritic.Lib.Domain;
public class PublisherStateSet
{
    private readonly Dictionary<Guid, Publisher> _publisherDictionary;
    private ILookup<LeagueYearKey, Publisher> _leagueYearLookup;

    public PublisherStateSet(IEnumerable<Publisher> publishers)
    {
        _publisherDictionary = publishers.ToDictionary(x => x.PublisherID);
        _leagueYearLookup = _publisherDictionary.Values.ToLookup(x => x.LeagueYear.Key);
    }

    public IReadOnlyList<Publisher> Publishers => _publisherDictionary.Values.ToList();

    public IReadOnlyList<Publisher> GetPublishersInLeagueYear(LeagueYearKey key) => _leagueYearLookup[key].ToList();

    public Publisher GetPublisher(Guid publisherID)
    {
        var hasPublisher = _publisherDictionary.TryGetValue(publisherID, out var publisher);
        if (!hasPublisher)
        {
            throw new Exception($"Cannot find publisher: {publisherID}");
        }

        return publisher;
    }

    public void AcquireGameForPublisher(Publisher publisherToEdit, PublisherGame game, uint bidAmount)
    {
        UpdatePublisher(publisherToEdit, game, Maybe<PublisherGame>.None, (int)bidAmount * -1, null);
    }

    public void SpendBudgetForPublisher(Publisher publisherToEdit, uint budget)
    {
        UpdatePublisher(publisherToEdit, Maybe<PublisherGame>.None, Maybe<PublisherGame>.None, (int)budget * -1, null);
    }

    public void ObtainBudgetForPublisher(Publisher publisherToEdit, uint budget)
    {
        UpdatePublisher(publisherToEdit, Maybe<PublisherGame>.None, Maybe<PublisherGame>.None, (int)budget, null);
    }

    public void DropGameForPublisher(Publisher publisherToEdit, PublisherGame publisherGame)
    {
        var leagueOptions = publisherToEdit.LeagueYear.Options;
        if (publisherGame.WillRelease())
        {
            if (leagueOptions.WillReleaseDroppableGames == -1 || leagueOptions.WillReleaseDroppableGames > publisherToEdit.WillReleaseGamesDropped)
            {
                UpdatePublisher(publisherToEdit, Maybe<PublisherGame>.None, publisherGame, 0, DropType.WillRelease);
            }
            if (leagueOptions.FreeDroppableGames == -1 || leagueOptions.FreeDroppableGames > publisherToEdit.FreeGamesDropped)
            {
                UpdatePublisher(publisherToEdit, Maybe<PublisherGame>.None, publisherGame, 0, DropType.Free);
            }
            throw new Exception("Publisher cannot drop any more 'Will Release' games");
        }

        if (leagueOptions.WillNotReleaseDroppableGames == -1 || leagueOptions.WillNotReleaseDroppableGames > publisherToEdit.WillNotReleaseGamesDropped)
        {
            UpdatePublisher(publisherToEdit, Maybe<PublisherGame>.None, publisherGame, 0, DropType.WillNotRelease);
        }
        if (leagueOptions.FreeDroppableGames == -1 || leagueOptions.FreeDroppableGames > publisherToEdit.FreeGamesDropped)
        {
            UpdatePublisher(publisherToEdit, Maybe<PublisherGame>.None, publisherGame, 0, DropType.Free);
        }
        throw new Exception("Publisher cannot drop any more 'Will Not Release' games");
    }

    private void UpdatePublisher(Publisher publisherToEdit, Maybe<PublisherGame> addGame, Maybe<PublisherGame> removeGame, int budgetChange, DropType? dropType)
    {
        var updatedPublisher = GetUpdatedPublisher(publisherToEdit, addGame, removeGame, budgetChange, dropType);
        _publisherDictionary[updatedPublisher.PublisherID] = updatedPublisher;
        _leagueYearLookup = _publisherDictionary.Values.ToLookup(x => x.LeagueYear.Key);
    }

    private static Publisher GetUpdatedPublisher(Publisher publisherToEdit, Maybe<PublisherGame> addGame, Maybe<PublisherGame> removeGame, int budgetChange, DropType? dropType)
    {
        var newPublisherGames = publisherToEdit.PublisherGames.ToList();
        if (addGame.HasValue)
        {
            newPublisherGames = newPublisherGames.Concat(new[] { addGame.Value }).ToList();
        }
        if (removeGame.HasValue)
        {
            newPublisherGames = newPublisherGames.Where(x => x.PublisherGameID != removeGame.Value.PublisherGameID).ToList();
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

        return new Publisher(publisherToEdit.PublisherID, publisherToEdit.LeagueYear, publisherToEdit.User, publisherToEdit.PublisherName, publisherToEdit.PublisherIcon, publisherToEdit.DraftPosition,
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
