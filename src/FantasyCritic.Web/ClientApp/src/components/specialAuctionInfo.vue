<template>
  <b-alert variant="secondary" show>
    <div v-if="!isLocked" class="active-layout">
      <div>
        Special Auction in progress for:
        <masterGamePopover :master-game="specialAuction.masterGameYear"></masterGamePopover>
        <div>Bids will process for this game only on: {{ scheduledEndTime | dateTimeAt }}</div>
        <vac :end-time="scheduledEndTime" @finish="endTimeElapsed">
          <template #process="{ timeObj }">
            <span class="countdown">Time Remaining: {{ `${timeObj.d} Days, ${timeObj.h} Hours, ${timeObj.m} Minutes, ${timeObj.s} Seconds` }}</span>
          </template>
        </vac>
      </div>
      <b-button v-if="league.userIsInLeague" v-b-modal="`bidGameForm-${specialAuction.masterGameYear.masterGameID}`" variant="primary" class="bid-button">Place Bid</b-button>
    </div>
    <div v-else>
      <div>Auction closed at: {{ scheduledEndTime | dateTime }}</div>
      <h3>This auction is now locked, and will process shortly.</h3>
      On a good day, bids process within 10 minutes. If it's been more than 20 minutes, contact us on Twitter or Discord.
    </div>
    <bidGameForm :special-auction="specialAuction"></bidGameForm>
  </b-alert>
</template>

<script>
import moment from 'moment';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import BidGameForm from '@/components/modals/bidGameForm.vue';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  components: {
    MasterGamePopover,
    BidGameForm
  },
  mixins: [LeagueMixin],
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
  font-weight: bold;
}
.active-layout {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
  align-items: center;
}
</style>
