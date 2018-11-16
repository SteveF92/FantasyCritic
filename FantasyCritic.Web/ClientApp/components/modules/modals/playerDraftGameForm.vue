<template>
  <b-modal id="playerDraftGameForm" ref="playerDraftGameFormRef" title="Select Draft Game" hide-footer @hidden="clearData">
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <div class="form-group">
        <label for="draftGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input v-model="draftGameName" id="draftGameName" name="draftGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="draftMasterGame" :possibleGames="possibleMasterGames" :maximumEligibilityLevel="maximumEligibilityLevel"></possibleMasterGamesTable>
        <div v-if="draftMasterGame">
          Selected Game: {{draftMasterGame.gameName}}
        </div>
      </div>
    </form>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="addGame">
      <div>
        <input type="submit" class="btn btn-primary add-game-button" value="Draft Game" v-if="formIsValid" />
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
              Your league manager can override these warnings.
            </label>
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
                draftGameName: null,
                draftMasterGame: null,
                draftGameType: null,
                draftResult: null,
                draftOverride: false,
                draftCounterPick: false,
                possibleMasterGames: []
            }
        },
        components: {
            PossibleMasterGamesTable
        },
        computed: {
          formIsValid() {
            return (this.draftGameName);
          }
        },
        props: ['userPublisher', 'maximumEligibilityLevel'],
        methods: {
            searchGame() {
                axios
                    .get('/api/game/MasterGame?gameName=' + this.draftGameName)
                    .then(response => {
                        this.possibleMasterGames = response.data;
                    })
                    .catch(response => {

                    });
            },
            addGame() {
                var gameName = this.draftGameName;
                if (this.draftMasterGame !== null) {
                    gameName = this.draftMasterGame.gameName;
                }

                var masterGameID = null;
                if (this.draftMasterGame !== null) {
                    masterGameID = this.draftMasterGame.masterGameID;
                }

                var request = {
                    publisherID: this.userPublisher.publisherID,
                    gameName: gameName,
                    counterPick: this.draftCounterPick,
                    masterGameID: masterGameID,
                    managerOverride: this.draftOverride
                };

                axios
                  .post('/api/league/DraftGame', request)
                  .then(response => {
                      this.draftResult = response.data;
                      if (!this.draftResult.success) {
                        return;
                      }
                      this.$refs.playerDraftGameFormRef.hide();
                      var draftInfo = {
                        gameName
                      };
                      this.$emit('gameDrafted', draftInfo);
                      this.draftGameName = null;
                      this.draftMasterGame = null;
                      this.draftCounterPick = false;
                      this.possibleMasterGames = [];
                    })
                    .catch(response => {
                      
                    });
            },
            clearData() {
              this.draftResult = null;
              this.draftGameName = null;
              this.draftMasterGame = null;
              this.draftCounterPick = false;
              this.possibleMasterGames = [];
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
