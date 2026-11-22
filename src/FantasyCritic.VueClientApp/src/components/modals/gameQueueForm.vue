<template>
  <b-modal id="gameQueueForm" ref="gameQueueFormRef" size="xl" title="My Watchlist" @hidden="clearAllData" @show="onOpen">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
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

    <div v-if="!requiredLeagueYear.settings.hasSpecialSlots">
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
        <searchSlotTypeBadge :game-slot="requiredLeagueYear.slotInfo.overallSlot" name="ALL" :selected="searchMode === 'topAvailable' && !selectedSlot" @click="getTopGames"></searchSlotTypeBadge>
        <searchSlotTypeBadge
          :game-slot="requiredLeagueYear.slotInfo.regularSlot"
          name="REG"
          :selected="selectedSlot === requiredLeagueYear.slotInfo.regularSlot"
          @click="getGamesForSlot(requiredLeagueYear.slotInfo.regularSlot)"></searchSlotTypeBadge>
        <searchSlotTypeBadge
          v-for="(specialSlot, index) in requiredLeagueYear.slotInfo.specialSlots"
          :key="index"
          :game-slot="specialSlot"
          :selected="selectedSlot === specialSlot"
          @click="getGamesForSlot(specialSlot)"></searchSlotTypeBadge>
      </span>
    </div>

    <div v-show="isBusy" class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#000000' }" />
    </div>

    <h3 v-if="searchHeading" class="text-black">{{ searchHeading }}</h3>

    <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="gameToQueue" :possible-games="possibleMasterGames" @update:model-value="addGameToQueue"></possibleMasterGamesTable>
    <div v-if="searchMode && possibleMasterGames.length === 0" class="alert alert-info">No games available to display.</div>

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
        <VueDraggable v-model="desiredQueueRanks" tag="tbody" handle=".handle">
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
            <td class="hype-column" data-label="Hype Factor">{{ formatScore(queuedGame.masterGame.dateAdjustedHypeFactor, 1) }}</td>
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
        </VueDraggable>
      </table>
    </div>
    <b-modal id="editWatchlistNotesModal" ref="editWatchlistNotesModalRef" :title="notesModalTitle" ok-title="Save Notes" @ok="saveNotes" @hidden="clearNotesData">
      <b-form-textarea v-model="notesInEdit" rows="8" :maxlength="maximumNotesLength"></b-form-textarea>
      <div class="notes-length">{{ notesInEdit.length }} / {{ maximumNotesLength }}</div>
    </b-modal>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Set Rankings" @click="setQueueRankings" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import { BModal } from 'bootstrap-vue-next';
import { VueDraggable } from 'vue-draggable-plus';

import {
  leagueClient,
  type AddGameToQueueRequest,
  type LeaguePublisherViewModel,
  type MasterGameYearViewModel,
  type PossibleMasterGameYearViewModel,
  type PublisherSingleSlotRequirementsViewModel,
  type QueuedGameDeleteRequest,
  type QueuedGameViewModel,
  type QueueRankingRequest,
  type QueueResultViewModel,
  type SetQueuedGameNotesRequest
} from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatScore } from '@/utilities/formatters';
import SearchSlotTypeBadge from '@/components/gameTables/searchSlotTypeBadge.vue';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';
import StatusBadge from '@/components/statusBadge.vue';

type SearchMode = 'search' | 'topAvailable' | 'otherPublisherWatchlist' | null;

interface GameQueueFormData {
  searchGameName: string;
  possibleMasterGames: PossibleMasterGameYearViewModel[];
  searchMode: SearchMode;
  errorInfo: string;
  queueResult: QueueResultViewModel | null;
  desiredQueueRanks: QueuedGameViewModel[];
  gameToQueue: MasterGameYearViewModel | null;
  isBusy: boolean;
  selectedSlot: PublisherSingleSlotRequirementsViewModel | null;
  selectedOtherPublisher: LeaguePublisherViewModel | null;
  gameToEditNotes: QueuedGameViewModel | null;
  notesInEdit: string;
  latestSearchID: number;
}

export default defineComponent({
  setup() {
    const maximumNotesLength = 1000;
    return { maximumNotesLength };
  },
  components: {
    VueDraggable,
    PossibleMasterGamesTable,
    StatusBadge,
    SearchSlotTypeBadge,
    MasterGamePopover
  },
  data(): GameQueueFormData {
    return {
      searchGameName: '',
      possibleMasterGames: [],
      errorInfo: '',
      queueResult: null,
      desiredQueueRanks: [],
      gameToQueue: null,
      searchMode: null,
      isBusy: false,
      selectedSlot: null,
      selectedOtherPublisher: null,
      gameToEditNotes: null,
      notesInEdit: '',
      latestSearchID: 0
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'requiredUserPublisher', 'queuedGames']),
    otherPublishers(): LeaguePublisherViewModel[] {
      return this.requiredLeagueYear.allPublishersForUser.filter((p) => p.publisherID !== this.requiredUserPublisher.publisherID);
    },
    notesModalTitle(): string {
      return this.gameToEditNotes ? 'Notes: ' + this.gameToEditNotes.masterGame.gameName : 'Notes';
    },
    searchHeading(): string {
      switch (this.searchMode) {
        case 'topAvailable':
          return 'Top Available Games';
        case 'otherPublisherWatchlist':
          return this.selectedOtherPublisher ? `Watchlist for ${this.selectedOtherPublisher.publisherName} in ${this.selectedOtherPublisher.leagueName}` : '';
        default:
          return '';
      }
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    formatScore,
    initializeDesiredRankings(): void {
      this.desiredQueueRanks = this.queuedGames;
    },
    async onOpen(): Promise<void> {
      this.initializeDesiredRankings();
      await this.getTopGames();
    },
    async searchGame(): Promise<void> {
      await this.fetchGames('search', null, () => leagueClient.possibleMasterGames(this.searchGameName, this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID));
    },
    async getTopGames(): Promise<void> {
      await this.fetchGames('topAvailable', null, () =>
        leagueClient.topAvailableGames(this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID, this.requiredUserPublisher.publisherID, null)
      );
    },
    async getGamesForSlot(slotInfo: PublisherSingleSlotRequirementsViewModel): Promise<void> {
      const slotJSON = JSON.stringify(slotInfo);
      const base64Slot = btoa(slotJSON);
      await this.fetchGames('topAvailable', slotInfo, () =>
        leagueClient.topAvailableGames(this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID, this.requiredUserPublisher.publisherID, base64Slot)
      );
    },
    async getOtherPublisher(otherPublisher: LeaguePublisherViewModel): Promise<void> {
      this.selectedOtherPublisher = otherPublisher;
      await this.fetchGames('otherPublisherWatchlist', null, () => leagueClient.currentQueuedGameYears(this.requiredUserPublisher.publisherID, otherPublisher.publisherID));
    },
    async fetchGames(searchMode: SearchMode, slotToSelect: PublisherSingleSlotRequirementsViewModel | null, searchAction: () => Promise<PossibleMasterGameYearViewModel[]>): Promise<void> {
      this.clearDataExceptSearch();
      this.selectedSlot = slotToSelect;
      this.isBusy = true;
      const searchID = ++this.latestSearchID;

      try {
        const games = await searchAction();
        if (searchID !== this.latestSearchID) {
          return;
        }

        this.possibleMasterGames = games;
        this.searchMode = searchMode;
      } catch (error) {
        const message = getApiErrorMessage(error, 'Failed to search for games.');
        if (searchID !== this.latestSearchID) {
          return;
        }

        this.errorInfo = message;
      } finally {
        if (searchID === this.latestSearchID) {
          this.isBusy = false;
        }
      }
    },
    async addGameToQueue(): Promise<void> {
      if (!this.gameToQueue) {
        throw new Error('No game is selected to queue.');
      }

      const request: AddGameToQueueRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        masterGameID: this.gameToQueue.masterGameID
      };

      this.errorInfo = '';
      this.isBusy = true;
      try {
        this.queueResult = await leagueClient.addGameToQueue(request);
        if (this.queueResult.success) {
          await this.notifyAction('Game added to watchlist.');
          this.initializeDesiredRankings();
        }
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to queue game.');
      } finally {
        this.isBusy = false;
      }
    },
    async setQueueRankings(): Promise<void> {
      const request: QueueRankingRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        queueRanks: this.desiredQueueRanks.map((x) => x.masterGame.masterGameID)
      };

      this.errorInfo = '';
      this.isBusy = true;
      try {
        await leagueClient.setQueueRankings(request);
        await this.notifyAction('Watchlist reordered.');
        this.initializeDesiredRankings();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to change rankings.');
      } finally {
        this.isBusy = false;
      }
    },
    startEditingNotes(queuedGame: QueuedGameViewModel) {
      this.gameToEditNotes = queuedGame;
      this.notesInEdit = queuedGame.notes ?? '';
      (this.$refs.editWatchlistNotesModalRef as InstanceType<typeof BModal> | undefined)?.show();
    },
    clearNotesData(): void {
      this.gameToEditNotes = null;
      this.notesInEdit = '';
    },
    async saveNotes(): Promise<void> {
      if (!this.gameToEditNotes) {
        throw new Error('No game is selected to edit notes for.');
      }

      const request: SetQueuedGameNotesRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        masterGameID: this.gameToEditNotes.masterGame.masterGameID,
        notes: this.notesInEdit
      };

      this.errorInfo = '';
      this.isBusy = true;
      try {
        await leagueClient.setQueuedGameNotes(request);
        await this.notifyAction('Watchlist notes saved.');
        this.initializeDesiredRankings();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to edit notes.');
      } finally {
        this.isBusy = false;
      }
    },
    async removeQueuedGame(game: QueuedGameViewModel): Promise<void> {
      const request: QueuedGameDeleteRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        masterGameID: game.masterGame.masterGameID
      };

      this.errorInfo = '';
      this.isBusy = true;
      try {
        await leagueClient.deleteQueuedGame(request);
        await this.notifyAction('Game removed from watchlist.');
        this.initializeDesiredRankings();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to remove game from watchlist.');
      } finally {
        this.isBusy = false;
      }
    },
    clearAllData(): void {
      this.clearNotesData();
      this.clearQueueData();
      this.clearDataExceptSearch();
      this.searchGameName = '';
      this.selectedOtherPublisher = null;
      this.latestSearchID++;
    },
    clearQueueData(): void {
      this.desiredQueueRanks = this.queuedGames;
      this.queueResult = null;
    },
    clearDataExceptSearch(): void {
      this.queueResult = null;
      this.possibleMasterGames = [];
      this.searchMode = null;
      this.selectedSlot = null;
      this.errorInfo = '';
      this.isBusy = false;
    }
  }
});
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
