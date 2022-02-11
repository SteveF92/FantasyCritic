using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.PublisherGameFixer
{
    public record PublisherGameBidAmountUpdateEntity(Guid PublisherGameID, uint? BidAmount);
    public record PublisherGameOverallDraftPositionUpdateEntity(Guid PublisherGameID, int? OverallDraftPosition);
    public record PublisherGameDraftPositionUpdateEntity(Guid PublisherGameID, int? DraftPosition);
    public record PublisherGameCombinedUpdateEntity(Guid PublisherGameID, uint? BidAmount, int? OverallDraftPosition, int? DraftPosition);
}
