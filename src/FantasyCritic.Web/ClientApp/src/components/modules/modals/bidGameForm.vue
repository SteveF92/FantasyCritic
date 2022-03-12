<template>
  <b-modal id="bidGameForm" ref="bidGameFormRef" size="lg" title="Make a Bid" hide-footer @hidden="clearData">
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{ errorInfo }}
    </div>
    <p>
      You can use this form to place a bid on a game.
      <br />
      Bids are processed on Saturday Nights. See the FAQ for more info.
    </p>

    <div class="alert alert-warning" v-show="publisherSlotsAreFilled">
      Warning! You have already filled all of your game slots. You can still make bids, but you must drop a game first. You can use the conditional drop feature for this.
    </div>

    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <label for="bidGameName" class="control-label">Game Name</label>
      <div class="input-group game-search-input">
        <input v-model="bidGameName" id="bidGameName" name="bidGameName" type="text" class="form-control input" />
        <span class="input-group-btn">
          <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
        </span>
      </div>

      <div v-if="!leagueYear.hasSpecialSlots">
        <b-button variant="secondary" v-on:click="getTopGames" class="show-top-button">Show Top Available Games</b-button>
        <b-button variant="secondary" v-on:click="getQueuedGames" class="show-top-button">Show My Watchlist</b-button>
      </div>
      <div v-else>
        <b-button variant="secondary" v-on:click="getQueuedGames" class="show-top-button">Show My Watchlist</b-button>
        <h5 class="text-black">Search by Slot</h5>
        <span class="search-tags">
          <searchSlotTypeBadge :gameSlot="leagueYear.slotInfo.overallSlot" name="ALL" v-on:click.native="getTopGames"></searchSlotTypeBadge>
          <searchSlotTypeBadge :gameSlot="leagueYear.slotInfo.regularSlot" name="REG" v-on:click.native="getGamesForSlot(leagueYear.slotInfo.regularSlot)"></searchSlotTypeBadge>
          <searchSlotTypeBadge v-for="specialSlot in leagueYear.slotInfo.specialSlots" :gameSlot="specialSlot" v-on:click.native="getGamesForSlot(specialSlot)"></searchSlotTypeBadge>
        </span>
      </div>

      <div v-show="isBusy" class="spinner">
        <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#000000' }" />
      </div>

      <div class="search-results">
        <div v-if="!bidMasterGame">
          <h3 class="text-black" v-show="showingTopAvailable">Top Available Games</h3>
          <h3 class="text-black" v-show="showingQueuedGames">Watchlist</h3>
          <h3 class="text-black" v-show="!showingTopAvailable && !showingQueuedGames && possibleMasterGames && possibleMasterGames.length > 0">Search Results</h3>
          <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="bidMasterGame" :possibleGames="possibleMasterGames" v-on:input="newGameSelected"></possibleMasterGamesTable>
        </div>
        <div v-else>
          <ValidationObserver v-slot="{ invalid }">
            <h3 for="bidMasterGame" class="selected-game text-black">Selected Game:</h3>
            <masterGameSummary :masterGame="bidMasterGame"></masterGameSummary>
            <hr />
            <div class="form-group">
              <label for="bidAmount" class="control-label">Bid Amount (Remaining: {{ leagueYear.userPublisher.budget | money }})</label>

              <ValidationProvider rules="required|integer" v-slot="{ errors }">
                <input v-model="bidAmount" id="bidAmount" name="bidAmount" type="number" class="form-control input" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </div>
            <div class="form-group">
              <label for="conditionalDrop" class="control-label">
                Conditional Drop (Optional)
                <font-awesome-icon icon="info-circle" v-b-popover.hover="'You can use this to drop a game only if your bid succeeds.'" />
              </label>
              <b-form-select v-model="conditionalDrop">
                <option v-for="publisherGame in droppableGames" v-bind:value="publisherGame">
                  {{ publisherGame.gameName }}
                </option>
              </b-form-select>
            </div>
            <b-button variant="primary" v-on:click="bidGame" class="add-game-button" v-if="formIsValid" :disabled="requestIsBusy">{{ bidButtonText }}</b-button>
            <div v-if="bidResult && !bidResult.success" class="alert bid-error alert-danger">
              <h3 class="alert-heading">Error!</h3>
              <ul>
                <li v-for="error in bidResult.errors">{{ error }}</li>
              </ul>
            </div>
          </ValidationObserver>
        </div>
      </div>

      <div class="alert alert-info" v-show="searched && !bidMasterGame && possibleMasterGames.length === 0">
        <div class="row">
          <span class="col-12 col-md-7">No games were found.</span>
        </div>
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
      bidGameName: '',
      bidMasterGame: null,
      bidAmount: 0,
      bidResult: null,
      conditionalDrop: null,
      possibleMasterGames: [],
      errorInfo: '',
      showingTopAvailable: false,
      showingQueuedGames: false,
      searched: false,
      isBusy: false,
      requestIsBusy: false
    };
  },
  components: {
    PossibleMasterGamesTable,
    MasterGameSummary,
    SearchSlotTypeBadge
  },
  computed: {
    formIsValid() {
      return !!this.bidMasterGame;
    },
    publisherSlotsAreFilled() {
      let userGames = this.leagueYear.userPublisher.games;
      let standardGameSlots = this.leagueYear.standardGames;
      let userStandardGames = _.filter(userGames, { counterPick: false });
      return userStandardGames.length >= standardGameSlots;
    },
    droppableGames() {
      return _.filter(this.publisher.games, { counterPick: false });
    },
    bidButtonText() {
      return 'Place Bid';
    }
  },
  props: ['leagueYear', 'publisher'],
  methods: {
    searchGame() {
      this.clearDataExceptSearch();
      this.isBusy = true;
      axios
        .get('/api/league/PossibleMasterGames?gameName=' + this.bidGameName + '&year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.searched = true;
        })
        .catch((response) => {
          this.isBusy = false;
        });
    },
    getTopGames() {
      this.clearDataExceptSearch();
      this.isBusy = true;
      axios
        .get('/api/league/TopAvailableGames?year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.showingTopAvailable = true;
        })
        .catch((response) => {
          this.isBusy = false;
        });
    },
    getQueuedGames() {
      this.clearDataExceptSearch();
      this.isBusy = true;
      axios
        .get('/api/league/CurrentQueuedGameYears/' + this.publisher.publisherID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.showingQueuedGames = true;
        })
        .catch((response) => {
          this.isBusy = false;
        });
    },
    getGamesForSlot(slotInfo) {
      this.clearDataExceptSearch();
      this.isBusy = true;
      let slotJSON = JSON.stringify(slotInfo);
      let base64Slot = btoa(slotJSON);
      let urlEncodedSlot = encodeURI(base64Slot);
      axios
        .get('/api/league/TopAvailableGames?year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID + '&slotInfo=' + urlEncodedSlot)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.showingTopAvailable = true;
        })
        .catch((response) => {
          this.isBusy = false;
        });
    },
    bidGame() {
      var request = {
        publisherID: this.leagueYear.userPublisher.publisherID,
        masterGameID: this.bidMasterGame.masterGameID,
        bidAmount: this.bidAmount,
        counterPick: false
      };

      if (this.conditionalDrop) {
        request.conditionalDropPublisherGameID = this.conditionalDrop.publisherGameID;
      }

      this.requestIsBusy = true;
      axios
        .post('/api/league/MakePickupBid', request)
        .then((response) => {
          this.bidResult = response.data;
          this.requestIsBusy = false;
          if (!this.bidResult.success) {
            return;
          }
          this.$refs.bidGameFormRef.hide();
          var bidInfo = {
            gameName: this.bidMasterGame.gameName,
            bidAmount: this.bidAmount
          };
          this.$emit('gameBid', bidInfo);
          this.clearData();
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
          this.requestIsBusy = false;
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
      this.requestIsBusy = false;
      this.conditionalDrop = false;
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
