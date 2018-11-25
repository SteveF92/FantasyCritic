<template>
  <b-modal id="managerDraftCounterPickForm" ref="managerDraftCounterPickFormRef" title="Select Counter Pick" hide-footer @hidden="clearData">
    <form class="form-horizontal" v-on:submit.prevent="selectCounterPick" hide-footer>
      <div class="form-group">
        <label for="selectedCounterPick" class="control-label">Game</label>
        <b-form-select v-model="selectedCounterPick">
          <option v-for="publisherGame in availableCounterPicks" v-bind:value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>

      <div v-if="selectedCounterPick">
        <input type="submit" class="btn btn-primary add-game-button" value="Select Game as Counter-Pick" />
      </div>
    </form>
  </b-modal>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";

    export default {
        data() {
          return {
            selectedCounterPick: null
          }
        },
        props: ['nextPublisherUp', 'availableCounterPicks'],
        methods: {
          selectCounterPick() {

            var request = {
              publisherID: this.nextPublisherUp.publisherID,
              gameName: this.selectedCounterPick.gameName,
              counterPick: true,
              masterGameID: this.selectedCounterPick.masterGameID
            };

            axios
              .post('/api/leagueManager/ManagerDraftGame', request)
              .then(response => {
                this.draftResult = response.data;
                if (!this.draftResult.success) {
                  return;
                }
                this.$refs.playerDraftCounterPickFormRef.hide();
                var draftInfo = {
                  gameName: this.selectedCounterPick.gameName
                };
                this.$emit('counterPickDrafted', draftInfo);
                this.selectedCounterPick = null;
              })
              .catch(response => {

              });
          },
          clearData() {
            this.selectedCounterPick = null;
          },
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
                    publisherID: this.nextPublisherUp.publisherID,
                    gameName: gameName,
                    counterPick: true,
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
                      this.$refs.managerDraftCounterPickFormRef.hide();
                      var draftInfo = {
                        gameName,
                        publisherName: this.nextPublisherUp.publisherName
                      };
                      this.$emit('gameDrafted', draftInfo);
                      this.draftGameName = null;
                      this.draftMasterGame = null;
                      this.draftOverride = false;
                      this.possibleMasterGames = [];
                    })
                    .catch(response => {
                      
                    });
            },
            clearData() {
              this.draftResult = null;
              this.draftGameName = null;
              this.draftMasterGame = null;
              this.draftOverride = false;
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
