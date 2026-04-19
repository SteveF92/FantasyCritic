import { RoyaleYearQuarter } from './RoyaleYearQuarter.ts';
import { RoyalePublisherGame } from './RoyalePublisherGame.ts';
import { RoyaleAction } from './RoyaleAction.ts';
import { RoyalePublisherStatistics } from './RoyalePublisherStatistics.ts';

export interface RoyalePublisher {
  publisherID: string;
  yearQuarter: RoyaleYearQuarter;
  userID: string;
  playerName: string;
  publisherName: string;
  publisherIcon: string | null;
  publisherSlogan: string | null;
  publisherGames: RoyalePublisherGame[];
  publisherActions: RoyaleAction[];
  statistics: RoyalePublisherStatistics[];
  budget: number;
  totalFantasyPoints: number;
  ranking: number | null;
  quartersWon: RoyaleYearQuarter[];
  previousQuarterWinner: boolean;
  oneTimeWinner: boolean;
}
