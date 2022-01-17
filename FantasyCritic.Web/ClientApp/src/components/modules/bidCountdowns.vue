<template>
  <div>
    <div v-if="nextPublicRevealTime">
      <h2>Bids will be revealed in...</h2>
      <flip-countdown :deadline="nextPublicRevealTime"></flip-countdown>
    </div>
    <div v-if="nextBidTime">
      <h2>Bids lock in...</h2>
      <flip-countdown v-if="nextBidTime" :deadline="nextBidTime"></flip-countdown>
    </div>
  </div>
</template>

<script>
import FlipCountdown from 'vue2-flip-countdown'

export default {
  props: ['mode'],
  components: {
    FlipCountdown
  },
  computed: {
    nextPublicRevealTime() {
      if (this.mode !== 'NextPublic') {
        return null;
      }
      let date = new Date(this.$store.getters.bidTimes.nextPublicBiddingTime)
      return date.toString();
    },
    nextBidTime() {
      if (this.mode !== 'NextBid') {
        return null;
      }
      let date = new Date(this.$store.getters.bidTimes.nextBidLockTime)
      return date.toString();
    }
  }
}
</script>
<style>
  .flip-clock {
    border-radius: 25px;
    padding: 10px;
  }
</style>
