<template>
  <b-modal id="associateGameForm" ref="associateGameFormRef" size="lg" title="Associate Publisher Game" hide-footer @hidden="clearData">
    <div class="alert alert-info">This form allows you to link a game that currently says "Not Linked to Master Game" with the correct master game.</div>
    <div class="form-group">
      <label for="associatePublisher" class="control-label">Publisher</label>
      <b-form-select v-model="associatePublisher">
        <option v-for="publisher in publishers" :key="publisher.publisherID" :value="publisher">
          {{ publisher.publisherName }}
        </option>
      </b-form-select>
      <div v-if="associatePublisher">
        <label for="associatePublisherGame" class="control-label">Game</label>
        <b-form-select v-model="associatePublisherGame">
          <option v-for="publisherGame in associatePublisher.games" :key="publisherGame.publisherGameID" :value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>
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
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="associateMasterGame" :possible-games="possibleMasterGames" @input="newGameSelected"></possibleMasterGamesTable>

        <label v-if="associateMasterGame" for="associateMasterGame" class="control-label">Selected Game: {{ associateMasterGame.gameName }}</label>
      </div>
    </form>

    <form v-if="associateMasterGame" method="post" class="form-horizontal" role="form" @submit.prevent="associateGame">
      <div>
        <input type="submit" class="btn btn-primary full-width-button" value="Associate game" />
      </div>

      <div v-if="associateResult && !associateResult.success" class="alert associate-error" :class="{ 'alert-danger': !associateResult.overridable, 'alert-warning': associateResult.overridable }">
        <h3 v-if="associateResult.overridable" class="alert-heading">Warning!</h3>
        <h3 v-if="!associateResult.overridable" class="alert-heading">Error!</h3>
        <ul>
          <li v-for="error in associateResult.errors" :key="error">{{ error }}</li>
        </ul>

        <div v-if="associateResult.overridable" class="form-check">
          <span>
            <label class="text-white">Do you want to override these warnings?</label>
            <input v-model="associateOverride" class="form-check-input override-checkbox" type="checkbox" />
          </span>
        </div>
      </div>
    </form>
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
      associatePublisher: null,
      associateMasterGame: null,
      associatePublisherGame: null,
      associateOverride: false,
      associateResult: null,
      possibleMasterGames: []
    };
  },
  methods: {
    searchGame() {
      this.possibleMasterGames = [];
      this.associateResult = null;
      axios
        .get('/api/league/PossibleMasterGames?gameName=' + this.searchGameName + '&year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
        })
        .catch(() => {});
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
        .then((response) => {
          this.associateResult = response.data;
          if (!this.associateResult.success) {
            return;
          }

          this.$refs.associateGameFormRef.hide();
          this.notifyAction(this.associateMasterGame.gameName + ' sucessfully associated.');
          this.searchGameName = '';
          this.associatePublisher = null;
          this.associateMasterGame = null;
          this.associatePublisherGame = null;
          this.associateOverride = false;
          this.possibleMasterGames = [];
        })
        .catch(() => {});
    },
    clearData() {
      this.associateResult = null;
      this.searchGameName = '';
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
};
</script>
<style scoped>
.associate-error {
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
