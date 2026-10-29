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

<script>
import Countdown from '@/components/countdown.vue';

export default {
  components: {
    Countdown
  },
  props: {
    mode: { type: String, required: true }
  },
  data() {
    return {
      forceActionProcessing: false
    };
  },
  computed: {
    nextPublicRevealTime() {
      if (this.mode !== 'NextPublic') {
        return null;
      }

      return this.$store.getters.bidTimes.nextPublicBiddingTime;
    },
    nextBidTime() {
      if (this.mode !== 'NextBid') {
        return null;
      }

      return this.$store.getters.bidTimes.nextBidLockTime;
    },
    actionProcessing() {
      return this.forceActionProcessing || this.$store.getters.bidTimes.actionProcessingMode;
    }
  },
  methods: {
    nextBidTimeElapsed() {
      this.forceActionProcessing = true;
    },
    publicBidRevealTimeElapsed() {
      this.$emit('publicBidRevealTimeElapsed');
    }
  }
};
</script>
<style scoped>
.countdown {
  font-size: 20px;
  font-weight: bold;
}
</style>
