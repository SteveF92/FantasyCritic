using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Discord.Models;
public record NewMasterGameMessage(MasterGame MasterGame, int Year);
public record GameCriticScoreUpdateMessage(MasterGame Game, decimal? OldCriticScore, decimal? NewCriticScore, int Year);
public record MasterGameEditMessage(MasterGameYear ExistingGame, MasterGameYear EditedGame, IReadOnlyList<string> Changes);
