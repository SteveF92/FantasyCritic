<template>
  <b-modal id="player-draft-counter-pick-form" ref="playerDraftCounterPickFormRef" size="lg" title="Select Counter Pick" no-footer @hidden="clearData" @show="getPossibleCounterPicks">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <form class="form-horizontal" @submit.prevent="selectCounterPick">
      <div v-if="!selectedCounterPick && isBusy" class="game-list-spinner">
        <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: 'black' }" />
      </div>

      <div v-if="possibleCounterPicks.length > 0">
        <h3 class="text-black">Available Counter Picks</h3>
        <possibleCounterPicksTable v-model="selectedCounterPick" :possible-games="possibleCounterPicks" @update:model-value="newGameSelected"></possibleCounterPicksTable>
      </div>

      <div v-if="possibleCounterPicks.length === 0 && !isBusy && !errorInfo" class="alert alert-info">No games are available for counter picks.</div>

      <div v-if="selectedCounterPick">
        <h3 class="selected-game text-black">Selected Game:</h3>
        <masterGameSummary v-if="selectedCounterPick.masterGame" :master-game="selectedCounterPick.masterGame"></masterGameSummary>
        <div v-else class="text-black">
          <h4>{{ selectedCounterPick.gameName }}</h4>
          <div v-if="selectedCounterPick.publisherName">Currently owned by {{ selectedCounterPick.publisherName }}</div>
        </div>
        <b-button variant="primary" class="full-width-button" :disabled="isBusy" @click="selectCounterPick">Select Game as Counter Pick</b-button>
      </div>

      <div v-if="draftResult && !draftResult.success" class="alert alert-danger bid-error">
        <h3 class="alert-heading">Error!</h3>
        <ul>
          <li v-for="error in draftResult.errors" :key="error">{{ error }}</li>
        </ul>
      </div>
    </form>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueClient, type DraftGameRequest, type PlayerClaimResultViewModel, type PublisherGameViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { makeToast } from '@/utilities/toast';
import MasterGameSummary from '@/components/masterGameSummary.vue';
import PossibleCounterPicksTable from '@/components/possibleCounterPicksTable.vue';

interface PlayerDraftCounterPickFormData {
  selectedCounterPick: PublisherGameViewModel | null;
  possibleCounterPicks: PublisherGameViewModel[];
  draftResult: PlayerClaimResultViewModel | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  components: {
    PossibleCounterPicksTable,
    MasterGameSummary
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredUserPublisher'])
  },
  data(): PlayerDraftCounterPickFormData {
    return {
      selectedCounterPick: null,
      possibleCounterPicks: [],
      draftResult: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    async selectCounterPick(): Promise<void> {
      const counterPick = this.selectedCounterPick;
      if (!counterPick) {
        return;
      }

      const request: DraftGameRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        gameName: counterPick.gameName,
        counterPick: true,
        masterGameID: counterPick.masterGame?.masterGameID,
        allowIneligibleSlot: false
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        this.draftResult = await leagueClient.draftGame(request);
        if (!this.draftResult.success) {
          return;
        }

        (this.$refs.playerDraftCounterPickFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        makeToast('You have selected ' + counterPick.gameName + ' as a counter pick.');
        this.selectedCounterPick = null;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when selecting your counter pick.');
      } finally {
        this.isBusy = false;
      }
    },
    async getPossibleCounterPicks(): Promise<void> {
      this.isBusy = true;
      this.errorInfo = '';
      try {
        this.possibleCounterPicks = await leagueClient.possibleCounterPicks(this.requiredUserPublisher.publisherID);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to load counter picks.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.isBusy = false;
      this.possibleCounterPicks = [];
      this.selectedCounterPick = null;
      this.draftResult = null;
      this.errorInfo = '';
    },
    newGameSelected(): void {
      this.draftResult = null;
    }
  }
});
</script>

<style scoped>
.game-list-spinner {
  text-align: center;
  margin: 20px 0;
}

.selected-game {
  margin-top: 15px;
}

.full-width-button {
  width: 100%;
  margin-top: 10px;
}

.bid-error {
  margin-top: 15px;
}
</style>
