import { mapState } from 'pinia';

import { usePublisherStore } from '@/store/publisherStore';

const publisherMixin = {
  computed: {
    ...mapState(usePublisherStore, [
      'moveMode',
      'publisher',
      'leagueYear',
      'gameSlots',
      'sortOrderMode',
      'coverArtMode',
      'includeRemovedInSorted',
      'historicalTrades',
      'showProjections',
      'draftOrderView'
    ]),
    userIsPublisher() {
      const publisherStore = usePublisherStore();
      return publisherStore.getters.userInfo && this.publisher.userID === publisherStore.getters.userInfo.userID;
    },
    hasFormerGames() {
      return this.publisher && this.publisher.formerGames.length > 0;
    }
  },
  methods: {
    async notifyAction(message) {
      const publisherStore = usePublisherStore();
      await publisherStore.refreshPublisher();
      if (message) {
        this.makeToast(message);
      }
    }
  }
};

export default publisherMixin;
