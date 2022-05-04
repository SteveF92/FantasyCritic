<template>
  <div>
    <div class="alert alert-primary">
      <template v-if="actionProcessing">
        <h3>Bids are processing as we speak</h3>
        On a good day, bids process within 10 minutes. If it's been more than 20 minutes, check Twitter for updates.
      </template>
      <vac v-if="!actionProcessing && nextPublicRevealTime" :end-time="nextPublicRevealTime" @finish="publicBidRevealTimeElapsed">
        <span slot="process" slot-scope="{ timeObj }" class="countdown">Bids will be revealed in {{ `${timeObj.d} Days, ${timeObj.h} Hours, ${timeObj.m} Minutes, ${timeObj.s} Seconds` }}</span>
      </vac>
      <vac v-if="!actionProcessing && nextBidTime" :end-time="nextBidTime" @finish="nextBidTimeElapsed">
        <span slot="process" slot-scope="{ timeObj }" class="countdown">{{ `Bids process in ${timeObj.d} Days, ${timeObj.h} Hours, ${timeObj.m} Minutes, ${timeObj.s} Seconds` }}</span>
      </vac>
    </div>
  </div>
</template>

<script>
import moment from 'moment';

export default {
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
