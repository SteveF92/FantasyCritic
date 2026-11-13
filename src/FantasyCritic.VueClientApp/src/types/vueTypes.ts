import type { RoyalePublisherGameViewModel, RoyalePublisherStatisticsViewModel } from '@/api/clients';
import type { ExperienceLevel, GameMode, PresetResult } from '@/utilities/leagueCreationPresets';

export interface PopoverContent {
  title: string;
  body: string;
}

export interface RoyaleGamePurchase {
  gameName: string;
  purchaseCost: number;
}

export interface RoyalePublisherGraphSeries {
  publisherID: string;
  publisherName: string;
  statistics: RoyalePublisherStatisticsViewModel[];
  publisherGames: RoyalePublisherGameViewModel[];
}

export interface AppliedPreset extends PresetResult {
  gameMode: GameMode;
  experienceLevel: ExperienceLevel;
}
