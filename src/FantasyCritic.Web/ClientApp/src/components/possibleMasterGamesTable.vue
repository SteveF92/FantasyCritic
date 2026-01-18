<template>
  <div>
    <b-pagination v-model="currentPage" :total-rows="rows" :per-page="perPage" aria-controls="my-table"></b-pagination>

    <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="gameRows" :fields="gameFields" :per-page="perPage" :current-page="currentPage" bordered small responsive striped>
      <template #cell(masterGame.gameName)="data">
        <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
      </template>
      <template #cell(masterGame.maximumReleaseDate)="data">
        <div :class="{ 'text-danger': data.item.masterGame.isReleased && !data.item.masterGame.releasingToday }" class="release-date">
          <span>
            {{ data.item.masterGame.estimatedReleaseDate }}
            <font-awesome-icon
              v-if="getBidEligibility(data.item.masterGame) === 'tooLate'"
              v-b-popover.hover.focus="'Game is likely to be ineligible by the time bids are processed.'"
              icon="xmark"
              size="xl" />

            <font-awesome-icon
              v-if="getBidEligibility(data.item.masterGame) === 'lastChance'"
              v-b-popover.hover.focus="'This might be your last chance to bid on this game.'"
              icon="exclamation-triangle" />
          </span>
          <span v-if="data.item.masterGame.isReleased && !data.item.masterGame.releasingToday" class="release-date-qualifier">(Released)</span>
          <span v-if="data.item.masterGame.releasingToday" class="release-date-qualifier">(Today)</span>
        </div>
      </template>
      <template #cell(masterGame.dateAdjustedHypeFactor)="data">
        {{ data.item.masterGame.dateAdjustedHypeFactor | score(1) }}
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
<script>
import StatusBadge from '@/components/statusBadge.vue';
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    StatusBadge,
    MasterGamePopover
  },
  props: {
    value: { type: Object, default: null },
    possibleGames: { type: Array, required: true }
  },
  data() {
    return {
      selectedMasterGame: null,
      lastPopoverShown: null,
      perPage: 10,
      currentPage: 1,
      gameFieldsInternal: [
        { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.dateAdjustedHypeFactor', label: 'Hype Factor', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'status', label: 'Status', thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'select', label: '', thClass: 'bg-primary' }
      ],
      sortBy: 'dateAdjustedHypeFactor',
      sortDesc: true
    };
  },
  computed: {
    rows() {
      return this.possibleGames.length;
    },
    gameRows() {
      let gameRows = this.possibleGames;
      if (!gameRows) {
        return [];
      }
      return gameRows;
    },
    gameFields() {
      return this.gameFieldsInternal;
    }
  },
  methods: {
    selectGame(masterGame) {
      this.selectedMasterGame = masterGame;
      this.$emit('input', this.selectedMasterGame);
    },
    /**
     * Determines the bid eligibility status for a given master game based on its release date.
     *
     * @param {Object} masterGame - The master game object to check eligibility for.
     * @param {string|Date} masterGame.releaseDate - The release date of the master game.
     * @returns {string} - Returns one of the following statuses:
     *   - 'tooLate': The game's release date is before the next open bidding time.
     *   - 'lastChance': The game's release date is between the next and following open bidding times.
     *   - '': The game is eligible for bidding beyond the following open bidding time.
     */
    getBidEligibility(masterGame) {
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
        return '';
      }
    },
    /**
     * Retrieves the next open bidding time based on the league's pickup system.
     * - If the pickup system is "SecretBidding", returns the next bid lock time.
     * - For "SemiPublicBidding" and "SemiPublicBiddingSecretCounterPicks", returns the next public bidding time.
     * Utilizes Vuex store getters and state to determine the appropriate time.
     *
     * @returns {Date} The next relevant bidding time as a Date object.
     */
    retrieveNextOpenBiddingTime() {
      const bidTimes = this.$store.getters.bidTimes;
      const pickupSystem = this.$store.state.leagueYear?.options?.pickupSystem;
      if (pickupSystem === 'SecretBidding') {
        return new Date(bidTimes.nextBidLockTime);
      }
      return new Date(bidTimes.nextPublicBiddingTime);
    },
    /**
     * Calculates the following open bidding time based on the league's pickup system.
     * - Retrieves the next open bidding time using `retrieveNextOpenBiddingTime()`.
     * - Determines the interval in days: 7 for "SecretBidding", 14 otherwise.
     * - Returns a new Date object representing the next open bidding time plus the interval.
     *
     * @returns {Date} The Date of the following open bidding time.
     */
    retrieveFollowingOpenBiddingTime() {
      const nextBidTime = this.retrieveNextOpenBiddingTime();
      const pickupSystem = this.$store.state.leagueYear?.options?.pickupSystem;
      const intervalDays = pickupSystem === 'SecretBidding' ? 7 : 14;

      return new Date(nextBidTime.getTime() + intervalDays * 24 * 60 * 60 * 1000);
    }
  }
};
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
