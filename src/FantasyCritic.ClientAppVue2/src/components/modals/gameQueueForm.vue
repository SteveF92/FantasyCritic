<template>
  <b-modal id="gameQueueForm" ref="gameQueueFormRef" size="xl" title="My Watchlist" @hidden="clearAllData" @show="onOpen">
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
    <div class="table-responsive">
      <table class="table table-sm table-bordered table-striped watchlist-table">
        <thead>
          <tr class="bg-primary">
            <th scope="col"></th>
            <th scope="col" class="game-column">Game</th>
            <th scope="col" class="game-column">Release Date</th>
            <th scope="col" class="hype-column">Hype Factor</th>
            <th scope="col">Notes</th>
            <th scope="col">Status</th>
            <th scope="col"></th>
          </tr>
        </thead>
        <draggable v-model="desiredQueueRanks" tag="tbody" handle=".handle">
          <tr v-for="queuedGame in desiredQueueRanks" :key="queuedGame.rank">
            <td scope="row" class="handle">
              <font-awesome-icon icon="bars" size="lg" />
              <span class="handle-rank">{{ queuedGame.rank }}</span>
            </td>
            <td class="game-cell"><masterGamePopover :master-game="queuedGame.masterGame"></masterGamePopover></td>
            <td data-label="Release Date">
              <span>{{ queuedGame.masterGame.estimatedReleaseDate }}</span>
              <span v-show="queuedGame.masterGame.isReleased">(Released)</span>
            </td>
            <td class="hype-column" data-label="Hype Factor">{{ queuedGame.masterGame.dateAdjustedHypeFactor | score(1) }}</td>
            <td class="notes-cell" data-label="Notes">
              <div class="notes-display">
                <span v-if="queuedGame.notes" class="notes-text">{{ queuedGame.notes }}</span>
                <span v-else class="no-notes">No notes</span>
                <b-button :variant="queuedGame.notes ? 'info' : 'secondary'" size="sm" title="Edit notes" @click="startEditingNotes(queuedGame)">
                  <font-awesome-icon icon="pen" />
                  <span class="button-label">Edit Notes</span>
                </b-button>
              </div>
            </td>
            <td data-label="Status">
              <statusBadge :possible-master-game="queuedGame"></statusBadge>
            </td>
            <td class="select-cell">
              <b-button variant="danger" size="sm" title="Remove from watchlist" @click="removeQueuedGame(queuedGame)">
                <font-awesome-icon icon="minus-circle" class="remove-icon" />
                <span class="button-label">Remove</span>
              </b-button>
            </td>
          </tr>
        </draggable>
      </table>
    </div>
    <b-modal id="editWatchlistNotesModal" :title="notesModalTitle" ok-title="Save Notes" @ok="saveNotes" @hidden="clearNotesData">
      <b-form-textarea v-model="notesInEdit" rows="8" :maxlength="maximumNotesLength"></b-form-textarea>
      <div class="notes-length">{{ notesInEdit.length }} / {{ maximumNotesLength }}</div>
    </b-modal>
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
      selectedOtherPublisher: null,
      gameToEditNotes: null,
      notesInEdit: '',
      maximumNotesLength: 1000
    };
  },
  computed: {
    otherPublishers() {
      return this.leagueYear.allPublishersForUser.filter((p) => p.publisherID !== this.userPublisher.publisherID);
    },
    notesModalTitle() {
      return this.gameToEditNotes ? 'Notes: ' + this.gameToEditNotes.masterGame.gameName : 'Notes';
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
      } catch {
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
    startEditingNotes(queuedGame) {
      this.gameToEditNotes = queuedGame;
      this.notesInEdit = queuedGame.notes ?? '';
      this.$bvModal.show('editWatchlistNotesModal');
    },
    clearNotesData() {
      this.gameToEditNotes = null;
      this.notesInEdit = '';
    },
    async saveNotes() {
      const model = {
        publisherID: this.userPublisher.publisherID,
        masterGameID: this.gameToEditNotes.masterGame.masterGameID,
        notes: this.notesInEdit
      };

      await axios.post('/api/league/SetQueuedGameNotes', model);
      await this.notifyAction('Watchlist notes saved.');
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
      this.clearNotesData();
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

.handle {
  white-space: nowrap;
}

.handle-rank {
  margin-left: 6px;
  font-weight: bold;
}

.notes-cell {
  vertical-align: middle;
  max-width: 250px;
}

/* Without a floor, auto table layout crushes these two columns down to a few
   characters and pushes the rest of the table into a horizontal scroll. */
@media only screen and (min-width: 992px) {
  .game-column {
    min-width: 115px;
  }

  .notes-cell {
    min-width: 140px;
  }
}

.notes-display {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 5px;
}

.notes-text {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  overflow-wrap: anywhere;
}

.no-notes {
  font-style: italic;
}

/* Both action buttons carry an icon and a text label, and each width shows
   whichever of the two it has room for. */
.remove-icon {
  display: none;
}

.notes-display .button-label {
  display: none;
}

/* The watchlist table is already crowded, so below the bootstrap 'lg' breakpoint
   hype factor drops out, the notes preview goes with it — the button color is
   then what signals which games have notes — and Remove shrinks to its icon. */
@media only screen and (max-width: 991px) {
  .hype-column,
  .notes-text,
  .no-notes {
    display: none;
  }

  .notes-display {
    justify-content: center;
  }

  .select-cell .button-label {
    display: none;
  }

  .remove-icon {
    display: inline-block;
  }
}

/* Below 'md' there is no width left to take away, so each row stops being a row
   and becomes a card: headers off, cells stacked, and every cell labelled by the
   header it lost. Vertical space is cheap here, so the notes preview and both
   button labels come back. */
@media only screen and (max-width: 767px) {
  .watchlist-table thead {
    display: none;
  }

  /* Flex so the two action cells can be ordered below the game's details,
     rather than Notes splitting them the way the column order would. */
  .watchlist-table tr {
    display: flex;
    flex-direction: column;
    margin-bottom: 10px;
    border: 1px solid #6c757d;
  }

  .watchlist-table td.notes-cell {
    order: 1;
  }

  .watchlist-table td.select-cell {
    order: 2;
  }

  .watchlist-table td {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 10px;
    border: none;
    border-bottom: 1px solid rgba(128, 128, 128, 0.3);
  }

  .watchlist-table td:last-child {
    border-bottom: none;
  }

  .watchlist-table td[data-label]::before {
    content: attr(data-label);
    font-weight: bold;
    white-space: nowrap;
    text-align: left;
  }

  /* Nothing is competing for width in a card, so hype factor earns its line back. */
  .watchlist-table td.hype-column {
    display: flex;
  }

  .watchlist-table td.handle,
  .watchlist-table td.game-cell {
    justify-content: flex-start;
  }

  .watchlist-table td.game-cell {
    font-size: 16px;
    font-weight: bold;
  }

  .watchlist-table td.notes-cell {
    flex-direction: column;
    align-items: stretch;
    max-width: none;
  }

  .notes-display {
    flex-direction: column;
    align-items: stretch;
    gap: 8px;
  }

  .watchlist-table .notes-text,
  .watchlist-table .no-notes {
    display: -webkit-box;
    -webkit-line-clamp: 4;
    -webkit-box-orient: vertical;
  }

  .select-cell .button-label,
  .notes-display .button-label {
    display: inline;
    margin-left: 6px;
  }

  .remove-icon {
    display: none;
  }

  /* Comfortable tap targets, and nothing to line up with, so both go full width. */
  .notes-display .btn,
  .select-cell .btn {
    width: 100%;
    padding: 8px 12px;
  }
}

.notes-length {
  margin-top: 5px;
  text-align: right;
  font-size: 12px;
}
</style>
