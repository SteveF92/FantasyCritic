using System;
using System.Collections.Generic;
using FantasyCritic.Lib.Identity;
using NodaTime.Text;

namespace FantasyCritic.FakeRepo.Factories
{
    public static class UserFactory
    {
        public static List<FantasyCriticUser> GetUsers()
        {
            List<FantasyCriticUser> users = new List<FantasyCriticUser>();

            //Password is: 8kyKaxEr4Q1NJedQJpdp
            FantasyCriticUser demoUser = new FantasyCriticUser(Guid.Parse("9142b786-f614-483f-92ca-1ef489508641"), "Demo User", null, 9088, "demo@fantasycritic.games",
                "DEMO@FANTASYCRITIC.GAMES", true, "Q5JER6BNH6QSOD6OZYORZTHTWS6WKDMH", "AQAAAAEAACcQAAAAEKV4ZCnHDTX3wGEXdcPFdp0aJahHZm7HUE7vSry9hGlosaRW7cGQZByX1w+jjBd1XQ==", false, null,
                InstantPattern.ExtendedIso.Parse("2019-04-13T15:30:00Z").GetValueOrThrow(), false);

            users.Add(demoUser);

            return users;
        }
    }
}
