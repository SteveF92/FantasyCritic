<template>
  <b-modal id="bidGameForm" ref="bidGameFormRef" size="lg" title="Make a Bid" hide-footer @hidden="clearData">
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <div class="form-group">
        <label for="bidGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input v-model="bidGameName" id="bidGameName" name="bidGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="bidMasterGame" :possibleGames="possibleMasterGames"
                                  :maximumEligibilityLevel="leagueYear.eligibilitySettings.eligibilityLevel"
                                  v-on:input="newGameSelected"></possibleMasterGamesTable>

        <label v-if="bidMasterGame" for="bidMasterGame" class="control-label">Selected Game: {{bidMasterGame.gameName}}</label>
      </div>
    </form>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="bidGame">
      <div class="form-group">
        <label for="bidAmount" class="control-label">Bid Amount (Remaining: {{leagueYear.userPublisher.budget | money}})</label>
        <input v-model="bidAmount" id="bidAmount" name="bidAmount" type="text" class="form-control input" />
      </div>
      <div>
        <input type="submit" class="btn btn-primary add-game-button" value="Make Bid" v-if="formIsValid" />
      </div>
      <div v-if="bidResult && !bidResult.success" class="alert bid-error alert-danger">
        <h3 class="alert-heading">Error!</h3>
        <ul>
          <li v-for="error in bidResult.errors">{{error}}</li>
        </ul>
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
                possibleMasterGames: []
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
            axios
                .get('/api/game/MasterGame?gameName=' + this.bidGameName)
                .then(response => {
                    this.possibleMasterGames = response.data;
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

                    });
            },
            clearData() {
              this.bidResult = null;
              this.bidGameName = "";
              this.bidMasterGame = null;
              this.bidAmount = 0;
              this.possibleMasterGames = [];
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
