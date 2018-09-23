<template>
  <div>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <div class="form-group">
        <label for="claimGameName" class="control-label">Game Name</label>
        <div class="input-group">
          <input v-model="claimGameName" id="claimGameName" name="claimGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="claimMasterGame" :possibleGames="possibleMasterGames" :maximumEligibilityLevel="maximumEligibilityLevel"></possibleMasterGamesTable>
        <div v-if="claimMasterGame">
          Selected Game: {{claimMasterGame.gameName}}
        </div>
      </div>
    </form>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="addGame">
      <div class="form-group">
        <label for="claimPublisher" class="control-label">Publisher</label>
        <b-form-select v-model="claimPublisher">
          <option v-for="publisher in publishers" v-bind:value="publisher">
            {{ publisher.publisherName }}
          </option>
        </b-form-select>
      </div>
      <b-form-group>
        <b-form-radio-group id="gameType"
                            buttons
                            v-model="claimGameType"
                            :options="claimGameTypes" />
      </b-form-group>
      <div>
        <input type="submit" class="btn btn-primary add-game-button" value="Add game to publisher" />
      </div>
    </form>
  </div>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import PossibleMasterGamesTable from "components/modules/possibleMasterGamesTable";
    export default {
        data() {
            return {
                claimGameName: null,
                claimPublisher: null,
                claimMasterGame: null,
                claimGameType: null,
                claimGameTypes: [
                    'Draft',
                    'CounterPick',
                    'Waiver'
                ],
                possibleMasterGames: []
            }
        },
        components: {
            PossibleMasterGamesTable
        },
        props: ['publishers', 'maximumEligibilityLevel'],
        methods: {
            searchGame() {
                axios
                    .get('/api/game/MasterGame?gameName=' + this.claimGameName)
                    .then(response => {
                        this.possibleMasterGames = response.data;
                    })
                    .catch(response => {

                    });
            },
            addGame() {
                var gameName = this.claimGameName;
                if (this.claimMasterGame !== null) {
                    gameName = this.claimMasterGame.gameName;
                }

                var waiver = (this.claimGameType === "Waiver");
                var counterPick = (this.claimGameType === "CounterPick");
                var masterGameID = null;
                if (this.claimMasterGame !== null) {
                    masterGameID = this.claimMasterGame.masterGameID;
                }

                var request = {
                    publisherID: this.claimPublisher.publisherID,
                    gameName: gameName,
                    waiver: waiver,
                    counterPick: counterPick,
                    masterGameID: masterGameID
                };

                axios
                    .post('/api/league/ManagerClaimGame', request)
                    .then(response => {
                        this.$emit('claim-game-success', gameName, this.claimPublisher.publisherName);
                        this.claimGameName = null;
                        this.claimPublisher = null;
                        this.claimMasterGame = null;
                        this.claimGameType = null;
                    })
                    .catch(response => {

                    });
            }
        }
    }
</script>
<style scoped>
.add-game-button{
  width: 100%;
}
</style>
