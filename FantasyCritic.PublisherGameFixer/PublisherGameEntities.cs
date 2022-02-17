using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.PublisherGameFixer
{
    public record PublisherGameBidAmountUpdateEntity(Guid PublisherGameID, uint BidAmount);
    public record PublisherGameDraftPositionUpdateEntity(Guid PublisherGameID, int OverallDraftPosition, int DraftPosition);
    public record FormerPublisherGameBidAmountUpdateEntity(Guid PublisherGameID, Instant Timestamp, uint BidAmount);
    public record FormerPublisherGameDraftPositionUpdateEntity(Guid PublisherGameID, Instant Timestamp, int OverallDraftPosition, int DraftPosition);
}
