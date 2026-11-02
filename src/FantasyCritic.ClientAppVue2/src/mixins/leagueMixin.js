import { mapState } from 'pinia';

import { useAuthStore } from '@/store/authStore';
import { useLeagueStore } from '@/store/leagueStore';
import { makeToast } from '@/utilities/toast';

const leagueMixin = {
  computed: {
    ...mapState(useLeagueStore, [
      'hasError',
      'forbidden',
      'inviteCode',
      'leagueYear',
      'userPublisher',
      'leagueActions',
      'leagueActionSets',
      'historicalTrades',
      'showProjections',
      'draftOrderView',
      'league',
      'publishers',
      'players',
      'hasSpecialSlots',
      'supportedYear',
      'firstDraft',
      'pendingDraft',
      'activeDraft',
      'nextPublisherUp',
      'userIsNextInDraft',
      'draftIsPaused',
      'isManager',
      'publishersThatCanWin',
      'topPublisher',
      'currentBids',
      'currentDrops',
      'queuedGames',
      'gameNews',
      'oneShotMode',
      'playStarted',
      'readyToSetDraftOrder',
      'firstDraftFinished',
      'hasPendingOrActiveDraft',
      'pendingDraftIsFirst',
      'pendingDraftIsWithinSevenDays',
      'pendingDraftIsOverdue',
      'pendingDraftIsImminent',
      'pendingDraftCalloutVisible',
      'draftIsActiveOrPaused',
      'biddingAllowed',
      'counterPickBiddingAllowed',
      'tradesAllowed',
      'dropsAllowed',
      'postDraftPlayable',
      'postDraftEditable'
    ]),
    ...mapState(useAuthStore, ['decimalsToShow'])
  },
  methods: {
    async notifyAction(message, refresh = true) {
      if (refresh) {
        await useLeagueStore().notifyAction(message);
      } else if (message) {
        makeToast(message);
      }
    }
  }
};

export default leagueMixin;
