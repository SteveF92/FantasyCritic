<template>
  <b-modal id="playerDraftCounterPickForm" ref="playerDraftCounterPickFormRef" title="Select Counter-Pick" hide-footer @hidden="clearData" @show="getPossibleCounterPicks">
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
        <input type="submit" class="btn btn-primary full-width-button" value="Select Game as Counter-Pick" :disabled="isBusy" />
      </div>
    </form>
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
    }
  }
};
</script>
