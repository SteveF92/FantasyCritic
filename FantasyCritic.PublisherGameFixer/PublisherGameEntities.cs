using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.PublisherGameFixer
{
    public record PublisherGameBidAmountUpdateEntity(Guid PublisherGameID, uint BidAmount);
    public record PublisherGameDraftPositionUpdateEntity(Guid PublisherGameID, int OverallDraftPosition, int DraftPosition);
}
