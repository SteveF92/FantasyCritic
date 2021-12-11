<template>
  <b-modal id="playerDraftCounterPickForm" ref="playerDraftCounterPickFormRef" title="Select Counter-Pick" hide-footer
           @hidden="clearData" @show="getPossibleCounterPicks">
    <form class="form-horizontal" v-on:submit.prevent="selectCounterPick" hide-footer>
        <div class="form-group">
            <label for="selectedCounterPick" class="control-label">Game</label>
            <b-form-select v-model="selectedCounterPick">
              <option v-for="publisherGame in possibleCounterPicks" v-bind:value="publisherGame">
                {{ publisherGame.gameName }}
              </option>
            </b-form-select>
        </div>

        <div v-if="selectedCounterPick">
          <input type="submit" class="btn btn-primary add-game-button" value="Select Game as Counter-Pick" :disabled="isBusy" />
        </div>
    </form>
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
      isBusy: false
    };
  },
  props: ['userPublisher'],
  methods: {
    selectCounterPick() {
      this.isBusy = true;
      var request = {
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
        .then(response => {
          this.draftResult = response.data;
          if (!this.draftResult.success) {
            return;
          }
          this.$refs.playerDraftCounterPickFormRef.hide();
          var draftInfo = {
            gameName: this.selectedCounterPick.gameName
          };
          this.$emit('counterPickDrafted', draftInfo);
          this.selectedCounterPick = null;
        })
        .catch(response => {

        });
    },
    getPossibleCounterPicks() {
      axios
        .get('/api/league/PossibleCounterPicks?publisherID=' + this.userPublisher.publisherID)
        .then(response => {
          this.possibleCounterPicks = response.data;
          this.isBusy = false;
          this.counterPicking = true;
        })
        .catch(response => {
          this.isBusy = false;
        });
    },
    clearData() {
      this.isBusy = false;
      this.possibleCounterPicks = [];
      this.selectedCounterPick = null;
    }
  },
  mounted() {
    this.getPossibleCounterPicks();
  }
};
</script>
<style scoped>
.add-game-button{
  width: 100%;
}
</style>
