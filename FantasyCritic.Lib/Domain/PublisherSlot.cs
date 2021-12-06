using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class PublisherSlot
    {
        public PublisherSlot(int slotNumber, int overallSlotNumber, bool counterPick, Maybe<SpecialGameSlot> specialGameSlot, Maybe<PublisherGame> publisherGame)
        {
            SlotNumber = slotNumber;
            OverallSlotNumber = overallSlotNumber;
            CounterPick = counterPick;
            SpecialGameSlot = specialGameSlot;
            PublisherGame = publisherGame;
        }

        public int SlotNumber { get; }
        public int OverallSlotNumber { get; }
        public bool CounterPick { get; }
        public Maybe<SpecialGameSlot> SpecialGameSlot { get; }
        public Maybe<PublisherGame> PublisherGame { get; }

        public bool SlotHasEligibleGame(LeagueYear leagueYear)
        {
            if (CounterPick)
            {
                return true;
            }
            if (PublisherGame.HasNoValue)
            {
                return true;
            }

            bool? overridenEligibility = leagueYear.GetOverriddenEligibility(PublisherGame.Value.MasterGame);
            var tagOverrides = leagueYear.GetOverriddenTags(PublisherGame.Value.MasterGame);
            var eligible = CalculateGameIsCurrentlyEligible(leagueYear.Options, overridenEligibility, tagOverrides);
            return eligible;
        }

        public decimal GetProjectedOrRealFantasyPoints(LeagueYear leagueYear, SystemWideValues systemWideValues, bool simpleProjections, LocalDate currentDate)
        {
            if (PublisherGame.HasNoValue || PublisherGame.Value.MasterGame.HasNoValue)
            {
                if (PublisherGame.Value.ManualCriticScore.HasValue)
                {
                    return PublisherGame.Value.ManualCriticScore.Value;
                }

                return systemWideValues.GetAveragePoints(CounterPick);
            }

            decimal? fantasyPoints = CalculateFantasyPoints(leagueYear, currentDate);
            if (fantasyPoints.HasValue)
            {
                return fantasyPoints.Value;
            }

            if (simpleProjections)
            {
                return PublisherGame.Value.MasterGame.Value.GetSimpleProjectedFantasyPoints(systemWideValues, CounterPick);
            }

            return PublisherGame.Value.MasterGame.Value.GetProjectedOrRealFantasyPoints(leagueYear.Options.ScoringSystem, CounterPick, currentDate);
        }

        public decimal? CalculateFantasyPoints(LeagueYear leagueYear, LocalDate currentDate)
        {
            if (PublisherGame.HasNoValue)
            {
                return null;
            }
            if (PublisherGame.Value.ManualCriticScore.HasValue)
            {
                return leagueYear.Options.ScoringSystem.GetPointsForScore(PublisherGame.Value.ManualCriticScore.Value, CounterPick);
            }
            if (PublisherGame.Value.MasterGame.HasNoValue)
            {
                return null;
            }

            var eligible = SlotHasEligibleGame(leagueYear);
            if (!eligible)
            {
                return 0m;
            }

            return PublisherGame.Value.MasterGame.Value.CalculateFantasyPoints(leagueYear.Options.ScoringSystem, CounterPick, currentDate, true);
        }

        private bool CalculateGameIsCurrentlyEligible(LeagueOptions options, bool? overridenEligibility, IReadOnlyList<MasterGameTag> overriddenTags)
        {
            if (PublisherGame.HasNoValue || PublisherGame.Value.MasterGame.HasNoValue)
            {
                return true;
            }

            var specialSlot = options.GetSpecialGameSlotByOverallSlotNumber(SlotNumber);
            if (specialSlot.HasNoValue)
            {
                if (overridenEligibility.HasValue)
                {
                    return overridenEligibility.Value;
                }
            }
            else
            {
                var gameIsBanned = overridenEligibility.HasValue && !overridenEligibility.Value;
                if (gameIsBanned)
                {
                    return false;
                }
            }

            var leagueTags = options.GetTagsForSlot(SlotNumber);
            var customCodeTags = leagueTags.Where(x => x.Tag.HasCustomCode).ToList();
            var nonCustomCodeTags = leagueTags.Except(customCodeTags).ToList();

            var masterGame = PublisherGame.Value.MasterGame.Value.MasterGame;
            var bannedTags = nonCustomCodeTags.Where(x => x.Status == TagStatus.Banned).Select(x => x.Tag);
            var requiredTags = nonCustomCodeTags.Where(x => x.Status == TagStatus.Required).Select(x => x.Tag);

            var tagsToUse = masterGame.Tags;
            if (overriddenTags.Any())
            {
                tagsToUse = overriddenTags.ToList();
            }

            var bannedTagsIntersection = tagsToUse.Intersect(bannedTags);
            var requiredTagsIntersection = tagsToUse.Intersect(requiredTags);
            bool hasBannedTags = bannedTagsIntersection.Any();
            bool hasNoRequiredTags = requiredTags.Any() && !requiredTagsIntersection.Any();

            if (hasBannedTags || hasNoRequiredTags)
            {
                return false;
            }

            var masterGameCustomCodeTags = tagsToUse.Where(x => x.HasCustomCode).ToList();
            if (!masterGameCustomCodeTags.Any())
            {
                return true;
            }

            var dateGameWasAcquired = PublisherGame.Value.Timestamp.InZone(TimeExtensions.EasternTimeZone).Date;
            if (masterGame.EarlyAccessReleaseDate.HasValue)
            {
                var plannedForEarlyAccessTag = customCodeTags.SingleOrDefault(x => x.Tag.Name == "PlannedForEarlyAccess");
                if (plannedForEarlyAccessTag is not null)
                {
                    if (plannedForEarlyAccessTag.Status == TagStatus.Banned)
                    {
                        return false;
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
                            return false;
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
                        return false;
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
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
