namespace FantasyCritic.MySQL.Entities;

internal class DraftPickSkipEntity
{
    public Guid DraftID { get; set; }
    public Guid PublisherID { get; set; }
    public bool CounterPick { get; set; }
    public int PickNumber { get; set; }
    public bool IsManualSkip { get; set; }

    public PublisherDraftPickSkip ToDomain() => new PublisherDraftPickSkip(CounterPick, PickNumber, IsManualSkip);
}
