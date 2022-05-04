<template>
  <b-alert variant="secondary" show>
    Special Auction in progress for:
    <masterGamePopover :master-game="specialAuction.masterGameYear"></masterGamePopover>
    <br />
    <vac v-if="!isLocked" :end-time="scheduledEndTime" @finish="endTimeElapsed">
      <span slot="process" slot-scope="{ timeObj }" class="countdown">Auction will close in {{ `${timeObj.d} Days, ${timeObj.h} Hours, ${timeObj.m} Minutes, ${timeObj.s} Seconds` }}</span>
    </vac>
    <template v-else>
      <h3>This auction is now locked, and will process shortly.</h3>
      On a good day, bids process within 10 minutes. If it's been more than 20 minutes, contact me on Twitter or Discord.
    </template>
  </b-alert>
</template>

<script>
import moment from 'moment';
import MasterGamePopover from '@/components/masterGamePopover';

export default {
  components: {
    MasterGamePopover
  },
  props: {
    specialAuction: { type: Object, required: true }
  },
  data() {
    return {
      forceLocked: false
    };
  },
  computed: {
    scheduledEndTime() {
      let date = moment(this.specialAuction.scheduledEndTime);
      return date.format('YYYY-MM-DD HH:mm:ss');
    },
    isLocked() {
      return this.specialAuction.isLocked || this.specialAuction.processed || this.forceLocked;
    }
  },
  methods: {
    endTimeElapsed() {
      this.forceLocked = true;
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
