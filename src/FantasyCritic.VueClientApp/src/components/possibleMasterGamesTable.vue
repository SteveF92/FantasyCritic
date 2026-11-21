<template>
  <div>
    <b-pagination v-model="currentPage" :total-rows="possibleGames.length" :per-page="perPage" aria-controls="my-table"></b-pagination>

    <b-table v-model:sort-by="sortBy" :items="possibleGames" :fields="gameFields" :per-page="perPage" :current-page="currentPage" bordered small responsive striped>
      <template #cell(masterGame.gameName)="data">
        <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
      </template>
      <template #cell(masterGame.maximumReleaseDate)="data">
        <div :class="{ 'text-danger': data.item.masterGame.isReleased && !data.item.masterGame.releasingToday }" class="release-date">
          <span>
            {{ data.item.masterGame.estimatedReleaseDate }}
            <font-awesome-icon
              v-if="bidMode && getBidEligibility(data.item.masterGame) === 'tooLate'"
              v-b-popover.hover.focus="'Game is likely to be ineligible by the time bids are processed.'"
              icon="xmark"
              size="xl" />

            <font-awesome-icon
              v-if="bidMode && getBidEligibility(data.item.masterGame) === 'lastChance'"
              v-b-popover.hover.focus="'This might be your last chance to bid on this game.'"
              icon="exclamation-triangle" />
          </span>
          <span v-if="data.item.masterGame.isReleased && !data.item.masterGame.releasingToday" class="release-date-qualifier">(Released)</span>
          <span v-if="data.item.masterGame.releasingToday" class="release-date-qualifier">(Today)</span>
        </div>
      </template>
      <template #cell(masterGame.dateAdjustedHypeFactor)="data">
        {{ formatScore(data.item.masterGame.dateAdjustedHypeFactor, 1) }}
      </template>
      <template #cell(status)="data">
        <statusBadge :possible-master-game="data.item"></statusBadge>
      </template>
      <template #cell(select)="data">
        <b-button size="sm" variant="info" @click="selectGame(data.item.masterGame)">Select</b-button>
      </template>
    </b-table>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { BTableSortBy, TableField } from 'bootstrap-vue-next';

import type { MasterGameYearViewModel, PossibleMasterGameYearViewModel } from '@/api/clients';
import { numericComparer } from '@/globalFunctions';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { useLeagueStore } from '@/store/leagueStore';
import { formatScore } from '@/utilities/formatters';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import StatusBadge from '@/components/statusBadge.vue';

interface PossibleMasterGamesTableData {
  perPage: number;
  currentPage: number;
  sortBy: BTableSortBy[];
}

type BidEligibility = 'notYetEligible' | 'tooLate' | 'lastChance' | 'noEligibilityWarning';

export default defineComponent({
  setup() {
    const gameFields: TableField<PossibleMasterGameYearViewModel>[] = [
      { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
      { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
      { key: 'masterGame.dateAdjustedHypeFactor', label: 'Hype Factor', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum', sortCompare: numericComparer },
      { key: 'status', label: 'Status', thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
      { key: 'select', label: '', thClass: 'bg-primary' }
    ];

    return { gameFields };
  },
  components: {
    StatusBadge,
    MasterGamePopover
  },
  props: {
    modelValue: { type: Object as PropType<MasterGameYearViewModel | null>, default: null },
    possibleGames: { type: Array as PropType<PossibleMasterGameYearViewModel[]>, required: true },
    bidMode: { type: Boolean, required: false, default: false }
  },
  emits: {
    'update:modelValue': (_value: MasterGameYearViewModel) => true
  },
  data(): PossibleMasterGamesTableData {
    return {
      perPage: 10,
      currentPage: 1,
      sortBy: [{ key: 'masterGame.dateAdjustedHypeFactor', order: 'desc' }]
    };
  },
  computed: {
    pickupSystem(): string | undefined {
      const leagueStore = useLeagueStore();
      return leagueStore.leagueYear?.settings?.pickupSystem;
    }
  },
  methods: {
    formatScore,
    selectGame(masterGame: MasterGameYearViewModel): void {
      this.$emit('update:modelValue', masterGame);
    },
    /**
     * Determines the bid eligibility status for a given master game based on its release date.
     */
    getBidEligibility(masterGame: MasterGameYearViewModel): BidEligibility {
      if (!masterGame.releaseDate) {
        return 'notYetEligible';
      }
      const releaseTime = new Date(masterGame.releaseDate).getTime();
      const nextBidTime = this.retrieveNextOpenBiddingTime().getTime();
      const followingBidTime = this.retrieveFollowingOpenBiddingTime().getTime();

      if (releaseTime < nextBidTime) {
        return 'tooLate';
      } else if (releaseTime < followingBidTime) {
        return 'lastChance';
      } else {
        return 'noEligibilityWarning';
      }
    },
    /**
     * Retrieves the next open bidding time based on the league's bidding system.
     * - If the pickup system is "SecretBidding", returns the next bid lock time.
     * - For every public bidding system, returns the next public bidding time, since that reveal is the point after which
     *   you can no longer freely bid on a game that nobody else has bid on.
     */
    retrieveNextOpenBiddingTime(): Date {
      const interLeagueStore = useInterLeagueStore();
      const bidTimes = interLeagueStore.bidTimes;
      if (this.pickupSystem === 'SecretBidding') {
        return new Date(bidTimes.nextBidLockTime);
      }
      return new Date(bidTimes.nextPublicBiddingTime);
    },
    /**
     * Calculates the following open bidding time based on the league's bidding system.
     * - Retrieves the next open bidding time using `retrieveNextOpenBiddingTime()`.
     * - Determines the interval in days: 7 for "SecretBidding", 14 otherwise.
     * - Returns a new Date object representing the next open bidding time plus the interval.
     */
    retrieveFollowingOpenBiddingTime(): Date {
      const nextBidTime = this.retrieveNextOpenBiddingTime();
      const intervalDays = this.pickupSystem === 'SecretBidding' ? 7 : 14;

      return new Date(nextBidTime.getTime() + intervalDays * 24 * 60 * 60 * 1000);
    }
  }
});
</script>

<style scoped>
.fake-link {
  text-decoration: underline;
  cursor: pointer;
}

.release-date {
  font-weight: bold;
}

.select-cell {
  text-align: center;
}

@media only screen and (max-width: 450px) {
  .no-mobile {
    display: none;
  }
}

.release-date-qualifier {
  margin-left: 5px;
}
.emoji-tooltip {
  cursor: pointer;
}
</style>
