using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class PublisherGame
    {
        public PublisherGame(Guid publisherID, Guid publisherGameID, string gameName, Instant timestamp, bool counterPick, decimal? manualCriticScore, bool manualWillNotRelease,
            decimal? fantasyPoints, Maybe<MasterGameYear> masterGame, int? draftPosition, int? overallDraftPosition, bool currentlyIneligible)
        {
            PublisherID = publisherID;
            PublisherGameID = publisherGameID;
            GameName = gameName;
            Timestamp = timestamp;
            CounterPick = counterPick;
            ManualCriticScore = manualCriticScore;
            ManualWillNotRelease = manualWillNotRelease;
            FantasyPoints = fantasyPoints;
            MasterGame = masterGame;
            DraftPosition = draftPosition;
            OverallDraftPosition = overallDraftPosition;
            CurrentlyIneligible = currentlyIneligible;
        }

        public Guid PublisherID { get; }
        public Guid PublisherGameID { get; }
        public string GameName { get; }
        public Instant Timestamp { get; }
        public bool CounterPick { get; }
        public decimal? ManualCriticScore { get; }
        public bool ManualWillNotRelease { get; }
        public decimal? FantasyPoints { get; }
        public Maybe<MasterGameYear> MasterGame { get; }
        public int? DraftPosition { get; }
        public int? OverallDraftPosition { get; }
        public bool CurrentlyIneligible { get; }

        public bool WillRelease()
        {
            if (ManualWillNotRelease)
            {
                return false;
            }

            if (MasterGame.HasNoValue)
            {
                return false;
            }

            return MasterGame.Value.WillRelease();
        }

        public decimal GetProjectedOrRealFantasyPoints(ScoringSystem scoringSystem, SystemWideValues systemWideValues, bool simpleProjections, LocalDate currentDate)
        {
            if (MasterGame.HasNoValue)
            {
                return systemWideValues.GetAveragePoints(CounterPick);
            }

            decimal? fantasyPoints = CalculateFantasyPoints(scoringSystem, currentDate);
            if (fantasyPoints.HasValue)
            {
                return fantasyPoints.Value;
            }

            if (simpleProjections)
            {
                return MasterGame.Value.GetSimpleProjectedFantasyPoints(systemWideValues, CounterPick);
            }

            return MasterGame.Value.GetProjectedOrRealFantasyPoints(scoringSystem, CounterPick, currentDate);
        }

        public decimal? CalculateFantasyPoints(ScoringSystem scoringSystem, LocalDate currentDate)
        {
            if (ManualCriticScore.HasValue)
            {
                return scoringSystem.GetPointsForScore(ManualCriticScore.Value, CounterPick); 
            }
            if (MasterGame.HasNoValue)
            {
                return null;
            }

            return MasterGame.Value.CalculateFantasyPoints(scoringSystem, CounterPick, currentDate, true);
        }

        public bool CalculateIsCurrentlyIneligible(LeagueOptions options, bool? overridenEligibility)
        {
            if (overridenEligibility.HasValue)
            {
                return !overridenEligibility.Value;
            }

            if (MasterGame.HasNoValue)
            {
                return false;
            }

            var leagueTags = options.LeagueTags;
            var customCodeTags = leagueTags.Where(x => x.Tag.HasCustomCode).ToList();
            var nonCustomCodeTags = leagueTags.Except(customCodeTags).ToList();

            var masterGame = MasterGame.Value.MasterGame;
            var bannedTags = nonCustomCodeTags.Where(x => x.Status == TagStatus.Banned).Select(x => x.Tag);
            var requiredTags = nonCustomCodeTags.Where(x => x.Status == TagStatus.Required).Select(x => x.Tag);

            var bannedTagsIntersection = masterGame.Tags.Intersect(bannedTags);
            var missingRequiredTags = requiredTags.Except(masterGame.Tags);

            if (bannedTagsIntersection.Any() || missingRequiredTags.Any())
            {
                return true;
            }

            var masterGameCustomCodeTags = masterGame.Tags.Where(x => x.HasCustomCode).ToList();
            if (!masterGameCustomCodeTags.Any())
            {
                return false;
            }

            var dateGameWasAcquired = Timestamp.InZone(TimeExtensions.EasternTimeZone).Date;
            if (masterGame.EarlyAccessReleaseDate.HasValue)
            {
                var plannedForEarlyAccessTag = customCodeTags.SingleOrDefault(x => x.Tag.Name == "PlannedForEarlyAccess");
                if (plannedForEarlyAccessTag is not null)
                {
                    if (plannedForEarlyAccessTag.Status == TagStatus.Banned)
                    {
                        return true;
                    }
                }

                var currentlyInEarlyAccessTag = customCodeTags.SingleOrDefault(x => x.Tag.Name == "CurrentlyInEarlyAccess");
                if (currentlyInEarlyAccessTag is not null)
                {
                    if (currentlyInEarlyAccessTag.Status == TagStatus.Banned)
                    {
                        var pickedUpBeforeInEarlyAccess = dateGameWasAcquired < masterGame.EarlyAccessReleaseDate.Value;
                        if (!pickedUpBeforeInEarlyAccess)
                        {
                            return true;
                        }
                    }
                }
            }

            if (masterGame.InternationalReleaseDate.HasValue)
            {
                var willReleaseInternationallyFirstTag = customCodeTags.SingleOrDefault(x => x.Tag.Name == "WillReleaseInternationallyFirst");
                if (willReleaseInternationallyFirstTag is not null)
                {
                    if (willReleaseInternationallyFirstTag.Status == TagStatus.Banned)
                    {
                        return true;
                    }
                }

                var releasedInternationallyTag = customCodeTags.SingleOrDefault(x => x.Tag.Name == "ReleasedInternationally");
                if (releasedInternationallyTag is not null)
                {
                    if (releasedInternationallyTag.Status == TagStatus.Banned)
                    {
                        var pickedUpBeforeReleasedInternationally = dateGameWasAcquired < masterGame.InternationalReleaseDate.Value;
                        if (!pickedUpBeforeReleasedInternationally)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override string ToString() => GameName;
    }
}
