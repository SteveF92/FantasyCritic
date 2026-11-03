import { mapState } from 'pinia';

import { useAuthStore } from '@/store/authStore';
import { usePublisherStore } from '@/store/publisherStore';

const publisherMixin = {
  computed: {
    ...mapState(usePublisherStore, ['moveMode', 'publisher', 'leagueYear', 'gameSlots', 'sortOrderMode', 'coverArtMode', 'includeRemovedInSorted']),
    userIsPublisher() {
      const authStore = useAuthStore();
      return authStore.userInfo && this.publisher.userID === authStore.userInfo.userID;
    },
    hasFormerGames() {
      return this.publisher && this.publisher.formerGames.length > 0;
    }
  }
};

export default publisherMixin;
