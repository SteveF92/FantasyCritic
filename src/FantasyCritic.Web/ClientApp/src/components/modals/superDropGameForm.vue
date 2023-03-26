<template>
  <b-modal id="superDropGameForm" ref="superDropGameFormRef" size="lg" title="Super Drop a Game" hide-footer @hidden="clearData">
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{ errorInfo }}
    </div>
    <div class="alert alert-info">
      Super drops work differently from normal drops. You can drop any game you wish, and drops happen immediately, rather than having to wait until Saturday.
      <br />
      <br />
      <template v-if="userPublisher.superDropsAvailable > 1">You currently have {{ userPublisher.superDropsAvailable }} super drops.</template>
      <template v-if="userPublisher.superDropsAvailable === 1">You currently have 1 super drop.</template>
      <template v-if="!userPublisher.superDropsAvailable">You do not currently have any super drops.</template>
    </div>
    <form class="form-horizontal" hide-footer @submit.prevent="dropGame">
      <div class="form-group">
        <label for="gameToDrop" class="control-label">Game</label>
        <b-form-select v-model="gameToDrop">
          <option v-for="publisherGame in userPublisher.games" :key="publisherGame.publisherGameID" :value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>

      <div v-if="gameToDrop">
        <input type="submit" class="btn btn-danger full-width-button" value="Super Drop Game" :disabled="isBusy" />
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
    }
  },
  methods: {
    async dropGame() {
      var request = {
        publisherID: this.userPublisher.publisherID,
        publisherGameID: this.gameToDrop.publisherGameID
      };

      this.isBusy = true;
      try {
        const response = await axios.post('/api/league/UseSuperDrop', request);
        this.isBusy = false;
        this.dropResult = response.data;
        if (!this.dropResult.success) {
          return;
        }

        this.notifyAction(this.gameToDrop.gameName + ' has been super dropped.');
        this.$refs.superDropGameFormRef.hide();
        this.clearData();
      } catch (error) {
        this.isBusy = false;
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.dropResult = null;
      this.gameToDrop = null;
    }
  }
};
</script>
