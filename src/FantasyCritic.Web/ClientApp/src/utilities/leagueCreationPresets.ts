// src/FantasyCritic.Web/ClientApp/src/utilities/leagueCreationPresets.ts

export type GameMode = 'Standard' | 'Multi Draft' | 'One Shot';
export type ExperienceLevel = 'Beginner' | 'Standard' | 'Advanced';

export interface DraftSettings {
  name: string | null;
  scheduledDate: string | null;
  gamesToDraft: number;
  counterPicksToDraft: number;
}

export interface LeagueYearSettingsPartial {
  standardGames: number;
  counterPicks: number;
  gamesToDraft?: number; // not used post-refactor; included for transitional compat
  counterPicksToDraft?: number; // same
  minimumBidAmount: number;
  enableBids: boolean;
  tradingSystem: string;
  grantSuperDrops: boolean;
  pickupSystem: string;
  counterPicksBlockDrops: boolean;
  unrestrictedReleaseStatusDroppableGames: number;
  willNotReleaseDroppableGames: number;
  willReleaseDroppableGames: number;
  unlimitedUnrestrictedReleaseStatusDroppableGames: boolean;
  unlimitedWillNotReleaseDroppableGames: boolean;
  unlimitedWillReleaseDroppableGames: boolean;
  specialGameSlots: object[];
  tags: { banned: string[]; required: string[] };
  counterPickDeadline: string;
  mightReleaseDroppableDate: string;
}

export interface PresetResult {
  settings: Partial<LeagueYearSettingsPartial>;
  drafts: DraftSettings[];
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

  let standardGames = Math.floor((avgStdGames + thisStdGames) / 2);
  let counterPicks = Math.floor((avgCPKs + thisCPKs) / 2);
  let gamesToDraft = Math.floor((avgGTD + thisGTD) / 2);
  let counterPicksToDraft = Math.floor((avgCPKTD + thisCPKTD) / 2);

  if (counterPicks === 0 || counterPicksToDraft === 0) {
    counterPicks = 1;
    counterPicksToDraft = 1;
  }

  return { standardGames, counterPicks, gamesToDraft, counterPicksToDraft };
}

function computeSpecialSlots(standardGames: number, experienceLevel: ExperienceLevel): object[] {
  if (experienceLevel === 'Beginner') return [];
  const numberOfSpecialSlots = Math.floor(standardGames / 2);
  if (numberOfSpecialSlots < 1) return [];

  const slots: object[] = [];
  const includeExpansion = numberOfSpecialSlots >= 2;
  const includeRemake = numberOfSpecialSlots >= 2;
  const nonNgfCount = (includeExpansion ? 1 : 0) + (includeRemake ? 1 : 0);
  const ngfCount = Math.max(0, numberOfSpecialSlots - nonNgfCount);

  for (let i = 0; i < ngfCount; i++) {
    slots.push({ specialSlotPosition: slots.length, requiredTags: ['NewGamingFranchise'] });
  }
  if (includeExpansion) {
    slots.push({ specialSlotPosition: slots.length, requiredTags: ['NewGamingFranchise', 'ExpansionPack'] });
  }
  if (includeRemake) {
    slots.push({ specialSlotPosition: slots.length, requiredTags: ['NewGamingFranchise', 'PartialRemake', 'Remake', 'Reimagining'] });
  }
  return slots;
}

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

export function computePreset(gameMode: GameMode, experienceLevel: ExperienceLevel, playerCount: number, year: number): PresetResult {
  let recommendedNumberOfGames: number;
  let draftGameRatio: number;

  if (gameMode === 'One Shot') {
    recommendedNumberOfGames = 50;
    draftGameRatio = 1;
  } else if (experienceLevel === 'Beginner') {
    recommendedNumberOfGames = 42;
    draftGameRatio = 4 / 7;
  } else if (experienceLevel === 'Advanced') {
    recommendedNumberOfGames = 108;
    draftGameRatio = 4 / 9;
  } else {
    recommendedNumberOfGames = 72;
    draftGameRatio = 1 / 2;
  }

  const { standardGames, counterPicks, gamesToDraft, counterPicksToDraft } = computeGameCounts(recommendedNumberOfGames, draftGameRatio, playerCount);

  const alwaysBanned = ['Port', 'PlannedForEarlyAccess', 'CurrentlyInEarlyAccess', 'ReleasedInternationally'];
  const standardBanned = ['YearlyInstallment', 'DirectorsCut', 'PartialRemake', 'Remaster', 'ExpansionPack'];
  const bannedTags = experienceLevel === 'Beginner' ? alwaysBanned : [...alwaysBanned, ...standardBanned];

  const settings: Partial<LeagueYearSettingsPartial> = {
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
    mightReleaseDroppableDate: `${year}-11-01`
  };

  let drafts: DraftSettings[];
  if (gameMode === 'Multi Draft') {
    const draft1Games = Math.ceil(gamesToDraft / 2);
    const draft2Games = gamesToDraft - draft1Games;
    drafts = [
      { name: getDefaultDraftName(0), scheduledDate: null, gamesToDraft: draft1Games, counterPicksToDraft },
      { name: getDefaultDraftName(1), scheduledDate: null, gamesToDraft: Math.max(1, draft2Games), counterPicksToDraft: 0 }
    ];
  } else if (gameMode === 'One Shot') {
    drafts = [{ name: getDefaultDraftName(0), scheduledDate: null, gamesToDraft: standardGames, counterPicksToDraft: counterPicks }];
  } else {
    drafts = [{ name: getDefaultDraftName(0), scheduledDate: null, gamesToDraft, counterPicksToDraft }];
  }

  return { settings, drafts };
}

export function getDefaultDraftName(draftIndex: number): string {
  return draftIndex === 0 ? 'Initial Draft' : `Draft ${draftIndex + 1}`;
}

export function getDefaultDraft(draftIndex: number, standardGames: number, allocatedSoFar: number): DraftSettings {
  const remaining = Math.max(1, standardGames - allocatedSoFar);
  return { name: getDefaultDraftName(draftIndex), scheduledDate: null, gamesToDraft: remaining, counterPicksToDraft: 0 };
}
