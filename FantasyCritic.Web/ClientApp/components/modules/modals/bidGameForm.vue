<template>
  <b-modal id="bidGameForm" ref="bidGameFormRef" size="lg" title="Make a Bid" hide-footer @hidden="clearData">
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{errorInfo}}
    </div>
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
                                  :maximumEligibilityLevel="leagueYear.eligibilitySettings.eligibilityLevel"
                                  v-on:input="newGameSelected"></possibleMasterGamesTable>
      </div>

      <div class="alert alert-info" v-show="searched && !bidMasterGame && possibleMasterGames.length === 0">
        <div class="row">
          <span class="col-12 col-md-7">No games were found.</span>
        </div>
      </div>

      <div v-if="bidMasterGame">
        <h3 for="bidMasterGame" class="selected-game text-black">Selected Game: {{bidMasterGame.gameName}}</h3>
        <hr />
        <div class="form-group">
          <label for="bidAmount" class="control-label">Bid Amount (Remaining: {{leagueYear.userPublisher.budget | money}})</label>
          <input v-model="bidAmount" id="bidAmount" name="bidAmount" type="text" class="form-control input" />
        </div>
        <b-button variant="primary" v-on:click="bidGame" class="add-game-button" v-if="formIsValid" :disabled="isBusy">Draft Game</b-button>
        <div v-if="bidResult && !bidResult.success" class="alert bid-error alert-danger">
          <h3 class="alert-heading">Error!</h3>
          <ul>
            <li v-for="error in bidResult.errors">{{error}}</li>
          </ul>
        </div>
      </div>
    </form>
  </b-modal>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import PossibleMasterGamesTable from "components/modules/possibleMasterGamesTable";
    export default {
        data() {
          return {
                bidGameName: "",
                bidMasterGame: null,
                bidAmount: 0,
                bidResult: null,
                possibleMasterGames: [],
                errorInfo: "",
                showingTopAvailable: false,
                searched: false,
                isBusy: false
            }
        },
        components: {
            PossibleMasterGamesTable
        },
        computed: {
          formIsValid() {
            return (this.bidMasterGame);
          }
        },
        props: ['leagueYear', 'maximumEligibilityLevel'],
        methods: {
          searchGame() {
            this.bidResult = null;
            this.possibleMasterGames = [];
            this.searched = false;
            this.showingTopAvailable = false;
            this.isBusy = true;
            axios
                .get('/api/league/PossibleMasterGames?gameName=' + this.bidGameName + '&year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID)
                .then(response => {
                  this.possibleMasterGames = response.data;
                  this.isBusy = false;
                })
                .catch(response => {
                  this.isBusy = false;
                });
            },
            getTopGames() {
              this.possibleMasterGames = [];
              this.bidMasterGame = null;
              this.bidResult = null;
              this.showingTopAvailable = false;
              axios
                .get('/api/league/TopAvailableGames?year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID)
                .then(response => {
                  this.possibleMasterGames = response.data;
                  this.showingTopAvailable = true;
                  this.searched = true;
                  this.showingUnlistedField = false;
                  this.draftMasterGame = null;
                })
                .catch(response => {

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
                      this.bidGameName = "";
                      this.bidMasterGame = null;
                      this.bidAmount = 0;
                      this.possibleMasterGames = [];
                    })
                    .catch(response => {
                      this.errorInfo = response.response.data;
                    });
            },
            clearData() {
              this.bidResult = null;
              this.bidGameName = "";
              this.bidMasterGame = null;
              this.bidAmount = 0;
              this.possibleMasterGames = [];
              this.searched = false;
              this.showingTopAvailable = false;
              this.isBusy = false;
            },
            newGameSelected() {
              this.bidResult = null;
            }
        }
    }
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
