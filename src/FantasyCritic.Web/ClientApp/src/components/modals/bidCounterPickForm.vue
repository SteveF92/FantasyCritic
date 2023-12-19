<template>
  <b-modal id="bidCounterPickForm" ref="bidCounterPickFormRef" size="lg" title="Make a Counter Pick Bid" hide-footer @hidden="clearData" @show="getPossibleCounterPicks">
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{ errorInfo }}
    </div>
    <p>
      You can use this form to place a bid on a game.
      <br />
      Bids are processed on Saturday Nights. See the FAQ for more info.
      <br />
      Games that have already released, or are confirmed not to release this year are not available to be counterpicked.
    </p>

    <div v-if="publisherSlotsAreFilled" class="alert alert-danger">You have already filled all of your counter pick slots!</div>

    <form method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
      <div class="search-results">
        <ValidationObserver>
          <h3 class="text-black">Available Counter Picks</h3>
          <b-form-select v-model="bidCounterPick">
            <option v-for="publisherGame in availableCounterPicks" :key="publisherGame.publisherGameID" :value="publisherGame">
              {{ publisherGame.gameName }}
            </option>
          </b-form-select>

          <label for="bidAmount" class="control-label">Bid Amount (Remaining: {{ userPublisher.budget | money(0) }})</label>

          <ValidationProvider v-slot="{ errors }" rules="required|integer">
            <input id="bidAmount" v-model="bidAmount" name="bidAmount" type="number" class="form-control input" />
            <span class="text-danger">{{ errors[0] }}</span>
          </ValidationProvider>

          <div v-show="counterPickInvalid" class="alert alert-warning" role="alert">Unfortunately, you cannot make a counter pick bid for a game that is not linked to a master game.</div>

          <b-button v-if="formIsValid" variant="primary" class="full-width-button" :disabled="isBusy || counterPickInvalid" @click="bidGame">{{ bidButtonText }}</b-button>
          <div v-if="bidResult && !bidResult.success" class="alert alert-danger bid-error">
            <h3 class="alert-heading">Error!</h3>
            <ul>
              <li v-for="error in bidResult.errors" :key="error">{{ error }}</li>
            </ul>
          </div>
        </ValidationObserver>
      </div>
    </form>
  </b-modal>
</template>

<script>
import _ from 'lodash';

import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      bidCounterPick: null,
      bidAmount: 0,
      bidResult: null,
      possibleCounterPicks: [],
      errorInfo: '',
      isBusy: false
    };
  },
  computed: {
    formIsValid() {
      return !!this.bidCounterPick;
    },
    publisherSlotsAreFilled() {
      let userGames = this.userPublisher.games;
      let counterPickSlots = this.leagueYear.settings.counterPicks;
      let counterPicks = _.filter(userGames, { counterPick: true });
      return counterPicks.length >= counterPickSlots;
    },
    droppableGames() {
      return _.filter(this.publisher.games, { counterPick: false });
    },
    bidButtonText() {
      return 'Place Counter Pick Bid';
    },
    counterPickInvalid() {
      return this.bidCounterPick && !this.bidCounterPick.masterGame;
    },
    availableCounterPicks() {
      return _.filter(this.possibleCounterPicks, (x) => x.willRelease && !x.released);
    }
  },
  methods: {
    getPossibleCounterPicks() {
      this.clearData();
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
    bidGame() {
      const request = {
        publisherID: this.userPublisher.publisherID,
        masterGameID: this.bidCounterPick.masterGame.masterGameID,
        bidAmount: this.bidAmount,
        counterPick: true
      };

      axios
        .post('/api/league/MakePickupBid', request)
        .then((response) => {
          this.bidResult = response.data;
          if (!this.bidResult.success) {
            return;
          }

          this.$refs.bidCounterPickFormRef.hide();
          this.notifyAction('Bid for ' + this.bidCounterPick.masterGame.gameName + ' for $' + this.bidAmount + ' was made.');
          this.clearData();
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
        });
    },
    clearData() {
      this.bidCounterPick = null;
      this.bidResult = null;
      this.bidAmount = 0;
      this.isBusy = false;
      this.conditionalDrop = false;
    },
    newGameSelected() {
      this.bidCounterPick = null;
    }
  }
};
</script>
<style scoped>
.game-search-input {
  margin-bottom: 15px;
}

.override-checkbox {
  margin-left: 10px;
  margin-top: 8px;
}

.search-results {
  margin-top: 20px;
}

.spinner {
  margin-top: 20px;
  text-align: center;
}

.search-tags {
  display: flex;
  padding: 5px;
  background: rgba(50, 50, 50, 0.7);
  border-radius: 5px;
  justify-content: space-around;
}
</style>
