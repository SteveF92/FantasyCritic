<template>
  <b-modal id="gameQueueForm" ref="gameQueueFormRef" size="lg" title="My Watchlist" @hidden="clearAllData" @show="getTopGames">
    <div class="form-group">
      <h3 class="text-black">Add Game to Watchlist</h3>
      <label for="searchGameName" class="control-label">Game Name</label>
      <div class="input-group game-search-input">
        <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
        <span class="input-group-btn">
          <b-button variant="info" :disabled="!searchGameName" @click="searchGame">Search Game</b-button>
        </span>
      </div>
    </div>

    <div v-if="!leagueYear.settings.hasSpecialSlots">
      <b-button variant="secondary" class="show-top-button" @click="getTopGames">Show Top Available Games</b-button>
    </div>
    <div v-else>
      <h5 class="text-black">Top Available by Slot</h5>
      <span class="search-tags">
        <searchSlotTypeBadge :game-slot="leagueYear.slotInfo.overallSlot" name="ALL" :selected="selectedSlotIndex === 0" @click="getTopGames"></searchSlotTypeBadge>
        <searchSlotTypeBadge
          :game-slot="leagueYear.slotInfo.regularSlot"
          name="REG"
          :selected="selectedSlotIndex === 1"
          @click="getGamesForSlot(leagueYear.slotInfo.regularSlot, 1)"></searchSlotTypeBadge>
        <searchSlotTypeBadge
          v-for="(specialSlot, index) in leagueYear.slotInfo.specialSlots"
          :key="specialSlot.overallSlotNumber"
          :game-slot="specialSlot"
          :selected="selectedSlotIndex === 2 + index"
          @click="getGamesForSlot(specialSlot, 2 + index)"></searchSlotTypeBadge>
      </span>
    </div>

    <div v-show="isBusy" class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#000000' }" />
    </div>

    <h3 v-show="showingTopAvailable" class="text-black">Top Available Games</h3>

    <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="gameToQueue" :possible-games="possibleMasterGames" @input="addGameToQueue"></possibleMasterGamesTable>

    <div v-if="queueResult && !queueResult.success" class="alert alert-danger bid-error">
      <h3 class="alert-heading">Error!</h3>
      <ul>
        <li v-for="error in queueResult.errors" :key="error">{{ error }}</li>
      </ul>
    </div>

    <hr />
    <h3 class="text-black">Current Watchlist</h3>
    <label>Drag and drop to change order.</label>
    <table class="table table-sm table-responsive-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col"></th>
          <th scope="col" class="game-column">Game</th>
          <th scope="col" class="game-column">Release Date</th>
          <th scope="col">Hype Factor</th>
          <th scope="col">Ranking</th>
          <th scope="col">Status</th>
          <th scope="col"></th>
        </tr>
      </thead>
      <draggable v-model="desiredQueueRanks" tag="tbody" handle=".handle">
        <tr v-for="queuedGame in desiredQueueRanks" :key="queuedGame.rank">
          <td scope="row" class="handle"><font-awesome-icon icon="bars" size="lg" /></td>
          <td><masterGamePopover :master-game="queuedGame.masterGame"></masterGamePopover></td>
          <td>
            <span>{{ queuedGame.masterGame.estimatedReleaseDate }}</span>
            <span v-show="queuedGame.masterGame.isReleased">(Released)</span>
          </td>
          <td>{{ queuedGame.masterGame.dateAdjustedHypeFactor | score(1) }}</td>
          <td>{{ queuedGame.rank }}</td>
          <td>
            <statusBadge :possible-master-game="queuedGame"></statusBadge>
          </td>
          <td class="select-cell">
            <b-button variant="danger" size="sm" @click="removeQueuedGame(queuedGame)">Remove</b-button>
          </td>
        </tr>
      </draggable>
    </table>
    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Set Rankings" @click="setQueueRankings" />
    </template>
  </b-modal>
</template>

<script>
import axios from 'axios';
import draggable from 'vuedraggable';

import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';
import StatusBadge from '@/components/statusBadge.vue';
import SearchSlotTypeBadge from '@/components/gameTables/searchSlotTypeBadge.vue';
import LeagueMixin from '@/mixins/leagueMixin.js';
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    draggable,
    PossibleMasterGamesTable,
    StatusBadge,
    SearchSlotTypeBadge,
    MasterGamePopover
  },
  mixins: [LeagueMixin],
  data() {
    return {
      searchGameName: null,
      possibleMasterGames: [],
      queueResult: null,
      desiredQueueRanks: [],
      gameToQueue: null,
      showingTopAvailable: false,
      isBusy: false,
      selectedSlotIndex: 0
    };
  },
  mounted() {
    this.initializeDesiredRankings();
  },
  methods: {
    initializeDesiredRankings() {
      this.desiredQueueRanks = this.queuedGames;
    },
    searchGame() {
      this.clearDataExceptSearch();
      this.isBusy = true;

      axios
        .get('/api/league/PossibleMasterGames?gameName=' + this.searchGameName + '&year=' + this.leagueYear.year + '&leagueid=' + this.userPublisher.leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
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
        .get('/api/league/TopAvailableGames?year=' + this.leagueYear.year + '&leagueid=' + this.userPublisher.leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.showingTopAvailable = true;
          this.isBusy = false;
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
    async addGameToQueue() {
      var request = {
        publisherID: this.userPublisher.publisherID,
        masterGameID: this.gameToQueue.masterGameID
      };

      this.isBusy = true;
      try {
        const response = await axios.post('/api/league/AddGameToQueue', request);
        this.queueResult = response.data;
        this.isBusy = false;
        await this.notifyAction('Game added to watchlist.');
        this.initializeDesiredRankings();
      } catch (error) {
        this.isBusy = false;
        this.errorInfo = error;
      }
    },
    async setQueueRankings() {
      let desiredMasterGameIDs = this.desiredQueueRanks.map(function (v) {
        return v.masterGame.masterGameID;
      });
      var model = {
        publisherID: this.userPublisher.publisherID,
        queueRanks: desiredMasterGameIDs
      };

      await axios.post('/api/league/SetQueueRankings', model);
      await this.notifyAction('Watchlist reordered.');
      this.initializeDesiredRankings();
    },
    async removeQueuedGame(game) {
      var model = {
        publisherID: this.userPublisher.publisherID,
        masterGameID: game.masterGame.masterGameID
      };

      await axios.post('/api/league/DeleteQueuedGame', model);
      await this.notifyAction('Game removed from watchlist.');
      this.initializeDesiredRankings();
    },
    clearAllData() {
      this.clearQueueData();
      this.searchGameName = null;
      this.possibleMasterGames = [];
      this.showingTopAvailable = false;
    },
    clearQueueData() {
      this.desiredQueueRanks = this.queuedGames;
      this.queueResult = null;
    },
    clearDataExceptSearch() {
      this.queueResult = null;
      this.possibleMasterGames = [];
      this.showingTopAvailable = false;
      this.isBusy = false;
    }
  }
};
</script>
<style scoped>
.select-cell {
  text-align: center;
}

.spinner {
  margin-top: 20px;
  text-align: center;
}

.show-top-button {
  margin-bottom: 10px;
}
</style>
