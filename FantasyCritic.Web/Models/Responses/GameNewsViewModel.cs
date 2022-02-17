using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Responses
{
    public record GameNewsViewModel(IReadOnlyList<SingleGameNewsViewModel> UpcomingGames, IReadOnlyList<SingleGameNewsViewModel> RecentGames);
}
