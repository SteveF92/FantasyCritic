<template>
  <b-modal id="claimGameForm" ref="claimGameFormRef" size="lg" title="Add Publisher Game" hide-footer @hidden="clearData">
    <div class="alert alert-warning">Warning! This feature is intended to fix mistakes and other exceptional circumstances. In general, managers should not be adding games to player's rosters.</div>
    <form method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
      <div class="form-group">
        <label for="claimGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" :disabled="!searchGameName" @click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="claimMasterGame" :possible-games="possibleMasterGames" @input="newGameSelected"></possibleMasterGamesTable>

        <div v-show="searched && !claimMasterGame" class="alert" :class="{ 'alert-info': possibleMasterGames.length > 0, 'alert-warning': possibleMasterGames.length === 0 }">
          <div class="row">
            <span v-show="possibleMasterGames.length > 0" class="col-12 col-md-7">Don't see the game you are looking for?</span>
            <span v-show="possibleMasterGames.length === 0" class="col-12 col-md-7">No games were found.</span>
            <b-button variant="primary" size="sm" class="col-12 col-md-5" @click="showUnlistedField">Select unlisted game.</b-button>
          </div>

          <div v-if="showingUnlistedField">
            <label for="claimUnlistedGame" class="control-label">Custom Game Name</label>
            <div class="input-group game-search-input">
              <input id="claimUnlistedGame" v-model="claimUnlistedGame" name="claimUnlistedGame" type="text" class="form-control input" />
            </div>
            <div>Enter the full name of the game you want.</div>
            <div>You as league manager can link this custom game with a "master game" later.</div>
          </div>
        </div>

        <div v-if="claimMasterGame">
          <h3 for="claimMasterGame" class="selected-game text-black">Selected Game:</h3>
          <masterGameSummary :master-game="claimMasterGame"></masterGameSummary>
        </div>
      </div>
    </form>
    <form method="post" class="form-horizontal" role="form" @submit.prevent="addGame">
      <div class="form-group">
        <label for="claimPublisher" class="control-label">Publisher</label>
        <b-form-select v-model="claimPublisher">
          <option v-for="publisher in publishers" :key="publisher.publisherID" :value="publisher">
            {{ publisher.publisherName }}
          </option>
        </b-form-select>
      </div>
      <div class="form-check">
        <span>
          <label class="form-check-label">CounterPick</label>
          <input v-model="claimCounterPick" class="form-check-input override-checkbox" type="checkbox" />
        </span>
      </div>
      <div>
        <input v-if="formIsValid" type="submit" class="btn btn-primary full-width-button" value="Add game to publisher" />
      </div>
      <div v-if="claimResult && !claimResult.success" class="alert claim-error" :class="{ 'alert-danger': !claimResult.showAsWarning, 'alert-warning': claimResult.showAsWarning }">
        <h3 v-if="claimResult.showAsWarning" class="alert-heading">Warning!</h3>
        <h3 v-else class="alert-heading">Error!</h3>
        <ul>
          <li v-for="error in claimResult.errors" :key="error">{{ error }}</li>
        </ul>

        <div v-if="claimResult.overridable" class="form-check">
          <span>
            <label class="text-white">Do you want to override these warnings?</label>
            <input v-model="claimOverride" class="form-check-input override-checkbox" type="checkbox" />
          </span>
        </div>

        <div v-if="claimResult.noEligibleSpaceError" class="form-check">
          <div class="text-white">This game is going to end up in an ineligible slot.</div>
          <span>
            <label class="text-white">Do you want add this game anyway anyway?</label>
            <input v-model="allowIneligibleSlot" class="form-check-input override-checkbox" type="checkbox" />
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
      showingUnlistedField: false,
      allowIneligibleSlot: false
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
        .get('/api/league/PossibleMasterGames?gameName=' + this.searchGameName + '&year=' + this.leagueYear.year + '&leagueid=' + this.publishers[0].leagueID)
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
        managerOverride: this.claimOverride,
        allowIneligibleSlot: this.allowIneligibleSlot
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
