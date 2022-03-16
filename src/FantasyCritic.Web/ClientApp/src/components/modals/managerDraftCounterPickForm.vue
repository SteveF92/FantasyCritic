<template>
  <b-modal id="managerDraftCounterPickForm" ref="managerDraftCounterPickFormRef" title="Select Counter Pick" hide-footer @hidden="clearData" @show="getPossibleCounterPicks">
    <div v-if="nextPublisherUp">
      <div class="form-group">
        <label for="nextPublisherUp" class="control-label">Select the next counter pick for publisher:</label>
        <label>
          <strong>{{ nextPublisherUp.publisherName }} (Display Name: {{ nextPublisherUp.playerName }})</strong>
        </label>
      </div>
      <form class="form-horizontal" v-on:submit.prevent="selectCounterPick" hide-footer>
        <div class="form-group">
          <label for="selectedCounterPick" class="control-label">Game</label>
          <b-form-select v-model="selectedCounterPick">
            <option v-for="publisherGame in possibleCounterPicks" v-bind:value="publisherGame">
              {{ publisherGame.gameName }}
            </option>
          </b-form-select>
        </div>

        <div v-if="draftResult && !draftResult.success" class="alert bid-error alert-danger">
          <h3 class="alert-heading">Error!</h3>
          <ul>
            <li v-for="error in draftResult.errors">{{ error }}</li>
          </ul>
        </div>

        <div v-if="selectedCounterPick">
          <input type="submit" class="btn btn-primary add-game-button" value="Select Game as Counter-Pick" />
        </div>
      </form>
    </div>
  </b-modal>
</template>

<script>
import Vue from 'vue';
import axios from 'axios';

export default {
  data() {
    return {
      selectedCounterPick: null,
      possibleCounterPicks: [],
      draftResult: null
    };
  },
  props: ['nextPublisherUp'],
  methods: {
    selectCounterPick() {
      var request = {
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
          var draftInfo = {
            gameName: this.selectedCounterPick.gameName,
            publisherName: this.nextPublisherUp.publisherName
          };
          this.$emit('counterPickDrafted', draftInfo);
          this.selectedCounterPick = null;
        })
        .catch((response) => {
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
        .catch((response) => {
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
.add-game-button {
  width: 100%;
}
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
