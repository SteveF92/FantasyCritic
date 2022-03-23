<template>
  <b-modal id="claimGameForm" ref="claimGameFormRef" size="lg" title="Add Publisher Game" hide-footer @hidden="clearData">
    <div class="alert alert-warning">Warning! This feature is intended to fix mistakes and other exceptional circumstances. In general, managers should not be adding games to player's rosters.</div>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <div class="form-group">
        <label for="claimGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input v-model="searchGameName" id="searchGameName" name="searchGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="claimMasterGame" :possibleGames="possibleMasterGames" v-on:input="newGameSelected"></possibleMasterGamesTable>

        <div v-show="searched && !claimMasterGame" class="alert" v-bind:class="{ 'alert-info': possibleMasterGames.length > 0, 'alert-warning': possibleMasterGames.length === 0 }">
          <div class="row">
            <span class="col-12 col-md-7" v-show="possibleMasterGames.length > 0">Don't see the game you are looking for?</span>
            <span class="col-12 col-md-7" v-show="possibleMasterGames.length === 0">No games were found.</span>
            <b-button variant="primary" v-on:click="showUnlistedField" size="sm" class="col-12 col-md-5">Select unlisted game.</b-button>
          </div>

          <div v-if="showingUnlistedField">
            <label for="claimUnlistedGame" class="control-label">Custom Game Name</label>
            <div class="input-group game-search-input">
              <input v-model="claimUnlistedGame" id="claimUnlistedGame" name="claimUnlistedGame" type="text" class="form-control input" />
            </div>
            <div>Enter the full name of the game you want.</div>
            <div>You as league manager can link this custom game with a "master game" later.</div>
          </div>
        </div>

        <div v-if="claimMasterGame">
          <h3 for="claimMasterGame" class="selected-game text-black">Selected Game:</h3>
          <masterGameSummary :masterGame="claimMasterGame"></masterGameSummary>
        </div>
      </div>
    </form>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="addGame">
      <div class="form-group">
        <label for="claimPublisher" class="control-label">Publisher</label>
        <b-form-select v-model="claimPublisher">
          <option v-for="publisher in publishers" v-bind:value="publisher" :key="publisher.publisherID">
            {{ publisher.publisherName }}
          </option>
        </b-form-select>
      </div>
      <div class="form-check">
        <span>
          <label class="form-check-label">CounterPick</label>
          <input class="form-check-input override-checkbox" type="checkbox" v-model="claimCounterPick" />
        </span>
      </div>
      <div>
        <input type="submit" class="btn btn-primary add-game-button" value="Add game to publisher" v-if="formIsValid" />
      </div>
      <div v-if="claimResult && !claimResult.success" class="alert claim-error" v-bind:class="{ 'alert-danger': !claimResult.overridable, 'alert-warning': claimResult.overridable }">
        <h3 class="alert-heading" v-if="claimResult.overridable">Warning!</h3>
        <h3 class="alert-heading" v-if="!claimResult.overridable">Error!</h3>
        <ul>
          <li v-for="error in claimResult.errors" :key="error">{{ error }}</li>
        </ul>

        <div class="form-check" v-if="claimResult.overridable">
          <span>
            <label class="text-white">Do you want to override these warnings?</label>
            <input class="form-check-input override-checkbox" type="checkbox" v-model="claimOverride" />
          </span>
        </div>
      </div>
    </form>
  </b-modal>
</template>

<script>
import axios from 'axios';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable';
import MasterGameSummary from '@/components/masterGameSummary';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  components: {
    PossibleMasterGamesTable,
    MasterGameSummary
  },
  mixins: [LeagueMixin],
  data() {
    return {
      searchGameName: null,
      claimUnlistedGame: null,
      claimPublisher: null,
      claimMasterGame: null,
      claimResult: null,
      claimOverride: false,
      claimCounterPick: false,
      possibleMasterGames: [],
      searched: false,
      showingUnlistedField: false
    };
  },
  computed: {
    formIsValid() {
      return (this.claimUnlistedGame || this.claimMasterGame) && this.claimPublisher;
    }
  },
  methods: {
    searchGame() {
      this.clearDataExceptSearch();
      this.isBusy = true;
      axios
        .get('/api/league/PossibleMasterGames?gameName=' + this.searchGameName + '&year=' + this.year + '&leagueid=' + this.publishers[0].leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.searched = true;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    addGame() {
      var gameName = '';
      if (this.claimMasterGame !== null) {
        gameName = this.claimMasterGame.gameName;
      } else if (this.claimUnlistedGame !== null) {
        gameName = this.claimUnlistedGame;
      }

      var masterGameID = null;
      if (this.claimMasterGame !== null) {
        masterGameID = this.claimMasterGame.masterGameID;
      }

      var request = {
        publisherID: this.claimPublisher.publisherID,
        gameName: gameName,
        counterPick: this.claimCounterPick,
        masterGameID: masterGameID,
        managerOverride: this.claimOverride
      };

      axios
        .post('/api/leagueManager/ManagerClaimGame', request)
        .then((response) => {
          this.claimResult = response.data;
          if (!this.claimResult.success) {
            return;
          }

          this.$refs.claimGameFormRef.hide();
          this.notifyAction(gameName + ' added to ' + this.claimPublisher.publisherName);
          this.clearData();
        })
        .catch(() => {});
    },
    showUnlistedField() {
      this.showingUnlistedField = true;
      this.draftUnlistedGame = this.searchGameName;
    },
    clearDataExceptSearch() {
      this.claimUnlistedGame = null;
      this.claimMasterGame = null;
      this.claimResult = null;
      this.claimOverride = false;
      this.possibleMasterGames = [];
      this.searched = false;
      this.showingUnlistedField = false;
      this.claimCounterPick = false;
      this.claimPublisher = null;
    },
    clearData() {
      this.clearDataExceptSearch();
      this.searchGameName = null;
    },
    newGameSelected() {
      this.claimResult = null;
    }
  }
};
</script>
<style scoped>
.add-game-button {
  width: 100%;
}
.claim-error {
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
