<template>
  <b-modal id="manageEligibilityOverridesModal" ref="eligibilityOverridesModalRef" title="Manage Eligibility Overrides" @hidden="clearData" hide-footer>
    <div v-if="leagueYear.eligibilityOverrides.length > 0">
      <table class="table table-bordered table-striped">
        <thead>
          <tr class="bg-primary">
            <th scope="col" class="game-column">Game</th>
            <th scope="col">Eligible?</th>
            <th scope="col">Reset?</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="eligibilityOverride in leagueYear.eligibilityOverrides">
            <td>{{eligibilityOverride.masterGame.gameName}}</td>
            <td>{{eligibilityOverride.eligible | yesNo}}</td>
            <td class="select-cell">
              <b-button variant="danger" v-on:click="resetEligibility(eligibilityOverride)">Reset</b-button>
            </td>
          </tr>
        </tbody>
      </table>
      <hr />
    </div>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <div class="form-group">
        <label for="overrideGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input v-model="overrideGameName" id="overrideGameName" name="overrideGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="overrideMasterGame" :possibleGames="possibleMasterGames"
                                  :maximumEligibilityLevel="leagueYear.eligibilitySettings.eligibilityLevel"
                                  v-on:input="newGameSelected"></possibleMasterGamesTable>

        <label v-if="overrideMasterGame" for="overrideMasterGame" class="control-label">Selected Game: {{overrideMasterGame.gameName}}</label>
      </div>
    </form>
    <div class="eligibility-set-buttons" v-if="overrideMasterGame">
      <b-button variant="danger" size="sm" v-on:click="setEligibility(overrideMasterGame, false)">Ban Game</b-button>
      <b-button variant="success" size="sm" v-on:click="setEligibility(overrideMasterGame, true)">Allow Game</b-button>
    </div>
    <br />
    <div v-if="errorInfo" class="alert alert-danger">
      <h3 class="alert-heading">Error!</h3>
      <p class="text-white">{{errorInfo}}</p>
    </div>
  </b-modal>
</template>

<script>
  import axios from "axios";
  import PossibleMasterGamesTable from "components/modules/possibleMasterGamesTable";
  export default {
    data() {
      return {
        overrideGameName: "",
        overrideMasterGame: null,
        possibleMasterGames: [],
        errorInfo: ""
      }
    },
    components: {
      PossibleMasterGamesTable
    },
    computed: {
      formIsValid() {
        return (this.overrideMasterGame);
      }
    },
    props: ['leagueYear'],
    methods: {
      resetEligibility(eligibilityOverride) {
        var model = {
          leagueID: this.leagueYear.leagueID,
          year: this.leagueYear.year,
          masterGameID: eligibilityOverride.masterGame.masterGameID,
          eligible: null
        };
        axios
          .post('/api/leagueManager/SetGameEligibilityOverride', model)
          .then(response => {
            var gameInfo = {
              gameName: eligibilityOverride.masterGame.gameName
            };
            this.$emit('gameEligiblityReset', gameInfo);
          })
          .catch(response => {
            this.errorInfo = response.response.data;
          });
      },
      searchGame() {
        this.overrideMasterGame = null;
        this.possibleMasterGames = [];
        axios
          .get('/api/game/MasterGame?gameName=' + this.overrideGameName)
          .then(response => {
            this.possibleMasterGames = response.data;
          })
          .catch(response => {

          });
      },
      setEligibility(masterGame, eligible) {
        var model = {
          leagueID: this.leagueYear.leagueID,
          year: this.leagueYear.year,
          masterGameID: masterGame.masterGameID,
          eligible: eligible
        };
        axios
          .post('/api/leagueManager/SetGameEligibilityOverride', model)
          .then(response => {
            var gameInfo = {
              gameName: masterGame.gameName,
              eligible
            };
            this.$emit('gameEligibilitySet', gameInfo);
            this.clearData();
          })
          .catch(response => {
            this.errorInfo = response.response.data;
          });
      },
      clearData() {
        this.overrideGameName = "";
        this.overrideMasterGame = null;
        this.possibleMasterGames = [];
        this.errorInfo = "";
      },
      newGameSelected() {
        this.errorInfo = "";
      }
    }
  }
</script>
<style scoped>
  .select-cell {
    text-align: center;
  }
  .game-search-input {
    margin-bottom: 15px;
  }

  .eligibility-set-buttons {
    display: flex;
    justify-content: space-around;
  }
</style>
