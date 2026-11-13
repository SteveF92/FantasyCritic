import type { RoyalePublisherGameViewModel, RoyalePublisherStatisticsViewModel } from '@/api/clients';

export interface PopoverContent {
  title: string;
  body: string;
}

export interface RoyaleGamePurchase {
  gameName: string;
  purchaseCost: number;
}

export interface RoyalePublisherGraphSeries {
  publisherID?: string;
  publisherName?: string;
  statistics: RoyalePublisherStatisticsViewModel[];
  publisherGames: RoyalePublisherGameViewModel[];
}
