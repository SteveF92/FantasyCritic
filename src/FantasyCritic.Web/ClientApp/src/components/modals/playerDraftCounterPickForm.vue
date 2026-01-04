<template>
  <b-modal id="playerDraftCounterPickForm" ref="playerDraftCounterPickFormRef" size="lg" title="Select Counter Pick" hide-footer @hidden="clearData" @show="getPossibleCounterPicks">
    <form class="form-horizontal" hide-footer @submit.prevent="selectCounterPick">
      <div v-if="isBusy" class="game-list-spinner">
        <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: 'black' }" />
      </div>

      <div v-if="possibleCounterPicks.length > 0">
        <h3 class="text-black">Available Counter Picks</h3>
        <possibleCounterPicksTable v-model="selectedCounterPick" :possible-games="possibleCounterPicks" @input="newGameSelected"></possibleCounterPicksTable>
      </div>

      <div v-if="possibleCounterPicks.length === 0 && !isBusy" class="alert alert-info">
        No games are available for counter picks.
      </div>

      <div v-if="selectedCounterPick">
        <h3 v-show="selectedCounterPick.masterGame" for="selectedCounterPick" class="selected-game text-black">Selected Game:</h3>
        <masterGameSummary v-if="selectedCounterPick.masterGame" :master-game="selectedCounterPick.masterGame"></masterGameSummary>
        <div v-if="!selectedCounterPick.masterGame" class="selected-game-info">
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

<script>
import axios from 'axios';
import PossibleCounterPicksTable from '@/components/possibleCounterPicksTable.vue';
import MasterGameSummary from '@/components/masterGameSummary.vue';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  components: {
    PossibleCounterPicksTable,
    MasterGameSummary
  },
  mixins: [LeagueMixin],
  data() {
    return {
      selectedCounterPick: null,
      possibleCounterPicks: [],
      draftResult: null,
      isBusy: false
    };
  },
  methods: {
    selectCounterPick() {
      this.isBusy = true;
      let request = {
        publisherID: this.userPublisher.publisherID,
        gameName: this.selectedCounterPick.gameName,
        counterPick: true,
        masterGameID: null
      };

      if (this.selectedCounterPick.masterGame) {
        request.masterGameID = this.selectedCounterPick.masterGame.masterGameID;
      }

      axios
        .post('/api/league/DraftGame', request)
        .then((response) => {
          this.draftResult = response.data;
          this.isBusy = false;
          if (!this.draftResult.success) {
            return;
          }

          this.$refs.playerDraftCounterPickFormRef.hide();
          this.notifyAction('You have selected ' + this.selectedCounterPick.gameName + ' as a counter pick.', false);
          this.selectedCounterPick = null;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    getPossibleCounterPicks() {
      this.isBusy = true;
      axios
        .get('/api/league/PossibleCounterPicks?publisherID=' + this.userPublisher.publisherID)
        .then((response) => {
          this.possibleCounterPicks = response.data;
          this.isBusy = false;
          this.counterPicking = true;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    clearData() {
      this.isBusy = false;
      this.possibleCounterPicks = [];
      this.selectedCounterPick = null;
    },
    newGameSelected() {
      this.draftResult = null;
    }
  }
};
</script>
<style scoped>
.game-list-spinner {
  text-align: center;
  margin: 20px 0;
}

.selected-game-info {
  margin: 15px 0;
  padding: 15px;
  background-color: #f8f9fa;
  border-radius: 5px;
  border: 1px solid #dee2e6;
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
