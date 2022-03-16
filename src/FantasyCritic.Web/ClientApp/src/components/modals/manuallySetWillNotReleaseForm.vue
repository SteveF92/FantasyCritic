<template>
  <div>
    <form class="form-horizontal" hide-footer>
      <b-modal id="manuallySetWillNotRelease" ref="manuallyScorePublisherGameRef" title="Manually Set Will Not Release" hide-footer @hidden="clearData">
        <div class="alert alert-info">This form can be used when your league is extremely confident that a player's game will not release this year, even though it is not confirmed.</div>
        <div class="form-group">
          <label for="claimPublisher" class="control-label">Publisher</label>
          <b-form-select v-model="manuallySetWillNotReleasePublisher">
            <option v-for="publisher in leagueYear.publishers" v-bind:value="publisher">
              {{ publisher.publisherName }}
            </option>
          </b-form-select>
          <div v-if="manuallySetWillNotReleasePublisher">
            <label for="manuallySetWillNotReleaseGame" class="control-label">Game</label>
            <b-form-select v-model="manuallySetWillNotReleaseGame">
              <option v-for="publisherGame in manuallySetWillNotReleasePublisher.games" v-bind:value="publisherGame">
                {{ publisherGame.gameName }}
              </option>
            </b-form-select>
          </div>
          <br />
          <div v-if="manuallySetWillNotReleaseGame">
            <div v-if="manuallySetWillNotReleaseGame.manualWillNotRelease">
              <b-button variant="primary" class="add-game-button" v-on:click="manuallySetWillNotRelease(false)">Clear manual 'Will not Release'</b-button>
            </div>
            <div v-if="!manuallySetWillNotReleaseGame.manualWillNotRelease">
              <b-button variant="primary" class="add-game-button" v-on:click="manuallySetWillNotRelease(true)">Set game as 'Will not Release'</b-button>
            </div>
          </div>
        </div>
      </b-modal>
    </form>
  </div>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';

export default {
  data() {
    return {
      manuallySetWillNotReleasePublisher: null,
      manuallySetWillNotReleaseGame: null,
      errorInfo: ''
    };
  },
  props: ['leagueYear'],
  methods: {
    manuallySetWillNotRelease(willNotRelease) {
      var model = {
        publisherGameID: this.manuallySetWillNotReleaseGame.publisherGameID,
        publisherID: this.manuallySetWillNotReleasePublisher.publisherID,
        willNotRelease: willNotRelease
      };
      axios
        .post('/api/leagueManager/ManuallySetWillNotRelease', model)
        .then((response) => {
          this.$refs.manuallyScorePublisherGameRef.hide();
          this.$emit('gameWillNotReleaseSet');
          this.clearData();
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
        });
    },
    clearData() {
      this.manuallySetWillNotReleasePublisher = null;
      this.manuallySetWillNotReleaseGame = null;
    }
  }
};
</script>
<style scoped>
.add-game-button {
  width: 100%;
}

.claim-error {
  margin-top: 10px;
}

.game-search-input {
  margin-bottom: 15px;
}

.remove-manual-score-checkbox {
  margin-left: 15px;
  margin-top: 8px;
}
</style>
