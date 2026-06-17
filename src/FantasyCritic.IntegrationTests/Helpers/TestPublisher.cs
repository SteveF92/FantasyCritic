using System;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// A test-harness binding between an authenticated session and a publisher in draft order.
/// Not the domain roster-slot type.
/// </summary>

public sealed class TestPublisher
{
    public TestPublisher(int draftPosition, ApiSession session, Guid publisherID, string publisherName)
    {
        DraftPosition = draftPosition;
        Session = session;
        PublisherID = publisherID;
        PublisherName = publisherName;
        StartingBudget = 100;
    }

    public int DraftPosition { get; }
    public ApiSession Session { get; }
    public Guid PublisherID { get; }
    public string PublisherName { get; }
    public int StartingBudget { get; set; }
    public int WillReleaseDroppableBefore { get; set; }
}
