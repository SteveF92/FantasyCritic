<template>
  <b-modal id="manageTagOverridesModal" ref="tagOverridesModalRef" title="Manage Tag Overrides" @hidden="clearData" hide-footer>
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
          <tr v-for="tagOverride in leagueYear.tagOverrides">
            <td>{{tagOverride.masterGame.gameName}}</td>
            <td>
              <span v-for="(tag, index) in tagOverride.tags">
                <masterGameTagBadge :tagName="tagOverride.tags[index]"></masterGameTagBadge>
              </span>
            </td>
            <td class="select-cell">
              <b-button variant="danger" v-on:click="resetTags(tagOverride)">Reset</b-button>
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
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="overrideMasterGame" :possibleGames="possibleMasterGames" v-on:input="newGameSelected"></possibleMasterGamesTable>

        <label v-if="overrideMasterGame" for="overrideMasterGame" class="control-label">Selected Game: {{overrideMasterGame.gameName}}</label>
      </div>
    </form>
    <!--<div class="eligibility-set-buttons" v-if="overrideMasterGame">
      <b-button variant="danger" size="sm" v-on:click="setEligibility(overrideMasterGame, false)">Ban Game</b-button>
      <b-button variant="success" size="sm" v-on:click="setEligibility(overrideMasterGame, true)">Allow Game</b-button>
    </div>-->
    <br />
    <div v-if="errorInfo" class="alert alert-danger">
      <h3 class="alert-heading">Error!</h3>
      <p class="text-white">{{errorInfo}}</p>
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import PossibleMasterGamesTable from '@/components/modules/possibleMasterGamesTable';
import MasterGameTagBadge from '@/components/modules/masterGameTagBadge';

export default {
  data() {
    return {
      overrideGameName: '',
      overrideMasterGame: null,
      possibleMasterGames: [],
      errorInfo: ''
    };
  },
  components: {
    PossibleMasterGamesTable,
    MasterGameTagBadge
  },
  computed: {
    formIsValid() {
      return (this.overrideMasterGame);
    }
  },
  props: ['leagueYear'],
  methods: {
    resetTags(tagOverride) {
      var model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        masterGameID: tagOverride.masterGame.masterGameID,
        tags: []
      };
      axios
        .post('/api/leagueManager/SetGameTagOverride', model)
        .then(response => {
          var gameInfo = {
            gameName: tagOverride.masterGame.gameName
          };
          this.$emit('gameTagsReset', gameInfo);
        })
        .catch(response => {
          this.errorInfo = response.response.data;
        });
    },
    searchGame() {
      this.overrideMasterGame = null;
      this.possibleMasterGames = [];
      axios
        .get('/api/league/PossibleMasterGames?gameName=' + this.overrideGameName + '&year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID)
        .then(response => {
          this.possibleMasterGames = response.data;
        })
        .catch(response => {

        });
    },
    setTags(masterGame, tags) {
      var model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        masterGameID: masterGame.masterGameID,
        tags: tags
      };
      axios
        .post('/api/leagueManager/SetGameTagOverride', model)
        .then(response => {
          var gameInfo = {
            gameName: masterGame.gameName
          };
          this.$emit('gameEligibilitySet', gameInfo);
          this.clearData();
        })
        .catch(response => {
          this.errorInfo = response.response.data;
        });
    },
    clearData() {
      this.overrideGameName = '';
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
