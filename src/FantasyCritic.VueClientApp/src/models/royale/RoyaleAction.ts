import { MasterGameYear } from '@/models/masterGame/MasterGameYear.ts';

export interface RoyaleAction {
  masterGame: MasterGameYear | null;
  gameHidden: boolean;
  actionType: string | null;
  description: string | null;
  timestamp: string;
}
