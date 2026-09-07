<template>
  <b-modal id="manageEligibilityOverridesModal" ref="eligibilityOverridesModalRef" size="lg" title="Manage Eligibility Overrides" hide-footer @hidden="clearData">
    <div class="alert alert-info">This option will allow you to manually allow or ban a specific game in your league, no matter what your other settings are.</div>
    <div v-if="leagueYear.settings.hasSpecialSlots" class="alert alert-warning">
      Warning! Because your league uses 'special roster slots', you should consider using the 'tag override' option, instead of this option. If you set a game as 'eligible' here, it will be eligible
      in
      <em>any</em>
      slot in your league.
    </div>
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
          <tr v-for="eligibilityOverride in leagueYear.eligibilityOverrides" :key="eligibilityOverride.masterGame.masterGameID">
            <td>{{ eligibilityOverride.masterGame.gameName }}</td>
            <td>{{ eligibilityOverride.eligible | yesNo }}</td>
            <td class="select-cell">
              <b-button variant="danger" @click="resetEligibility(eligibilityOverride)">Reset</b-button>
            </td>
          </tr>
        </tbody>
      </table>
      <hr />
    </div>
    <form method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
      <div class="form-group">
        <label for="searchGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" :disabled="!searchGameName" @click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable
          v-if="!overrideMasterGame && possibleMasterGames.length > 0"
          v-model="overrideMasterGame"
          :possible-games="possibleMasterGames"
          @input="newGameSelected"></possibleMasterGamesTable>
      </div>
    </form>
    <label v-if="overrideMasterGame" for="overrideMasterGame" class="control-label">Selected Game: {{ overrideMasterGame.gameName }}</label>
    <div v-if="overrideMasterGame" class="eligibility-set-buttons">
      <b-button variant="danger" size="sm" @click="setEligibility(overrideMasterGame, false)">Ban Game</b-button>
      <b-button variant="success" size="sm" @click="setEligibility(overrideMasterGame, true)">Allow Game</b-button>
    </div>
    <br />
    <div v-if="errorInfo" class="alert alert-danger">
      <h3 class="alert-heading">Error!</h3>
      <p class="text-white">{{ errorInfo }}</p>
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  components: {
    PossibleMasterGamesTable
  },
  mixins: [LeagueMixin],
  data() {
    return {
      searchGameName: '',
      overrideMasterGame: null,
      possibleMasterGames: [],
      errorInfo: ''
    };
  },
  computed: {
    formIsValid() {
      return this.overrideMasterGame;
    }
  },
  methods: {
    resetEligibility(eligibilityOverride) {
      const model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        masterGameID: eligibilityOverride.masterGame.masterGameID,
        eligible: null
      };
      axios
        .post('/api/leagueManager/SetGameEligibilityOverride', model)
        .then(() => {
          this.notifyAction(eligibilityOverride.masterGame.gameName + "'s eligibility was reset to normal.");
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
        });
    },
    searchGame() {
      this.overrideMasterGame = null;
      this.possibleMasterGames = [];
      axios
        .get('/api/league/PossibleMasterGames?gameName=' + this.searchGameName + '&year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
        })
        .catch(() => {});
    },
    setEligibility(masterGame, eligible) {
      const model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        masterGameID: masterGame.masterGameID,
        eligible: eligible
      };
      axios
        .post('/api/leagueManager/SetGameEligibilityOverride', model)
        .then(() => {
          let message = '';
          if (eligible) {
            message = masterGame.gameName + ' was marked as eligible.';
          } else {
            message = masterGame.gameName + ' was marked as ineligible.';
          }
          this.notifyAction(message);
          this.clearData();
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
        });
    },
    clearData() {
      this.searchGameName = '';
      this.overrideMasterGame = null;
      this.possibleMasterGames = [];
      this.errorInfo = '';
    },
    newGameSelected() {
      this.errorInfo = '';
    }
  }
};
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
