import { MasterGameYear } from '@/models/masterGame/MasterGameYear';

export interface RoyalePublisherGame {
  publisherID: string;
  gameHidden: boolean;
  masterGame: MasterGameYear | null;
  locked: boolean;
  timestamp: string;
  lockDateTime: string | null;
  amountSpent: number | null;
  advertisingMoney: number;
  fantasyPoints: number | null;
  currentlyIneligible: boolean;
  refundAmount: number | null;
}
