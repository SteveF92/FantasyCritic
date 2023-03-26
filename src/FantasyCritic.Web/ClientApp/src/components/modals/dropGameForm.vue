<template>
  <b-modal id="dropGameForm" ref="dropGameFormRef" size="lg" title="Drop a Game" hide-footer @hidden="clearData">
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{ errorInfo }}
    </div>
    <p>
      You can use this form to request to drop a game.
      <br />
      Your league's settings determine how often you can do this.
      <br />
      Drop requests are processed on Saturday Nights. See the FAQ for more info.
    </p>
    <form class="form-horizontal" hide-footer @submit.prevent="dropGame">
      <div class="form-group">
        <label for="gameToDrop" class="control-label">Game</label>
        <b-form-select v-model="gameToDrop">
          <option v-for="publisherGame in droppableGames" :key="publisherGame.publisherGameID" :value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>

      <div v-if="gameToDrop">
        <input type="submit" class="btn btn-danger full-width-button" value="Make Drop Game Request" :disabled="isBusy" />
      </div>
      <div v-if="dropResult && !dropResult.success" class="alert alert-danger bid-error">
        <h3 class="alert-heading">Error!</h3>
        <ul>
          <li v-for="error in dropResult.errors" :key="error">{{ error }}</li>
        </ul>
      </div>
    </form>
  </b-modal>
</template>

<script>
import axios from 'axios';
import _ from 'lodash';

import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      dropResult: null,
      gameToDrop: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    formIsValid() {
      return this.dropMasterGame;
    },
    droppableGames() {
      return _.filter(this.userPublisher.games, { counterPick: false });
    }
  },
  methods: {
    dropGame() {
      var request = {
        publisherID: this.userPublisher.publisherID,
        publisherGameID: this.gameToDrop.publisherGameID
      };
      this.isBusy = true;
      axios
        .post('/api/league/MakeDropRequest', request)
        .then((response) => {
          this.isBusy = false;
          this.dropResult = response.data;
          if (!this.dropResult.success) {
            return;
          }

          this.notifyAction('Drop Request for ' + this.gameToDrop.gameName + ' was made.');
          this.$refs.dropGameFormRef.hide();
          this.clearData();
        })
        .catch((response) => {
          this.isBusy = false;
          this.errorInfo = response.response.data;
        });
    },
    clearData() {
      this.dropResult = null;
      this.gameToDrop = null;
    }
  }
};
</script>
