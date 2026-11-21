<template>
  <div class="alert alert-primary">
    <template v-if="actionProcessing">
      <h3>Bids are processing as we speak</h3>
      On a good day, bids process within 10 minutes. If it's been more than 20 minutes, check Discord for updates.
    </template>
    <countdown v-if="!actionProcessing && nextPublicRevealTime" :end-time="nextPublicRevealTime" @finish="publicBidRevealTimeElapsed">
      <template #default="{ timeRemaining }">
        <span class="countdown">Bids will be revealed in {{ timeRemaining }}</span>
      </template>
    </countdown>
    <countdown v-if="!actionProcessing && nextBidTime" :end-time="nextBidTime" @finish="nextBidTimeElapsed">
      <template #default="{ timeRemaining }">
        <span class="countdown">Bids process in {{ timeRemaining }}</span>
      </template>
    </countdown>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import { mapState } from 'pinia';

import { useInterLeagueStore } from '@/store/interLeagueStore';
import Countdown from '@/components/countdown.vue';

type CountdownMode = 'NextBid' | 'NextPublic';

interface BidCountdownsData {
  forceActionProcessing: boolean;
}

export default defineComponent({
  components: {
    Countdown
  },
  props: {
    mode: { type: String as PropType<CountdownMode>, required: true }
  },
  emits: { publicBidRevealTimeElapsed: () => true },
  data(): BidCountdownsData {
    return {
      forceActionProcessing: false
    };
  },
  computed: {
    ...mapState(useInterLeagueStore, ['bidTimes']),
    nextPublicRevealTime(): string | null {
      if (this.mode !== 'NextPublic') {
        return null;
      }

      return this.bidTimes.nextPublicBiddingTime;
    },
    nextBidTime(): string | null {
      if (this.mode !== 'NextBid') {
        return null;
      }

      return this.bidTimes.nextBidLockTime;
    },
    actionProcessing(): boolean {
      return this.forceActionProcessing || this.bidTimes.actionProcessingMode;
    }
  },
  methods: {
    nextBidTimeElapsed(): void {
      this.forceActionProcessing = true;
    },
    publicBidRevealTimeElapsed(): void {
      this.$emit('publicBidRevealTimeElapsed');
    }
  }
});
</script>

<style scoped>
.countdown {
  font-size: 20px;
  font-weight: bold;
}
</style>
