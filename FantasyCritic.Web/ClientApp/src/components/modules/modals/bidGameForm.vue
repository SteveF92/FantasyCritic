<template>
  <b-modal id="bidGameForm" ref="bidGameFormRef" size="lg" title="Make a Bid" hide-footer @hidden="clearData">
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{errorInfo}}
    </div>
    <p>
      You can use this form to place a bid on a game.
      <br />
      Bids are processed on Monday Nights. See the FAQ for more info.
    </p>

    <div class="alert alert-warning" v-show="publisherSlotsAreFilled">Warning! You have already filled all of your game slots.
    You can still make bids, but you must drop a game before bids are processed, or the bid will not succeed.</div>

    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <label for="bidGameName" class="control-label">Game Name</label>
      <div class="input-group game-search-input">
        <input v-model="bidGameName" id="bidGameName" name="bidGameName" type="text" class="form-control input" />
        <span class="input-group-btn">
          <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
        </span>
      </div>

      <b-button v-show="!showingTopAvailable || bidMasterGame" variant="secondary" v-on:click="getTopGames" class="show-top-button">Show Top Available Games</b-button>

      <div v-if="!bidMasterGame">
        <h3 class="text-black" v-show="showingTopAvailable">Top Available Games</h3>
        <h3 class="text-black" v-show="!showingTopAvailable && possibleMasterGames && possibleMasterGames.length > 0">Search Results</h3>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="bidMasterGame" :possibleGames="possibleMasterGames"
                                  v-on:input="newGameSelected"></possibleMasterGamesTable>
      </div>

      <div class="alert alert-info" v-show="searched && !bidMasterGame && possibleMasterGames.length === 0">
        <div class="row">
          <span class="col-12 col-md-7">No games were found.</span>
        </div>
      </div>

      <div v-if="bidMasterGame">
        <ValidationObserver v-slot="{ invalid }">
          <h3 for="bidMasterGame" class="selected-game text-black">Selected Game:</h3>
          <masterGameSummary :masterGame="bidMasterGame"></masterGameSummary>
          <hr />
          <div class="form-group">
            <label for="bidAmount" class="control-label">Bid Amount (Remaining: {{leagueYear.userPublisher.budget | money}})</label>

            <ValidationProvider rules="required|integer" v-slot="{ errors }">
              <input v-model="bidAmount" id="bidAmount" name="bidAmount" type="number" class="form-control input" />
              <span class="text-danger">{{ errors[0] }}</span>
            </ValidationProvider>
          </div>
          <b-button variant="primary" v-on:click="bidGame" class="add-game-button" v-if="formIsValid" :disabled="isBusy || invalid">Place Bid</b-button>
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

export default {
  data() {
    return {
      bidGameName: '',
      bidMasterGame: null,
      bidAmount: 0,
      bidResult: null,
      possibleMasterGames: [],
      errorInfo: '',
      showingTopAvailable: false,
      searched: false,
      isBusy: false
    };
  },
  components: {
    PossibleMasterGamesTable,
    MasterGameSummary
  },
  computed: {
    formIsValid() {
      return (this.bidMasterGame);
    },
    publisherSlotsAreFilled() {
      let userGames = this.leagueYear.userPublisher.games;
      let standardGameSlots = this.leagueYear.standardGames;
      let userStandardGames = _.filter(userGames, { 'counterPick': false });
      return userStandardGames.length >= standardGameSlots;
    }
  },
  props: ['leagueYear'],
  methods: {
    searchGame() {
      this.clearDataExceptSearch();
      this.isBusy = true;
      axios
        .get('/api/league/PossibleMasterGames?gameName=' + this.bidGameName + '&year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID)
        .then(response => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.searched = true;
        })
        .catch(response => {
          this.isBusy = false;
        });
    },
    getTopGames() {
      this.clearDataExceptSearch();
      this.isBusy = true;
      axios
        .get('/api/league/TopAvailableGames?year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID)
        .then(response => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.showingTopAvailable = true;
        })
        .catch(response => {
          this.isBusy = false;
        });
    },
    bidGame() {
      var request = {
        publisherID: this.leagueYear.userPublisher.publisherID,
        masterGameID: this.bidMasterGame.masterGameID,
        bidAmount: this.bidAmount
      };

      axios
        .post('/api/league/MakePickupBid', request)
        .then(response => {
          this.bidResult = response.data;
          if (!this.bidResult.success) {
            return;
          }
          this.$refs.bidGameFormRef.hide();
          var bidInfo = {
            gameName: this.bidMasterGame.gameName,
            bidAmount: this.bidAmount
          };
          this.$emit('gameBid', bidInfo);
          this.bidResult = null;
          this.bidGameName = '';
          this.bidMasterGame = null;
          this.bidAmount = 0;
          this.possibleMasterGames = [];
        })
        .catch(response => {
          this.errorInfo = response.response.data;
        });
    },
    clearDataExceptSearch() {
      this.bidResult = null;
      this.bidMasterGame = null;
      this.bidAmount = 0;
      this.possibleMasterGames = [];
      this.searched = false;
      this.showingTopAvailable = false;
      this.isBusy = false;
    },
    clearData() {
      this.clearDataExceptSearch();
      this.bidGameName = '';
    },
    newGameSelected() {
      this.bidResult = null;
    }
  }
};
</script>
<style scoped>
  .add-game-button {
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
</style>
