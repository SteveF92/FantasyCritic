<template>
  <b-modal id="gameQueueForm" ref="gameQueueFormRef" size="lg" title="My Watchlist" @hidden="clearAllData" @show="onOpen">
    <div class="form-group">
      <h3 class="text-black">Add Game to Watchlist</h3>
      <form class="form-horizontal" role="form" @submit.prevent="searchGame">
        <label for="searchGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" :disabled="!searchGameName" @click="searchGame">Search Game</b-button>
          </span>
        </div>
      </form>
    </div>

    <div v-if="!leagueYear.settings.hasSpecialSlots">
      <div class="watchlist-flex-area">
        <b-button variant="secondary" class="show-top-button" @click="getTopGames">Top Available Games</b-button>
        <b-dropdown text="My Other Watchlists">
          <b-dropdown-item v-for="publisher in otherPublishers" :key="publisher.publisherID" @click="getOtherPublisher(publisher)">
            <div>{{ publisher.leagueName }}</div>
            <div class="publisher-name">{{ publisher.publisherName }}</div>
          </b-dropdown-item>
        </b-dropdown>
      </div>
    </div>
    <div v-else>
      <div class="watchlist-flex-area">
        <h5 class="text-black">Top Available by Slot</h5>
        <b-dropdown text="My Other Watchlists">
          <b-dropdown-item v-for="publisher in otherPublishers" :key="publisher.publisherID" @click="getOtherPublisher(publisher)">
            <div>{{ publisher.leagueName }}</div>
            <div class="publisher-name">{{ publisher.publisherName }}</div>
          </b-dropdown-item>
        </b-dropdown>
      </div>
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

    <div v-show="isBusy" class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#000000' }" />
    </div>

    <h3 v-show="showingTopAvailable" class="text-black">Top Available Games</h3>
    <h3 v-if="showingOtherLeagueWatchlist" class="text-black">Watchlist for {{ selectedOtherPublisher.publisherName }} in {{ selectedOtherPublisher.leagueName }}</h3>

    <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="gameToQueue" :possible-games="possibleMasterGames" @input="addGameToQueue"></possibleMasterGamesTable>
    <div v-if="possibleMasterGames.length === 0" class="alert alert-info">No games available to display.</div>

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
      showingOtherLeagueWatchlist: false,
      isBusy: false,
      selectedSlotIndex: 0,
      selectedOtherPublisher: null
    };
  },
  computed: {
    otherPublishers() {
      return this.leagueYear.allPublishersForUser.filter((p) => p.publisherID !== this.userPublisher.publisherID);
    }
  },
  async created() {
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
    async onOpen() {
      await this.$store.dispatch('refreshLeagueYear');
      await this.getTopGames();
    },
    async getTopGames() {
      this.clearDataExceptSearch();
      this.selectedSlotIndex = 0;
      this.isBusy = true;

      try {
        const response = await axios.get('/api/league/TopAvailableGames?year=' + this.leagueYear.year + '&leagueid=' + this.userPublisher.leagueID + '&publisherid=' + this.userPublisher.publisherID);
        this.possibleMasterGames = response.data;
        this.showingTopAvailable = true;
        this.isBusy = false;
      } catch (error) {
        this.isBusy = false;
      }
    },
    getOtherPublisher(otherPublisher) {
      this.clearDataExceptSearch();

      this.selectedSlotIndex = 0;
      this.isBusy = true;

      axios
        .get('/api/league/CurrentQueuedGameYears/' + this.userPublisher.publisherID + `?otherPublisherID=${otherPublisher.publisherID}`)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.selectedOtherPublisher = otherPublisher;
          this.showingOtherLeagueWatchlist = true;
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
        .get('/api/league/TopAvailableGames?year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID + '&publisherid=' + this.userPublisher.publisherID + '&slotInfo=' + urlEncodedSlot)
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
      const request = {
        publisherID: this.userPublisher.publisherID,
        masterGameID: this.gameToQueue.masterGameID
      };

      this.isBusy = true;
      try {
        const response = await axios.post('/api/league/AddGameToQueue', request);
        this.queueResult = response.data;
        if (this.queueResult.success) {
          await this.notifyAction('Game added to watchlist.');
          this.initializeDesiredRankings();
        }
        this.isBusy = false;
      } catch (error) {
        this.isBusy = false;
        this.errorInfo = error;
      }
    },
    async setQueueRankings() {
      let desiredMasterGameIDs = this.desiredQueueRanks.map(function (v) {
        return v.masterGame.masterGameID;
      });
      const model = {
        publisherID: this.userPublisher.publisherID,
        queueRanks: desiredMasterGameIDs
      };

      await axios.post('/api/league/SetQueueRankings', model);
      await this.notifyAction('Watchlist reordered.');
      this.initializeDesiredRankings();
    },
    async removeQueuedGame(game) {
      const model = {
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
      this.showingOtherLeagueWatchlist = false;
      this.selectedOtherPublisher = null;
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

.watchlist-flex-area {
  display: flex;
  justify-content: space-between;
  align-items: end;
  margin-bottom: 10px;
}

.publisher-name {
  font-style: italic;
  font-size: 12px;
}
</style>
