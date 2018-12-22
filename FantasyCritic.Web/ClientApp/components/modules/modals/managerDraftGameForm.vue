<template>
  <b-modal id="managerDraftGameForm" ref="managerDraftGameFormRef" size="lg" title="Select Draft Game" hide-footer @hidden="clearData">
    <div class="form-group">
      <label for="nextPublisherUp" class="control-label">Select the next game for publisher: </label>
      <div>
        <strong>{{nextPublisherUp.publisherName}} (Display Name: {{nextPublisherUp.playerName}})</strong>
      </div>
    </div>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <div class="form-group">
        <label for="draftGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input v-model="searchGameName" id="searchGameName" name="searchGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="draftMasterGame" :possibleGames="possibleMasterGames" :maximumEligibilityLevel="maximumEligibilityLevel"></possibleMasterGamesTable>

        <div v-show="searched && !draftMasterGame" class="alert" v-bind:class="{ 'alert-info': possibleMasterGames.length > 0, 'alert-warning': possibleMasterGames.length === 0 }">
          <div class="row">
            <span class="col-8" v-show="possibleMasterGames.length > 0">Don't see the game you are looking for?</span>
            <span class="col-8" v-show="possibleMasterGames.length === 0">No games were found.</span>
            <b-button variant="primary" v-on:click="showUnlistedField" class="col-4" size="sm">Select unlisted game.</b-button>
          </div>

          <div v-if="showingUnlistedField">
            <label for="draftUnlistedGame" class="control-label">Custom Game Name</label>
            <div class="input-group game-search-input">
              <input v-model="draftUnlistedGame" id="draftUnlistedGame" name="draftUnlistedGame" type="text" class="form-control input" />
            </div>
            <div>Enter the full name of the game you want.</div>
            <div>You as league manager can link this custom game with a "master game" later.</div>
          </div>

        </div>

        <label v-if="draftMasterGame" for="draftMasterGame" class="control-label">Selected Game: {{draftMasterGame.gameName}}</label>
      </div>
    </form>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="addGame">
      <div>
        <input type="submit" class="btn btn-primary add-game-button" value="Add game to publisher" v-if="formIsValid" />
      </div>
      <div v-if="draftResult && !draftResult.success" class="alert draft-error" v-bind:class="{ 'alert-danger': !draftResult.overridable, 'alert-warning': draftResult.overridable }">
        <h4 class="alert-heading" v-if="draftResult.overridable">Warning!</h4>
        <h4 class="alert-heading" v-if="!draftResult.overridable">Error!</h4>
        <ul>
          <li v-for="error in draftResult.errors">{{error}}</li>
        </ul>

        <div class="form-check" v-if="draftResult.overridable">
          <span>
            <label class="form-check-label">
              Do you want to override these warnings?
            </label>
            <input class="form-check-input override-checkbox" type="checkbox" v-model="draftOverride">
          </span>
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
                searchGameName: null,
                draftUnlistedGame: null,
                draftMasterGame: null,
                draftGameType: null,
                draftResult: null,
                draftOverride: false,
                possibleMasterGames: [],
                searched: false,
                showingUnlistedField: false
            }
        },
        components: {
            PossibleMasterGamesTable
        },
        computed: {
          formIsValid() {
            return (this.draftUnlistedGame || this.draftMasterGame);
          },
        },
        props: ['nextPublisherUp', 'maximumEligibilityLevel'],
        methods: {
          searchGame() {
            this.possibleMasterGames = [];
            axios
                .get('/api/game/MasterGame?gameName=' + this.searchGameName)
                .then(response => {
                  this.possibleMasterGames = response.data;
                  this.searched = true;
                  this.showingUnlistedField = false;
                  this.draftMasterGame = null;
                })
                .catch(response => {

                });
            },
            showUnlistedField() {
              this.showingUnlistedField = true;
              this.draftUnlistedGame = this.searchGameName;
            },
            addGame() {
                var gameName = "";
                if (this.draftMasterGame !== null) {
                  gameName = this.draftMasterGame.gameName;
                } else if (this.draftUnlistedGame !== null) {
                  gameName = this.draftUnlistedGame;
                }

                var masterGameID = null;
                if (this.draftMasterGame !== null) {
                    masterGameID = this.draftMasterGame.masterGameID;
                }

                var request = {
                    publisherID: this.nextPublisherUp.publisherID,
                    gameName: gameName,
                    counterPick: false,
                    masterGameID: masterGameID,
                    managerOverride: this.draftOverride
                };

                axios
                  .post('/api/leagueManager/ManagerDraftGame', request)
                  .then(response => {
                      this.draftResult = response.data;
                      if (!this.draftResult.success) {
                        return;
                      }
                      this.$refs.managerDraftGameFormRef.hide();
                      var draftInfo = {
                        gameName,
                        publisherName: this.nextPublisherUp.publisherName
                      };
                      this.$emit('gameDrafted', draftInfo);
                      this.clearData();
                    })
                    .catch(response => {
                      
                    });
            },
            clearData() {
              this.searchGameName = null;
              this.draftUnlistedGame = null;
              this.draftMasterGame = null;
              this.draftGameType = null;
              this.draftResult = null;
              this.draftOverride = false;
              this.possibleMasterGames = [];
              this.searched = false;
              this.showingUnlistedField = false;
            }
        }
    }
</script>
<style scoped>
.add-game-button{
  width: 100%;
}
.draft-error{
  margin-top: 10px;
}
.game-search-input{
  margin-bottom: 15px;
}
.override-checkbox {
  margin-left: 10px;
  margin-top: 8px;
}
</style>
