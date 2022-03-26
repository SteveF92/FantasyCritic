<template>
  <b-modal id="gameQueueForm" ref="gameQueueFormRef" size="lg" title="My Watchlist" @hidden="clearAllData">
    <div class="form-group">
      <h3 class="text-black">Add Game to Watchlist</h3>
      <label for="searchGameName" class="control-label">Game Name</label>
      <div class="input-group game-search-input">
        <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
        <span class="input-group-btn">
          <b-button variant="info" @click="searchGame">Search Game</b-button>
        </span>
      </div>
    </div>

    <div v-if="!leagueYear.hasSpecialSlots">
      <b-button variant="secondary" class="show-top-button" @click="getTopGames">Show Top Available Games</b-button>
    </div>
    <div v-else>
      <h5 class="text-black">Search by Slot</h5>
      <span class="search-tags">
        <searchSlotTypeBadge :game-slot="leagueYear.slotInfo.overallSlot" name="ALL" @click.native="getTopGames"></searchSlotTypeBadge>
        <searchSlotTypeBadge :game-slot="leagueYear.slotInfo.regularSlot" name="REG" @click.native="getGamesForSlot(leagueYear.slotInfo.regularSlot)"></searchSlotTypeBadge>
        <searchSlotTypeBadge
          v-for="specialSlot in leagueYear.slotInfo.specialSlots"
          :key="specialSlot.overallSlotNumber"
          :game-slot="specialSlot"
          @click.native="getGamesForSlot(specialSlot)"></searchSlotTypeBadge>
      </span>
    </div>

    <h3 v-show="showingTopAvailable" class="text-black">Top Available Games</h3>

    <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="gameToQueue" :possible-games="possibleMasterGames" @input="addGameToQueue"></possibleMasterGamesTable>

    <div v-if="queueResult && !queueResult.success" class="alert bid-error alert-danger">
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
            <statusBadge :possible-master-game="queuedGame"></statusBadge>
          </td>
          <td class="select-cell">
            <b-button variant="danger" size="sm" @click="removeQueuedGame(queuedGame)">Remove</b-button>
          </td>
        </tr>
      </draggable>
    </table>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Set Rankings" @click="setQueueRankings" />
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import draggable from 'vuedraggable';

import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable';
import StatusBadge from '@/components/statusBadge';
import SearchSlotTypeBadge from '@/components/gameTables/searchSlotTypeBadge';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  components: {
    draggable,
    PossibleMasterGamesTable,
    StatusBadge,
    SearchSlotTypeBadge
  },
  mixins: [LeagueMixin],
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
  watch: {
    queuedGames(newValue, oldValue) {
      if (!oldValue || (oldValue.constructor === Array && newValue.constructor === Array && oldValue.length !== newValue.length)) {
        this.clearQueueData();
      }
    },
    year() {
      this.fetchQueuedGames();
    }
  },
  mounted() {
    this.fetchQueuedGames();
  },
  methods: {
    fetchQueuedGames() {
      axios
        .get('/api/league/CurrentQueuedGames/' + this.userPublisher.publisherID)
        .then((response) => {
          this.queuedGames = response.data;
          this.desiredQueueRanks = this.queuedGames;
        })
        .catch(() => {});
    },
    searchGame() {
      this.clearDataExceptSearch();
      this.isBusy = true;

      axios
        .get('/api/league/PossibleMasterGames?gameName=' + this.searchGameName + '&year=' + this.year + '&leagueid=' + this.userPublisher.leagueID)
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
      this.isBusy = true;

      axios
        .get('/api/league/TopAvailableGames?year=' + this.year + '&leagueid=' + this.userPublisher.leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.showingTopAvailable = true;
          this.isBusy = false;
        })
        .catch(() => {
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
        .catch(() => {
          this.isBusy = false;
        });
    },
    addGameToQueue() {
      var request = {
        publisherID: this.userPublisher.publisherID,
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
        publisherID: this.userPublisher.publisherID,
        QueueRanks: desiredMasterGameIDs
      };
      axios
        .post('/api/league/SetQueueRankings', model)
        .then(() => {
          this.fetchQueuedGames();
        })
        .catch(() => {});
    },
    removeQueuedGame(game) {
      var model = {
        publisherID: this.userPublisher.publisherID,
        masterGameID: game.masterGame.masterGameID
      };
      axios
        .post('/api/league/DeleteQueuedGame', model)
        .then(() => {
          this.fetchQueuedGames();
        })
        .catch(() => {});
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

.search-tags {
  display: flex;
  padding: 5px;
  background: rgba(50, 50, 50, 0.7);
  border-radius: 5px;
  justify-content: space-around;
}
</style>
