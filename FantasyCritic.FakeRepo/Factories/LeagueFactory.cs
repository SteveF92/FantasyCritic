﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.FakeRepo.Factories
{
    internal static class LeagueFactory
    {
        public static List<League> GetLeagues()
        {
            var users = UserFactory.GetUsers();
            League league = new League(Guid.Parse("68227dc3-f5e1-4403-870a-e4d03cfc25d5"), "The Original Memers", users.Single(x => x.UserID == Guid.Parse("9142b786-f614-483f-92ca-1ef489508641")), 
                new List<int>(){2019}, true, false, 0);

            return new List<League>(){league};
        }

        public static List<LeagueYear> GetLeagueYears()
        {
            var leagues = GetLeagues();
            var eligibilityLevel = EligibilityLevelFactory.GetEligibilityLevels().Single(x => x.Level == 0);
            List<LeagueYear> leagueYears = new List<LeagueYear>();
            foreach (var league in leagues)
            {
                var eligibilitySettings = new EligibilitySettings(eligibilityLevel, true, false, true, false, false);
                var eligibilityOverrides = new List<EligibilityOverride>();
                LeagueYear year = new LeagueYear(league, 2019, new LeagueOptions(12, 6, 1, 2, -1, 0, false, eligibilitySettings, DraftSystem.Flexible, 
                    PickupSystem.Budget, ScoringSystem.GetScoringSystem("Standard"), true), PlayStatus.DraftFinal, eligibilityOverrides);
                leagueYears.Add(year);
            }

            return leagueYears;
        }

        public static Dictionary<League, List<FantasyCriticUser>> GetUsersInLeagues()
        {
            Dictionary<League, List<FantasyCriticUser>> membership = new Dictionary<League, List<FantasyCriticUser>>();
            var leagues = GetLeagues();
            var users = UserFactory.GetUsers();

            foreach (var league in leagues)
            {
                membership.Add(league, users);
            }

            return membership;
        }
    }
}
