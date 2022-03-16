<template>
  <div>
    <div v-if="actionProcessing">
      <h3>Bids are processing as we speak.</h3>
      <p>On a good day, bids process within 10 minutes. If it's been more than 20 minutes, check Twitter for updates.</p>
    </div>
    <div v-if="!actionProcessing && nextPublicRevealTime">
      <h2>Bids will be revealed in...</h2>
      <flip-countdown :deadline="nextPublicRevealTime" @timeElapsed="publicBidRevealTimeElapsed"></flip-countdown>
    </div>
    <div v-if="!actionProcessing && nextBidTime">
      <h2>Bids process in...</h2>
      <flip-countdown :deadline="nextBidTime" @timeElapsed="nextBidTimeElapsed"></flip-countdown>
    </div>
  </div>
</template>

<script>
import FlipCountdown from 'vue2-flip-countdown';
import moment from 'moment';

export default {
  props: ['mode'],
  components: {
    FlipCountdown
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
      let date = moment(this.$store.getters.bidTimes.nextPublicBiddingTime);
      return date.format('YYYY-MM-DD HH:mm:ss');
    },
    nextBidTime() {
      if (this.mode !== 'NextBid') {
        return null;
      }
      let date = moment(this.$store.getters.bidTimes.nextBidLockTime);
      return date.format('YYYY-MM-DD HH:mm:ss');
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
<style>
.flip-clock {
  border-radius: 25px;
  padding: 10px;
}
</style>
