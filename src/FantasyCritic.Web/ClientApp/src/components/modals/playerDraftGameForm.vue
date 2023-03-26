<template>
  <b-modal id="playerDraftGameForm" ref="playerDraftGameFormRef" size="lg" title="Select Draft Game" hide-footer @hidden="clearData" @show="getTopGames">
    <form method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
      <label for="searchGameName" class="control-label">Game Name</label>
      <div class="input-group game-search-input">
        <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
        <span class="input-group-btn">
          <b-button variant="info" :disabled="!searchGameName" @click="searchGame">Search Game</b-button>
        </span>
      </div>

      <div v-if="!leagueYear.settings.hasSpecialSlots">
        <b-button variant="secondary" class="show-top-button" @click="getTopGames">Show Top Available Games</b-button>
        <b-button variant="secondary" class="show-top-button" @click="getQueuedGames">Show My Watchlist</b-button>
      </div>
      <div v-else>
        <b-button variant="secondary" class="show-top-button" @click="getQueuedGames">Show My Watchlist</b-button>
        <h5 class="text-black">Top Available by Slot</h5>
        <span class="search-tags">
          <searchSlotTypeBadge :game-slot="leagueYear.slotInfo.overallSlot" name="ALL" :selected="selectedSlotIndex === 0" @click.native="getTopGames"></searchSlotTypeBadge>
          <searchSlotTypeBadge
            :game-slot="leagueYear.slotInfo.regularSlot"
            name="REG"
            :selected="selectedSlotIndex === 1"
            @click.native="getGamesForSlot(leagueYear.slotInfo.regularSlot, 1)"></searchSlotTypeBadge>
          <searchSlotTypeBadge
            v-for="(specialSlot, index) in leagueYear.slotInfo.specialSlots"
            :key="specialSlot.overallSlotNumber"
            :game-slot="specialSlot"
            :selected="selectedSlotIndex === 2 + index"
            @click.native="getGamesForSlot(specialSlot, 2 + index)"></searchSlotTypeBadge>
        </span>
      </div>

      <div v-if="!draftMasterGame">
        <div v-if="isBusy" class="game-list-spinner">
          <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: 'black' }" />
        </div>

        <h3 v-show="showingTopAvailable" class="text-black">Top Available Games</h3>
        <h3 v-show="showingQueuedGames" class="text-black">Watchlist</h3>
        <h3 v-show="!showingTopAvailable && !showingQueuedGames && possibleMasterGames && possibleMasterGames.length > 0" class="text-black">Search Results</h3>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="draftMasterGame" :possible-games="possibleMasterGames" @input="newGameSelected"></possibleMasterGamesTable>
      </div>

      <div v-show="searched && !draftMasterGame" class="alert" :class="{ 'alert-info': possibleMasterGames.length > 0, 'alert-warning': possibleMasterGames.length === 0 }">
        <div>
          <span v-show="possibleMasterGames.length > 0">Don't see the game you are looking for?</span>
          <span v-show="possibleMasterGames.length === 0">No games were found.</span>
          <b-button variant="primary" size="sm" class="unlisted-button" @click="showUnlistedField">Select unlisted game</b-button>
        </div>

        <div v-if="showingUnlistedField">
          <label for="draftUnlistedGame" class="control-label">Custom Game Name</label>
          <div class="input-group game-search-input">
            <input id="draftUnlistedGame" v-model="draftUnlistedGame" name="draftUnlistedGame" type="text" class="form-control input" />
          </div>
          <div>Enter the full name of the game you want.</div>
          <div v-show="!isManager">Your league manager can link this custom game with a "master game" later.</div>
          <div v-show="isManager">You as league manager can link this custom game with a "master game" later.</div>
        </div>
      </div>

      <div v-if="draftMasterGame || draftUnlistedGame">
        <h3 v-show="draftMasterGame" for="draftMasterGame" class="selected-game text-black">Selected Game:</h3>
        <masterGameSummary v-if="draftMasterGame" :master-game="draftMasterGame"></masterGameSummary>
        <b-button v-if="formIsValid" variant="primary" class="full-width-button" :disabled="isBusy" @click="addGame">Draft Game</b-button>
        <div v-if="draftResult && !draftResult.success" class="alert draft-error" :class="{ 'alert-danger': !draftResult.showAsWarning, 'alert-warning': draftResult.showAsWarning }">
          <hr />
          <h3 v-if="draftResult.showAsWarning" class="alert-heading">Warning!</h3>
          <h3 v-else class="alert-heading">Error!</h3>
          <ul>
            <li v-for="error in draftResult.errors" :key="error">{{ error }}</li>
          </ul>

          <div v-if="draftResult.overridable" class="form-check">
            <span>
              <label v-show="!isManager" class="form-check-label">Your league manager can override these warnings.</label>
              <label v-show="isManager" class="form-check-label">
                <span class="text-white">You as league manager can override these warnings.</span>
                <br />
                <span class="text-white">Use the "Select Next Game" button under "Manager Actions".</span>
              </label>
            </span>
          </div>

          <div v-if="draftResult.noEligibleSpaceError" class="form-check">
            <div class="text-white">This game is going to end up in an ineligible slot.</div>
            <span>
              <label class="text-white">Do you want draft this game anyway anyway?</label>
              <input v-model="allowIneligibleSlot" class="form-check-input override-checkbox" type="checkbox" />
            </span>
          </div>
        </div>
      </div>
    </form>
  </b-modal>
</template>

<script>
import axios from 'axios';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';
import MasterGameSummary from '@/components/masterGameSummary.vue';
import SearchSlotTypeBadge from '@/components/gameTables/searchSlotTypeBadge.vue';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  components: {
    PossibleMasterGamesTable,
    MasterGameSummary,
    SearchSlotTypeBadge
  },
  mixins: [LeagueMixin],
  data() {
    return {
      searchGameName: null,
      draftUnlistedGame: null,
      draftMasterGame: null,
      draftResult: null,
      draftOverride: false,
      possibleMasterGames: [],
      searched: false,
      showingUnlistedField: false,
      isBusy: false,
      showingTopAvailable: false,
      showingQueuedGames: false,
      selectedSlotIndex: 0,
      allowIneligibleSlot: false
    };
  },
  computed: {
    formIsValid() {
      return this.draftUnlistedGame || this.draftMasterGame;
    }
  },
  methods: {
    searchGame() {
      this.clearDataExceptSearch();
      this.isBusy = true;
      axios
        .get('/api/league/PossibleMasterGames?gameName=' + this.searchGameName + '&year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.searched = true;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    getTopGames() {
      this.clearDataExceptSearch();
      this.selectedSlotIndex = 0;
      this.isBusy = true;
      axios
        .get('/api/league/TopAvailableGames?year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.showingTopAvailable = true;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    getGamesForSlot(slotInfo, slotIndex) {
      this.clearDataExceptSearch();
      this.selectedSlotIndex = slotIndex;
      this.isBusy = true;
      let slotJSON = JSON.stringify(slotInfo);
      let base64Slot = btoa(slotJSON);
      let urlEncodedSlot = encodeURI(base64Slot);
      axios
        .get('/api/league/TopAvailableGames?year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID + '&slotInfo=' + urlEncodedSlot)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.showingTopAvailable = true;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    getQueuedGames() {
      this.clearDataExceptSearch();
      this.isBusy = true;
      axios
        .get('/api/league/CurrentQueuedGameYears/' + this.userPublisher.publisherID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.showingQueuedGames = true;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    showUnlistedField() {
      this.showingUnlistedField = true;
      this.draftUnlistedGame = this.searchGameName;
    },
    addGame() {
      this.isBusy = true;
      var gameName = '';
      if (this.draftMasterGame !== null) {
        gameName = this.draftMasterGame.gameName;
      } else if (this.draftUnlistedGame !== null) {
        gameName = this.draftUnlistedGame;
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
        managerOverride: this.draftOverride,
        allowIneligibleSlot: this.allowIneligibleSlot
      };

      axios
        .post('/api/league/DraftGame', request)
        .then((response) => {
          this.draftResult = response.data;
          if (!this.draftResult.success) {
            this.isBusy = false;
            return;
          }

          this.$refs.playerDraftGameFormRef.hide();
          this.notifyAction('You have drafted: ' + gameName);
          this.clearData();
        })
        .catch(() => {});
    },
    clearDataExceptSearch() {
      this.isBusy = false;
      this.draftUnlistedGame = null;
      this.draftMasterGame = null;
      this.draftResult = null;
      this.draftOverride = false;
      this.possibleMasterGames = [];
      this.searched = false;
      this.showingTopAvailable = false;
      this.showingQueuedGames = false;
      this.showingUnlistedField = false;
    },
    clearData() {
      this.clearDataExceptSearch();
      this.searchGameName = null;
    },
    newGameSelected() {
      this.draftResult = null;
    }
  }
};
</script>
<style scoped>
.draft-error {
  margin-top: 10px;
}
.game-search-input {
  margin-bottom: 15px;
}
.override-checkbox {
  margin-left: 10px;
  margin-top: 8px;
}

.show-top-button {
  margin-bottom: 10px;
}

.unlisted-button {
  margin-left: 10px;
}
</style>
