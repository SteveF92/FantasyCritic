using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;
using NodaTime;
using NodaTime.Text;

namespace FantasyCritic.FakeRepo.Factories
{
    internal static class MasterGameFactory
    {
        public static List<MasterGame> GetMasterGames()
        {
            List<MasterGame> games = new List<MasterGame>();

            var sekiro = CreateMasterGame("96f5e8e3-672b-4626-b47e-4bff3a6c4430", "Sekiro: Shadows Die Twice", "2019-03-22", new LocalDate(2019, 3, 22), 6630,
                89.9200m, 2019, 0, false, false, false, false, false, "Sekiro_art.jpg", "2019-03-20T15:30:00Z", false, "2019-01-22T15:30:00Z");
            var metroidPrime = CreateMasterGame("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96", "Metroid Prime 4", "TBA", null, null,
                null, 2020, 0, false, false, false, false, false, null, "2019-03-20T15:30:00Z", false, "2019-01-22T15:30:00Z");
            var civSix = CreateMasterGame("3729b47d-35b6-48c3-8acd-a57364617b8e", "Sid Meier's Civilization VI: Rise and Fall", "2018-02-08", new LocalDate(2018, 2, 8), 5405,
                82.6250m, 2019, 0, false, false, false, false, true, null, "2019-03-20T15:30:00Z", false, "2019-01-22T15:30:00Z");
            games.Add(sekiro);
            games.Add(metroidPrime);
            games.Add(civSix);

            return games;
        }

        private static MasterGame CreateMasterGame(string guid, string name, string estimatedReleaseDate, LocalDate? releaseDate, int? openCriticID, decimal? criticScore,
            int minimumReleaseYear, int eligibilityLevelID, bool yearlyInstallment, bool earlyAccess, bool freeToPlay, bool releasedInternationally, bool expansionPack,
            string boxartFileName, string firstCriticScoreTimestamp, bool doNotRefresh, string addedTimestamp)
        {
            EligibilityLevel eligibilityLevel = EligibilityLevelFactory.GetEligibilityLevels().Single(x => x.Level == eligibilityLevelID);

            var eligibilitySettings = new EligibilitySettings(eligibilityLevel, false, false, false, false, false);

            var game = new MasterGame(Guid.Parse(guid), name, estimatedReleaseDate, releaseDate, openCriticID,
                criticScore, minimumReleaseYear, eligibilitySettings, boxartFileName, 
                InstantPattern.ExtendedIso.Parse(firstCriticScoreTimestamp).GetValueOrThrow(), doNotRefresh,
                InstantPattern.ExtendedIso.Parse(addedTimestamp).GetValueOrThrow());

            return game;
        }
    }
}
