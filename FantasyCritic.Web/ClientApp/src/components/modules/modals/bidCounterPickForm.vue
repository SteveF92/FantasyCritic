<template>
  <b-modal id="bidCounterPickForm" ref="bidCounterPickFormRef" size="lg" title="Make a Counter Pick Bid" hide-footer @hidden="clearData" @show="getPossibleCounterPicks">
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{errorInfo}}
    </div>
    <p>
      You can use this form to place a bid on a game.
      <br />
      Bids are processed on Saturday Nights. See the FAQ for more info.
    </p>

    <div class="alert alert-danger" v-show="publisherSlotsAreFilled">
      You have already filled all of your counter pick slots!
    </div>

    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <div class="search-results">
        <ValidationObserver v-slot="{ invalid }">
          <h3 class="text-black">Available Counter Picks</h3>
          <b-form-select v-model="bidCounterPick">
            <option v-for="publisherGame in possibleCounterPicks" v-bind:value="publisherGame">
              {{ publisherGame.gameName }}
            </option>
          </b-form-select>

          <label for="bidAmount" class="control-label">Bid Amount (Remaining: {{leagueYear.userPublisher.budget | money}})</label>

          <ValidationProvider rules="required|integer" v-slot="{ errors }">
            <input v-model="bidAmount" id="bidAmount" name="bidAmount" type="number" class="form-control input" />
            <span class="text-danger">{{ errors[0] }}</span>
          </ValidationProvider>

          <div v-show="counterPickInvalid" class="alert alert-warning" role="alert">
            Unfortunately, you cannot make a counter pick bid for a game that is not linked to a master game.
          </div>

          <b-button variant="primary" v-on:click="bidGame" class="add-game-button" v-if="formIsValid" :disabled="isBusy || counterPickInvalid">{{bidButtonText}}</b-button>
          <div v-if="bidResult && !bidResult.success" class="alert bid-error alert-danger">
            <h3 class="alert-heading">Error!</h3>
            <ul>
              <li v-for="error in bidResult.errors">{{error}}</li>
            </ul>
          </div>
        </ValidationObserver>
      </div>
    </form>
  </b-modal>
</template>

<script>
import Vue from 'vue';
import axios from 'axios';
import PossibleMasterGamesTable from '@/components/modules/possibleMasterGamesTable';
import MasterGameSummary from '@/components/modules/masterGameSummary';
import SearchSlotTypeBadge from '@/components/modules/gameTables/searchSlotTypeBadge';

export default {
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
  components: {
    PossibleMasterGamesTable,
    MasterGameSummary,
    SearchSlotTypeBadge
  },
  computed: {
    formIsValid() {
      return !!this.bidCounterPick;
    },
    publisherSlotsAreFilled() {
      let userGames = this.leagueYear.userPublisher.games;
      let counterPickSlots = this.leagueYear.counterPicks;
      let counterPicks = _.filter(userGames, { 'counterPick': true });
      return counterPicks.length >= counterPickSlots;
    },
    droppableGames() {
      return _.filter(this.publisher.games, { 'counterPick': false });
    },
    bidButtonText() {
      return 'Place Counter Pick Bid';
    },
    availableCounterPicks() {
      return this.leagueYear.availableCounterPicks;
    },
    counterPickInvalid() {
      return this.bidCounterPick && !this.bidCounterPick.masterGame;
    }
  },
  props: ['leagueYear', 'publisher'],
  methods: {
    getPossibleCounterPicks() {
      this.clearData();
      this.isBusy = true;
      axios
        .get('/api/league/PossibleCounterPicks?publisherID=' + this.publisher.publisherID)
        .then(response => {
          this.possibleCounterPicks = response.data;
          this.isBusy = false;
          this.counterPicking = true;
        })
        .catch(response => {
          this.isBusy = false;
        });
    },
    bidGame() {
      var request = {
        publisherID: this.leagueYear.userPublisher.publisherID,
        masterGameID: this.bidCounterPick.masterGame.masterGameID,
        bidAmount: this.bidAmount,
        counterPick: true
      };

      axios
        .post('/api/league/MakePickupBid', request)
        .then(response => {
          this.bidResult = response.data;
          if (!this.bidResult.success) {
            return;
          }
          this.$refs.bidCounterPickFormRef.hide();
          var bidInfo = {
            gameName: this.bidCounterPick.masterGame.gameName,
            bidAmount: this.bidAmount
          };
          this.$emit('gameBid', bidInfo);
          this.clearData();
        })
        .catch(response => {
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
  },
  mounted() {
    this.getPossibleCounterPicks();
  }
};
</script>
<style scoped>
  .add-game-button {
    margin-top: 20px;
    width: 100%;
  }

  .bid-error {
    margin-top: 10px;
  }

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
