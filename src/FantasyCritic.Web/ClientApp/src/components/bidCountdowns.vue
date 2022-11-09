<template>
  <div>
    <div class="alert alert-primary">
      <template v-if="actionProcessing">
        <h3>Bids are processing as we speak</h3>
        On a good day, bids process within 10 minutes. If it's been more than 20 minutes, check Twitter for updates.
      </template>
      <vac v-if="!actionProcessing && nextPublicRevealTime" :end-time="nextPublicRevealTime" @finish="publicBidRevealTimeElapsed">
        <template #process="{ timeObj }">
          <span class="countdown">Bids will be revealed in {{ `${timeObj.d} Days, ${timeObj.h} Hours, ${timeObj.m} Minutes, ${timeObj.s} Seconds` }}</span>
        </template>
      </vac>
      <vac v-if="!actionProcessing && nextBidTime" :end-time="nextBidTime" @finish="nextBidTimeElapsed">
        <template #process="{ timeObj }">
          <span class="countdown">{{ `Bids process in ${timeObj.d} Days, ${timeObj.h} Hours, ${timeObj.m} Minutes, ${timeObj.s} Seconds` }}</span>
        </template>
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

      return moment(this.$store.getters.bidTimes.nextPublicBiddingTime).toDate();
    },
    nextBidTime() {
      if (this.mode !== 'NextBid') {
        return null;
      }

      return moment(this.$store.getters.bidTimes.nextBidLockTime).toDate();
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
