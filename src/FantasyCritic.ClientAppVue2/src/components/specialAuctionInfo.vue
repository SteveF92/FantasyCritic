<template>
  <b-alert variant="secondary" show>
    <div v-if="!isLocked" class="active-layout">
      <div>
        Special Auction in progress for:
        <masterGamePopover :master-game="specialAuction.masterGameYear"></masterGamePopover>
        <div>Bids will process for this game only on: {{ formatDateTimeAt(scheduledEndTime) }}</div>
        <countdown :end-time="specialAuction.scheduledEndTime" @finish="endTimeElapsed">
          <template #default="{ timeRemaining }">
            <span class="countdown">Time Remaining: {{ timeRemaining }}</span>
          </template>
        </countdown>
      </div>
      <b-button v-if="league.userIsInLeague" v-b-modal="`bidGameForm-${specialAuction.masterGameYear.masterGameID}`" variant="primary" class="bid-button">Place Bid</b-button>
    </div>
    <div v-else>
      <div>Auction closed at: {{ formatDateTime(scheduledEndTime) }}</div>
      <h3>This auction is now locked, and will process shortly.</h3>
      On a good day, bids process within 10 minutes. If it's been more than 20 minutes, contact us on Discord.
    </div>
    <bidGameForm :special-auction="specialAuction"></bidGameForm>
  </b-alert>
</template>

<script>
import { DateTime } from 'luxon';
import Countdown from '@/components/countdown.vue';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import BidGameForm from '@/components/modals/bidGameForm.vue';
import { mapState } from 'pinia';
import { useLeagueStore } from '@/store/leagueStore';
import { formatDateTime, formatDateTimeAt } from '@/utilities/formatters';

export default {
  components: {
    Countdown,
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
    ...mapState(useLeagueStore, ['league']),
    scheduledEndTime() {
      return DateTime.fromISO(this.specialAuction.scheduledEndTime).toFormat('yyyy-MM-dd HH:mm:ss');
    },
    isLocked() {
      return this.specialAuction.isLocked || this.specialAuction.processed || this.forceLocked;
    }
  },
  methods: {
    formatDateTime,
    formatDateTimeAt,
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
