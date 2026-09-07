<template>
  <b-modal id="manageTagOverridesModal" ref="tagOverridesModalRef" size="lg" title="Manage Tag Overrides" hide-footer @hidden="clearData">
    <div class="alert alert-info">This option will allow you to override the tags of a game to whatever you want, if you disagree with how the site has classified something.</div>
    <div v-if="leagueYear.tagOverrides.length > 0">
      <table class="table table-bordered table-striped">
        <thead>
          <tr class="bg-primary">
            <th scope="col" class="game-column">Game</th>
            <th scope="col">Tags</th>
            <th scope="col">Reset?</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="tagOverride in leagueYear.tagOverrides" :key="tagOverride.masterGame.masterGameID">
            <td>{{ tagOverride.masterGame.gameName }}</td>
            <td>
              <span v-for="tag in tagOverride.tags" :key="tag">
                <masterGameTagBadge :tag-name="tag"></masterGameTagBadge>
              </span>
            </td>
            <td class="select-cell">
              <b-button variant="danger" @click="resetTags(tagOverride)">Reset</b-button>
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
    <div v-if="overrideMasterGame">
      <masterGameTagSelector v-model="chosenTags"></masterGameTagSelector>
      <b-button variant="info" class="set-tags-button" size="sm" @click="setTags(overrideMasterGame)">Set Tags</b-button>
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
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';
import MasterGameTagSelector from '@/components/masterGameTagSelector.vue';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  components: {
    PossibleMasterGamesTable,
    MasterGameTagBadge,
    MasterGameTagSelector
  },
  mixins: [LeagueMixin],
  data() {
    return {
      searchGameName: '',
      overrideMasterGame: null,
      possibleMasterGames: [],
      errorInfo: '',
      chosenTags: []
    };
  },
  computed: {
    formIsValid() {
      return this.overrideMasterGame;
    }
  },
  methods: {
    resetTags(tagOverride) {
      const model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        masterGameID: tagOverride.masterGame.masterGameID,
        tags: []
      };
      axios
        .post('/api/leagueManager/SetGameTagOverride', model)
        .then(() => {
          this.notifyAction(tagOverride.masterGame.gameName + " had it's tags reset.");
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
    setTags(masterGame) {
      let tagNames = this.chosenTags.map((x) => x.name);

      const model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        masterGameID: masterGame.masterGameID,
        tags: tagNames
      };
      axios
        .post('/api/leagueManager/SetGameTagOverride', model)
        .then(() => {
          this.notifyAction(masterGame.gameName + " had it's tags overriden.");
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
      this.chosenTags = [];
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
.set-tags-button {
  margin-top: 10px;
  width: 100%;
}
</style>
