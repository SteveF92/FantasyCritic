import type { DraftSettingsRequest, LeagueYearSettingsRequest, SpecialGameSlotViewModel } from '@/api/clients';

export type GameMode = 'Standard' | 'Multi Draft' | 'One Shot';
export type ExperienceLevel = 'Beginner' | 'Standard' | 'Advanced';

export interface PresetResult {
  settings: LeagueYearSettingsRequest;
  drafts: DraftSettingsRequest[];
}

// ---------------------------------------------------------------------------
// Internal helpers
// ---------------------------------------------------------------------------

function computeGameCounts(
  recommendedNumberOfGames: number,
  draftGameRatio: number,
  playerCount: number
): { standardGames: number; counterPicks: number; gamesToDraft: number; counterPicksToDraft: number } {
  const avgStdGames = Math.floor(recommendedNumberOfGames / 6);
  const avgCPKs = Math.floor(avgStdGames / 6);
  const avgGTD = Math.floor(avgStdGames * draftGameRatio);
  const avgCPKTD = Math.floor(avgCPKs * draftGameRatio);

  const thisStdGames = Math.floor(recommendedNumberOfGames / playerCount);
  const thisCPKs = Math.floor(thisStdGames / 6);
  const thisGTD = Math.floor(thisStdGames * draftGameRatio);
  const thisCPKTD = Math.floor(thisCPKs * draftGameRatio);

  const standardGames = Math.floor((avgStdGames + thisStdGames) / 2);
  let counterPicks = Math.floor((avgCPKs + thisCPKs) / 2);
  const gamesToDraft = Math.floor((avgGTD + thisGTD) / 2);
  let counterPicksToDraft = Math.floor((avgCPKTD + thisCPKTD) / 2);

  if (counterPicks === 0 || counterPicksToDraft === 0) {
    counterPicks = 1;
    counterPicksToDraft = 1;
  }

  return { standardGames, counterPicks, gamesToDraft, counterPicksToDraft };
}

export function computeSpecialSlots(standardGames: number, experienceLevel: ExperienceLevel): SpecialGameSlotViewModel[] {
  if (experienceLevel === 'Beginner') return [];
  const numberOfSpecialSlots = Math.floor(standardGames / 2);
  if (numberOfSpecialSlots < 1) return [];

  const slots: SpecialGameSlotViewModel[] = [];
  const includeExpansion = numberOfSpecialSlots >= 2;
  const includeRemake = numberOfSpecialSlots >= 2;
  const nonNgfCount = (includeExpansion ? 1 : 0) + (includeRemake ? 1 : 0);
  const ngfCount = Math.max(0, numberOfSpecialSlots - nonNgfCount);

  for (let i = 0; i < ngfCount; i++) {
    slots.push({ specialSlotPosition: slots.length, requiredTags: ['NewGamingFranchise'] });
  }
  if (includeExpansion) {
    slots.push({ specialSlotPosition: slots.length, requiredTags: ['ExpansionPack'] });
  }
  if (includeRemake) {
    slots.push({ specialSlotPosition: slots.length, requiredTags: ['PartialRemake', 'Remake', 'Reimagining', 'DirectorsCut'] });
  }
  return slots;
}

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

export function computePreset(gameMode: GameMode, experienceLevel: ExperienceLevel, playerCount: number, year: number): PresetResult {
  let recommendedNumberOfGames: number;
  let draftGameRatio: number;

  if (experienceLevel === 'Beginner') {
    recommendedNumberOfGames = 42;
    draftGameRatio = 4 / 7;
  } else if (experienceLevel === 'Advanced') {
    recommendedNumberOfGames = 108;
    draftGameRatio = 4 / 9;
  } else {
    recommendedNumberOfGames = 72;
    draftGameRatio = 1 / 2;
  }

  if (gameMode === 'One Shot') {
    recommendedNumberOfGames = Math.floor(recommendedNumberOfGames * 0.75);
    draftGameRatio = 1;
  }

  const { standardGames, counterPicks, gamesToDraft, counterPicksToDraft } = computeGameCounts(recommendedNumberOfGames, draftGameRatio, playerCount);

  const alwaysBanned = ['Port', 'PlannedForEarlyAccess', 'CurrentlyInEarlyAccess', 'ReleasedInternationally'];
  const standardBanned = ['YearlyInstallment', 'DirectorsCut', 'PartialRemake', 'Remaster', 'ExpansionPack'];
  const bannedTags = experienceLevel === 'Beginner' ? alwaysBanned : [...alwaysBanned, ...standardBanned];

  const settings: LeagueYearSettingsRequest = {
    standardGames,
    counterPicks,
    minimumBidAmount: 0,
    enableBids: gameMode !== 'One Shot' && gameMode !== 'Multi Draft',
    tradingSystem: gameMode === 'One Shot' || experienceLevel === 'Beginner' ? 'NoTrades' : 'Standard',
    grantSuperDrops: experienceLevel === 'Beginner',
    pickupSystem: experienceLevel === 'Advanced' ? 'SecretBidding' : 'SemiPublicBiddingSecretCounterPicks',
    counterPicksBlockDrops: experienceLevel === 'Advanced',
    unrestrictedReleaseStatusDroppableGames: 0,
    willNotReleaseDroppableGames: 0,
    unlimitedUnrestrictedReleaseStatusDroppableGames: false,
    unlimitedWillReleaseDroppableGames: false,
    willReleaseDroppableGames: gameMode === 'One Shot' ? 0 : 1,
    unlimitedWillNotReleaseDroppableGames: gameMode !== 'One Shot',
    specialGameSlots: computeSpecialSlots(standardGames, experienceLevel),
    tags: { banned: bannedTags, required: [] },
    counterPickDeadline: `${year}-11-01`,
    mightReleaseDroppableDate: `${year}-11-01`,
    year: year,
    dropOnlyDraftGames: false,
    allowMoveIntoIneligible: false,
    bidsOnlyBeforeNextScheduledDraft: false,
    scoringSystem: 'LinearPositive',
    tiebreakSystem: 'LowestProjectedPoints',
    releaseSystem: 'MustBeReleased',
    ineligibleGameSystem: 'CaseByCase'
  };

  let drafts: DraftSettingsRequest[];
  if (gameMode === 'Multi Draft') {
    const draft1Games = Math.ceil(gamesToDraft / 2);
    const draft2Games = gamesToDraft - draft1Games;
    drafts = [
      { name: getDefaultDraftName(0), scheduledDate: undefined, gamesToDraft: draft1Games, counterPicksToDraft, counterPicksMustBeFromThisDraft: true },
      { name: getDefaultDraftName(1), scheduledDate: undefined, gamesToDraft: Math.max(1, draft2Games), counterPicksToDraft: 0, counterPicksMustBeFromThisDraft: true }
    ];
  } else if (gameMode === 'One Shot') {
    drafts = [buildOneShotDraft(standardGames, counterPicks)];
  } else {
    drafts = [{ name: getDefaultDraftName(0), scheduledDate: undefined, gamesToDraft, counterPicksToDraft, counterPicksMustBeFromThisDraft: true }];
  }

  return { settings, drafts };
}

export function getDefaultDraftName(draftIndex: number): string {
  return draftIndex === 0 ? 'Initial Draft' : `Draft ${draftIndex + 1}`;
}

export function buildOneShotDraft(standardGames: number, counterPicks: number, existing?: Partial<DraftSettingsRequest>): DraftSettingsRequest {
  return {
    name: getDefaultDraftName(0),
    scheduledDate: existing?.scheduledDate,
    gamesToDraft: standardGames,
    counterPicksToDraft: counterPicks,
    counterPicksMustBeFromThisDraft: true
  };
}

export function getDefaultDraft(draftIndex: number, standardGames: number, allocatedSoFar: number): DraftSettingsRequest {
  const remaining = Math.max(1, standardGames - allocatedSoFar);
  return { name: getDefaultDraftName(draftIndex), scheduledDate: undefined, gamesToDraft: remaining, counterPicksToDraft: 0, counterPicksMustBeFromThisDraft: true };
}
