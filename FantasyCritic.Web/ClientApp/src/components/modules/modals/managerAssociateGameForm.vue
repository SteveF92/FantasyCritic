<template>
  <b-modal id="associateGameForm" ref="associateGameFormRef" size="lg" title="Associate Publisher Game" hide-footer @hidden="clearData">
    <div class="alert alert-info">This form allows you to link a game that currently says "Not Linked to Master Game" with the correct master game.</div>
    <div class="form-group">
      <label for="associatePublisher" class="control-label">Publisher</label>
      <b-form-select v-model="associatePublisher">
        <option v-for="publisher in publishers" v-bind:value="publisher">
          {{ publisher.publisherName }}
        </option>
      </b-form-select>
      <div v-if="associatePublisher">
        <label for="associatePublisherGame" class="control-label">Game</label>
        <b-form-select v-model="associatePublisherGame">
          <option v-for="publisherGame in associatePublisher.games" v-bind:value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>
    </div>

    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <div class="form-group">
        <label for="associateGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input v-model="associateGameName" id="associateGameName" name="associateGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="associateMasterGame" :possibleGames="possibleMasterGames" :maximumEligibilityLevel="maximumEligibilityLevel"
                                  v-on:input="newGameSelected"></possibleMasterGamesTable>

        <label v-if="associateMasterGame" for="associateMasterGame" class="control-label">Selected Game: {{associateMasterGame.gameName}}</label>
      </div>
    </form>

    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="associateGame" v-if="associateMasterGame">
      <div>
        <input type="submit" class="btn btn-primary add-game-button" value="Associate game" />
      </div>

      <div v-if="associateResult && !associateResult.success" class="alert associate-error" v-bind:class="{ 'alert-danger': !associateResult.overridable, 'alert-warning': associateResult.overridable }">
        <h3 class="alert-heading" v-if="associateResult.overridable">Warning!</h3>
        <h3 class="alert-heading" v-if="!associateResult.overridable">Error!</h3>
        <ul>
          <li v-for="error in associateResult.errors">{{error}}</li>
        </ul>

        <div class="form-check" v-if="associateResult.overridable">
          <span>
            <label class="text-white">
              Do you want to override these warnings?
            </label>
            <input class="form-check-input override-checkbox" type="checkbox" v-model="associateOverride">
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
                associateGameName: "",
                associatePublisher: null,
                associateMasterGame: null,
                associatePublisherGame: null,
                associateOverride: false,
                associateResult: null,
                possibleMasterGames: []
            }
        },
        components: {
            PossibleMasterGamesTable
        },
        props: ['publishers', 'maximumEligibilityLevel', 'year'],
        methods: {
          searchGame() {
            this.possibleMasterGames = [];
            this.associateResult = null;
            axios
              .get('/api/league/PossibleMasterGames?gameName=' + this.associateGameName + '&year=' + this.year + '&leagueid=' + this.publishers[0].leagueID)
              .then(response => {
                  this.possibleMasterGames = response.data;
              })
              .catch(response => {

              });
            },
            associateGame() {
                var request = {
                    publisherID: this.associatePublisher.publisherID,
                    publisherGameID: this.associatePublisherGame.publisherGameID,
                    masterGameID: this.associateMasterGame.masterGameID,
                    managerOverride: this.associateOverride
                };

                axios
                  .post('/api/leagueManager/ManagerAssociateGame', request)
                  .then(response => {
                      this.associateResult = response.data;
                      if (!this.associateResult.success) {
                          return;
                        }

                      this.$refs.associateGameFormRef.hide();
                      this.$emit('gameAssociated', this.associateMasterGame);
                      this.associateGameName = "";
                      this.associatePublisher = null;
                      this.associateMasterGame = null;
                      this.associatePublisherGame = null;
                      this.associateOverride = false;
                      this.possibleMasterGames = [];
                    })
                    .catch(response => {

                    });
            },
          clearData() {
              this.associateResult = null;
              this.associateGameName = "";
              this.associatePublisher = null;
              this.associateMasterGame = null;
              this.associatePublisherGame = null;
              this.associateOverride = false;
              this.possibleMasterGames = [];
              this.claimCounterPick = false;
              this.claimPublisher = null;
          },
          newGameSelected() {
            this.associateResult = null;
          }
        }
    }
</script>
<style scoped>
.add-game-button{
  width: 100%;
}
.associate-error{
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
