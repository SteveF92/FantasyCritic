<template>
  <div>
    <div v-if="actionProcessing">
      <h3>Bids are processing as we speak.</h3>
      <p>On a good day, bids process within 10 minutes. If it's been more than 20 minutes, check Twitter for updates.</p>
    </div>
    <div class="alert alert-primary">
      <div v-if="!actionProcessing && nextPublicRevealTime">
        <vac :end-time="nextPublicRevealTime">
          <span slot="process" slot-scope="{ timeObj }" class="countdown">Bids will be revealed in {{ `${timeObj.d} Days, ${timeObj.h} Hours, ${timeObj.m} Minutes, ${timeObj.s} Seconds` }}</span>
        </vac>
      </div>
      <div v-if="!actionProcessing && nextBidTime">
        <vac :end-time="nextBidTime">
          <span slot="process" slot-scope="{ timeObj }" class="countdown">{{ `Bids process in ${timeObj.d} Days, ${timeObj.h} Hours, ${timeObj.m} Minutes, ${timeObj.s} Seconds` }}</span>
        </vac>
      </div>
    </div>
  </div>
</template>

<script>
import moment from 'moment';

export default {
  props: ['mode'],
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
<style scoped>
.countdown {
  font-size: 20px;
  font-weight: bold;
}
</style>
