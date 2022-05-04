<template>
  <b-alert variant="secondary" show>
    Special Auction in progress for:
    <masterGamePopover :master-game="specialAuction.masterGameYear"></masterGamePopover>
    <template v-if="!isLocked">
      <div>Auction is scheduled to close at: {{ scheduledEndTime | dateTime }}</div>
      <b-button v-b-modal="`bidGameForm-${specialAuction.masterGameYear.masterGameID}`" variant="primary">Place Bid</b-button>
      <bidGameForm :special-auction="specialAuction"></bidGameForm>
      <br />
      <vac :end-time="scheduledEndTime" @finish="endTimeElapsed">
        <span slot="process" slot-scope="{ timeObj }" class="countdown">Auction will close in {{ `${timeObj.d} Days, ${timeObj.h} Hours, ${timeObj.m} Minutes, ${timeObj.s} Seconds` }}</span>
      </vac>
    </template>
    <template v-else>
      <div>Auction closed at: {{ scheduledEndTime | dateTime }}</div>
      <h3>This auction is now locked, and will process shortly.</h3>
      On a good day, bids process within 10 minutes. If it's been more than 20 minutes, contact me on Twitter or Discord.
    </template>
  </b-alert>
</template>

<script>
import moment from 'moment';
import MasterGamePopover from '@/components/masterGamePopover';
import BidGameForm from '@/components/modals/bidGameForm';

export default {
  components: {
    MasterGamePopover,
    BidGameForm
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
