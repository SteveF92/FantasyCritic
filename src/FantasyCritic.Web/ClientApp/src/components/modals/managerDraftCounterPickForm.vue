<template>
  <b-modal id="managerDraftCounterPickForm" ref="managerDraftCounterPickFormRef" title="Select Counter Pick" hide-footer @hidden="clearData" @show="getPossibleCounterPicks">
    <div v-if="nextPublisherUp">
      <div class="form-group">
        <label for="nextPublisherUp" class="control-label">Select the next counter pick for publisher:</label>
        <label>
          <strong>{{ nextPublisherUp.publisherName }} (Display Name: {{ nextPublisherUp.playerName }})</strong>
        </label>
      </div>
      <form class="form-horizontal" hide-footer @submit.prevent="selectCounterPick">
        <div class="form-group">
          <label for="selectedCounterPick" class="control-label">Game</label>
          <b-form-select v-model="selectedCounterPick">
            <option v-for="publisherGame in possibleCounterPicks" :key="publisherGame.publisherGameID" :value="publisherGame">
              {{ publisherGame.gameName }}
            </option>
          </b-form-select>
        </div>

        <div v-if="draftResult && !draftResult.success" class="alert alert-danger bid-error">
          <h3 class="alert-heading">Error!</h3>
          <ul>
            <li v-for="error in draftResult.errors" :key="error">{{ error }}</li>
          </ul>
        </div>

        <div v-if="selectedCounterPick">
          <input type="submit" class="btn btn-primary full-width-button" value="Select Game as Counter-Pick" />
        </div>
      </form>
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      selectedCounterPick: null,
      possibleCounterPicks: [],
      draftResult: null
    };
  },
  methods: {
    selectCounterPick() {
      let request = {
        publisherID: this.nextPublisherUp.publisherID,
        gameName: this.selectedCounterPick.gameName,
        counterPick: true,
        masterGameID: null
      };

      if (this.selectedCounterPick.masterGame) {
        request.masterGameID = this.selectedCounterPick.masterGame.masterGameID;
      }

      axios
        .post('/api/leagueManager/ManagerDraftGame', request)
        .then((response) => {
          this.draftResult = response.data;
          this.isBusy = false;
          if (!this.draftResult.success) {
            return;
          }

          this.$refs.managerDraftCounterPickFormRef.hide();
          this.notifyAction(this.selectedCounterPick.gameName + ' selected as a counter pick by ' + this.nextPublisherUp.publisherName, false);
          this.selectedCounterPick = null;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    getPossibleCounterPicks() {
      axios
        .get('/api/league/PossibleCounterPicks?publisherID=' + this.nextPublisherUp.publisherID)
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
      this.selectedCounterPick = null;
      this.possibleCounterPicks = [];
    }
  }
};
</script>
<style scoped>
.draft-error {
  margin-top: 10px;
}
.game-search-input {
  margin-bottom: 15px;
}
.override-checkbox {
  margin-left: 10px;
  margin-top: 8px;
}
</style>
