<template>
  <div>
    <div v-if="games && games.length > 0">
      <b-table v-model:sort-by="sortBy" :items="games" :fields="gamesFields" bordered striped responsive small>
        <template #cell(masterGame.gameName)="data">
          <span class="game-name-column">
            <span v-if="data.item.counterPick" v-b-popover.hover.focus="counterPickText" class="badge tag-badge counter-pick-badge">CPK</span>
            <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
            <b-popover v-if="data.item.eligibilityErrors && data.item.eligibilityErrors.length > 0" title="This bid will fail!">
              <template #target>
                <font-awesome-icon class="bid-will-fail" color="white" icon="exclamation-triangle" />
              </template>
              This bid will fail for the below reasons:
              <br />
              <br />
              <h5>Errors</h5>
              <ul>
                <li v-for="error in data.item.eligibilityErrors" :key="error">{{ error }}</li>
              </ul>
            </b-popover>
          </span>
        </template>
        <template #cell(masterGame.maximumReleaseDate)="data">
          {{ getReleaseDate(data.item.masterGame) }}
        </template>
      </b-table>
    </div>
    <div v-else-if="!anyHiddenCounterPickBids">
      <div class="alert alert-info" role="alert">There are no games being bid upon this week.</div>
    </div>
    <div v-if="anyHiddenCounterPickBids" class="alert alert-warning hidden-counter-pick-alert" role="alert">
      <span class="badge tag-badge counter-pick-badge">CPK</span>
      <span>
        <strong>At least one game has a counter pick bid placed upon it.</strong>
        You are free to place additional counter pick bids until they process. The names of the games with counter pick bids placed will not be revealed until bids process.
      </span>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import type { BTableSortBy, TableField } from 'bootstrap-vue-next';
import { DateTime } from 'luxon';

import type { MasterGameYearViewModel, PublicBiddingMasterGameViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import type { PopoverContent } from '@/types/vueTypes';
import MasterGamePopover from '@/components/masterGamePopover.vue';

interface ActiveBidsData {
  sortBy: BTableSortBy[];
}

export default defineComponent({
  setup() {
    const counterPickText: PopoverContent = {
      title: 'Counter Pick Bid',
      body: 'This game is being bid on as a counter pick.'
    };

    const gamesFields: TableField<PublicBiddingMasterGameViewModel>[] = [
      { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
      { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' }
    ];

    return { counterPickText, gamesFields };
  },
  components: {
    MasterGamePopover
  },
  data(): ActiveBidsData {
    return {
      sortBy: [{ key: 'maximumReleaseDate', order: 'asc' }]
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear']),
    games(): PublicBiddingMasterGameViewModel[] {
      if (!this.requiredLeagueYear.publicBiddingGames) {
        return [];
      }

      return this.requiredLeagueYear.publicBiddingGames.masterGames;
    },
    anyHiddenCounterPickBids(): boolean {
      return !!this.requiredLeagueYear.publicBiddingGames?.anyHiddenCounterPickBids;
    }
  },
  methods: {
    getReleaseDate(game: MasterGameYearViewModel): string {
      if (game.releaseDate) {
        return DateTime.fromISO(game.releaseDate).toFormat('yyyy-MM-dd');
      }
      return game.estimatedReleaseDate + ' (Estimated)';
    }
  }
});
</script>

<style scoped>
.game-name-column {
  display: flex;
  align-items: center;
}

.popover-header {
  color: black;
}

.counter-pick-badge {
  background-color: #aa1e1e;
  color: white;
  margin-right: 8px;
}

.hidden-counter-pick-alert {
  display: flex;
  align-items: baseline;
}

.bid-will-fail {
  margin-left: 5px;
}
</style>
