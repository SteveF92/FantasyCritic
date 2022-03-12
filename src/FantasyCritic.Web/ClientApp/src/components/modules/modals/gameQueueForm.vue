<template>
  <b-modal id="gameQueueForm" ref="gameQueueFormRef" size="lg" title="My Watchlist" @hidden="clearAllData">
    <div class="form-group">
      <h3 class="text-black">Add Game to Watchlist</h3>
      <label for="searchGameName" class="control-label">Game Name</label>
      <div class="input-group game-search-input">
        <input v-model="searchGameName" id="searchGameName" name="searchGameName" type="text" class="form-control input" />
        <span class="input-group-btn">
          <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
        </span>
      </div>
    </div>

    <div v-if="!leagueYear.hasSpecialSlots">
      <b-button variant="secondary" v-on:click="getTopGames" class="show-top-button">Show Top Available Games</b-button>
    </div>
    <div v-else>
      <h5 class="text-black">Search by Slot</h5>
      <span class="search-tags">
        <searchSlotTypeBadge :gameSlot="leagueYear.slotInfo.overallSlot" name="ALL" v-on:click.native="getTopGames"></searchSlotTypeBadge>
        <searchSlotTypeBadge :gameSlot="leagueYear.slotInfo.regularSlot" name="REG" v-on:click.native="getGamesForSlot(leagueYear.slotInfo.regularSlot)"></searchSlotTypeBadge>
        <searchSlotTypeBadge v-for="specialSlot in leagueYear.slotInfo.specialSlots" :gameSlot="specialSlot" v-on:click.native="getGamesForSlot(specialSlot)"></searchSlotTypeBadge>
      </span>
    </div>

    <h3 class="text-black" v-show="showingTopAvailable">Top Available Games</h3>

    <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="gameToQueue" :possibleGames="possibleMasterGames" v-on:input="addGameToQueue"></possibleMasterGamesTable>

    <div v-if="queueResult && !queueResult.success" class="alert bid-error alert-danger">
      <h3 class="alert-heading">Error!</h3>
      <ul>
        <li v-for="error in queueResult.errors">{{ error }}</li>
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
          <th scope="col">Ranking</th>
          <th scope="col">Status</th>
          <th scope="col"></th>
        </tr>
      </thead>
      <draggable v-model="desiredQueueRanks" tag="tbody" handle=".handle">
        <tr v-for="queuedGame in desiredQueueRanks" :key="queuedGame.rank">
          <td scope="row" class="handle"><font-awesome-icon icon="bars" size="lg" /></td>
          <td>{{ queuedGame.masterGame.gameName }}</td>
          <td>
            <span>{{ queuedGame.masterGame.estimatedReleaseDate }}</span>
            <span v-show="queuedGame.masterGame.isReleased">(Released)</span>
          </td>
          <td>{{ queuedGame.rank }}</td>
          <td>
            <statusBadge :possibleMasterGame="queuedGame"></statusBadge>
          </td>
          <td class="select-cell">
            <b-button variant="danger" size="sm" v-on:click="removeQueuedGame(queuedGame)">Remove</b-button>
          </td>
        </tr>
      </draggable>
    </table>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Set Rankings" v-on:click="setQueueRankings" />
    </div>
  </b-modal>
</template>

<script>
import Vue from 'vue';
import axios from 'axios';
import draggable from 'vuedraggable';

import PossibleMasterGamesTable from '@/components/modules/possibleMasterGamesTable';
import StatusBadge from '@/components/modules/statusBadge';
import SearchSlotTypeBadge from '@/components/modules/gameTables/searchSlotTypeBadge';

export default {
  components: {
    draggable,
    PossibleMasterGamesTable,
    StatusBadge,
    SearchSlotTypeBadge
  },
  props: ['leagueYear', 'publisher', 'year'],
  data() {
    return {
      queuedGames: null,
      searchGameName: null,
      possibleMasterGames: [],
      queueResult: null,
      desiredQueueRanks: [],
      gameToQueue: null,
      showingTopAvailable: false,
      isBusy: false
    };
  },
  methods: {
    fetchQueuedGames() {
      axios
        .get('/api/league/CurrentQueuedGames/' + this.publisher.publisherID)
        .then((response) => {
          this.queuedGames = response.data;
          this.desiredQueueRanks = this.queuedGames;
        })
        .catch((response) => {});
    },
    searchGame() {
      this.clearDataExceptSearch();
      this.isBusy = true;

      axios
        .get('/api/league/PossibleMasterGames?gameName=' + this.searchGameName + '&year=' + this.year + '&leagueid=' + this.publisher.leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
        })
        .catch((response) => {
          this.isBusy = false;
        });
    },
    getTopGames() {
      this.clearDataExceptSearch();
      this.isBusy = true;

      axios
        .get('/api/league/TopAvailableGames?year=' + this.year + '&leagueid=' + this.publisher.leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.showingTopAvailable = true;
          this.isBusy = false;
        })
        .catch((response) => {
          this.isBusy = false;
        });
    },
    getGamesForSlot(slotInfo) {
      this.clearDataExceptSearch();
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
        .catch((response) => {
          this.isBusy = false;
        });
    },
    addGameToQueue() {
      var request = {
        publisherID: this.publisher.publisherID,
        masterGameID: this.gameToQueue.masterGameID
      };
      this.isBusy = true;
      axios
        .post('/api/league/AddGameToQueue', request)
        .then((response) => {
          this.queueResult = response.data;
          this.isBusy = false;
          this.fetchQueuedGames();
        })
        .catch((response) => {
          this.isBusy = false;
          this.errorInfo = response.response.data;
        });
    },
    setQueueRankings() {
      let desiredMasterGameIDs = this.desiredQueueRanks.map(function (v) {
        return v.masterGame.masterGameID;
      });
      var model = {
        publisherID: this.publisher.publisherID,
        QueueRanks: desiredMasterGameIDs
      };
      axios
        .post('/api/league/SetQueueRankings', model)
        .then((response) => {
          this.fetchQueuedGames();
        })
        .catch((response) => {});
    },
    removeQueuedGame(game) {
      var model = {
        publisherID: this.publisher.publisherID,
        masterGameID: game.masterGame.masterGameID
      };
      axios
        .post('/api/league/DeleteQueuedGame', model)
        .then((response) => {
          this.fetchQueuedGames();
        })
        .catch((response) => {});
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
  },
  mounted() {
    this.fetchQueuedGames();
  },
  watch: {
    queuedGames(newValue, oldValue) {
      if (!oldValue || (oldValue.constructor === Array && newValue.constructor === Array && oldValue.length !== newValue.length)) {
        this.clearQueueData();
      }
    },
    year() {
      this.fetchQueuedGames();
    }
  }
};
</script>
<style scoped>
.select-cell {
  text-align: center;
}

.search-tags {
  display: flex;
  padding: 5px;
  background: rgba(50, 50, 50, 0.7);
  border-radius: 5px;
  justify-content: space-around;
}
</style>
