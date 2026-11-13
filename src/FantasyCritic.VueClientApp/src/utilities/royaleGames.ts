import type { MasterGameYearViewModel, RoyalePublisherGameViewModel } from '@/api/clients';

export type OwnRoyalePublisherGame = RoyalePublisherGameViewModel & {
  masterGame: MasterGameYearViewModel;
  refundAmount: number;
};

export function asOwnRoyalePublisherGame(game: RoyalePublisherGameViewModel): OwnRoyalePublisherGame {
  const { masterGame, refundAmount } = game;
  if (!masterGame || refundAmount === undefined) {
    throw new Error('Expected one of your own Royale games, with its master game and refund amount.');
  }
  return { ...game, masterGame, refundAmount };
}
