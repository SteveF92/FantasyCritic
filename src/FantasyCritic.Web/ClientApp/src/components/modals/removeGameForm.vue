<template>
  <div>
    <form class="form-horizontal" hide-footer>
      <b-modal id="removePublisherGame" size="lg" ref="removePublisherGameRef" title="Remove Publisher Game" hide-footer @hidden="clearData">
        <div class="alert alert-warning">
          Warning! This feature is intended to fix mistakes and other exceptional circumstances. In general, managers should not be removing games from player's rosters.
        </div>
        <div class="form-group">
          <label for="claimPublisher" class="control-label">Publisher</label>
          <b-form-select v-model="removeGamePublisher">
            <option v-for="publisher in leagueYear.publishers" v-bind:value="publisher" :key="publisher.publisherID">
              {{ publisher.publisherName }}
            </option>
          </b-form-select>
          <div v-if="removeGamePublisher">
            <label for="removeGame" class="control-label">Game</label>
            <b-form-select v-model="removeGame">
              <option v-for="publisherGame in removeGamePublisher.games" v-bind:value="publisherGame" :key="publisherGame.publisherGameID">
                {{ publisherGame.gameName }}
              </option>
            </b-form-select>
          </div>
        </div>

        <div v-if="removeGame">
          <b-button variant="primary" class="add-game-button" v-on:click="removePublisherGame">Remove Game</b-button>
          <div v-if="errorInfo" class="alert alert-danger remove-error">
            <h3 class="alert-heading">Error!</h3>
            <p class="text-white">{{ errorInfo }}</p>
          </div>
        </div>
      </b-modal>
    </form>
  </div>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      removeGamePublisher: null,
      removeGame: null,
      errorInfo: ''
    };
  },
  methods: {
    removePublisherGame() {
      var model = {
        publisherGameID: this.removeGame.publisherGameID,
        publisherID: this.removeGamePublisher.publisherID
      };
      axios
        .post('/api/leagueManager/RemovePublisherGame', model)
        .then(() => {
          this.$refs.removePublisherGameRef.hide();
          this.notifyAction(this.removeGame.gameName + ' removed from ' + this.removeGamePublisher.publisherName);
          this.clearData();
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
        });
    },
    clearData() {
      this.removeGamePublisher = null;
      this.removeGame = null;
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

.remove-error {
  margin-top: 15px;
}
</style>
