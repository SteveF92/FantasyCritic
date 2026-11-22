<template>
  <b-modal id="player-draft-game-form" ref="playerDraftGameFormRef" size="lg" title="Select Draft Game" no-footer @hidden="clearData" @show="getTopGames">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <form method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
      <label for="searchGameName" class="control-label">Game Name</label>
      <div class="input-group game-search-input">
        <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
        <span class="input-group-btn">
          <b-button variant="info" :disabled="!searchGameName" @click="searchGame">Search Game</b-button>
        </span>
      </div>

      <b-button v-if="!requiredLeagueYear.settings.hasSpecialSlots" variant="secondary" class="show-top-button" @click="getTopGames">Show Top Available Games</b-button>
      <b-button variant="secondary" class="show-top-button" @click="getQueuedGames">Show My Watchlist</b-button>

      <div v-if="requiredLeagueYear.settings.hasSpecialSlots">
        <h5 class="text-black">Top Available by Slot</h5>
        <span class="search-tags">
          <searchSlotTypeBadge :game-slot="requiredLeagueYear.slotInfo.overallSlot" name="ALL" :selected="searchMode === 'topAvailable' && !selectedSlotKey" @click="getTopGames"></searchSlotTypeBadge>
          <searchSlotTypeBadge
            :game-slot="requiredLeagueYear.slotInfo.regularSlot"
            name="REG"
            :selected="selectedSlotKey === 'regular'"
            @click="getGamesForSlot(requiredLeagueYear.slotInfo.regularSlot, 'regular')"></searchSlotTypeBadge>
          <searchSlotTypeBadge
            v-for="(specialSlot, index) in requiredLeagueYear.slotInfo.specialSlots"
            :key="index"
            :game-slot="specialSlot"
            :selected="selectedSlotKey === `special-${index}`"
            @click="getGamesForSlot(specialSlot, `special-${index}`)"></searchSlotTypeBadge>
        </span>
      </div>

      <div v-show="isBusy" class="spinner">
        <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#000000' }" />
      </div>

      <div v-if="!draftMasterGame && searchMode && possibleMasterGames.length > 0" class="search-results">
        <h3 class="text-black">{{ searchHeading }}</h3>
        <possibleMasterGamesTable v-model="draftMasterGame" :possible-games="possibleMasterGames" @update:model-value="newGameSelected"></possibleMasterGamesTable>
      </div>

      <div v-show="(searchMode && !draftMasterGame) || showingUnlistedField" class="alert" :class="{ 'alert-info': possibleMasterGames.length > 0, 'alert-warning': possibleMasterGames.length === 0 }">
        <div v-if="!showingUnlistedField">
          <span v-show="possibleMasterGames.length > 0">Don't see the game you are looking for?</span>
          <span v-show="possibleMasterGames.length === 0">No games were found.</span>
          <b-button variant="primary" size="sm" class="unlisted-button" @click="showUnlistedField">Select unlisted game</b-button>
        </div>

        <div v-if="showingUnlistedField">
          <h4>Custom Game Name</h4>
          <div class="input-group game-search-input">
            <input id="draftUnlistedGame" v-model="draftUnlistedGameName" name="draftUnlistedGame" type="text" class="form-control input" />
          </div>
          <div>Enter the full name of the game you want.</div>
          <div v-show="!isManager">Your league manager can link this custom game with a "master game" later.</div>
          <div v-show="isManager">You as league manager can link this custom game with a "master game" later.</div>
        </div>
      </div>

      <div v-if="draftMasterGame || draftUnlistedGameName">
        <h3 v-show="draftMasterGame" class="selected-game text-black">Selected Game:</h3>
        <masterGameSummary v-if="draftMasterGame" :master-game="draftMasterGame"></masterGameSummary>
        <b-button v-if="formIsValid" variant="primary" class="full-width-button" :disabled="draftRequestIsBusy" @click="addGame">Draft Game</b-button>
        <div v-if="draftResult && !draftResult.success" class="alert draft-error" :class="{ 'alert-danger': !draftResult.showAsWarning, 'alert-warning': draftResult.showAsWarning }">
          <hr />
          <h3 v-if="draftResult.showAsWarning" class="alert-heading">Warning!</h3>
          <h3 v-else class="alert-heading">Error!</h3>
          <ul>
            <li v-for="error in draftResult.errors" :key="error">{{ error }}</li>
          </ul>

          <div v-if="draftResult.overridable" class="plain-check">
            <span>
              <label v-show="!isManager" class="plain-check-label">Your league manager can override these warnings.</label>
              <label v-show="isManager" class="plain-check-label">
                <span class="text-white">You as league manager can override these warnings.</span>
                <br />
                <span class="text-white">Use the "Select Next Game" button under "Manager Actions".</span>
              </label>
            </span>
          </div>

          <div v-if="draftResult.noEligibleSpaceError" class="plain-check">
            <div class="text-white">This game is going to end up in an ineligible slot.</div>
            <span>
              <label class="text-white">Do you want to draft this game anyway?</label>
              <input v-model="allowIneligibleSlot" class="plain-check-input override-checkbox" type="checkbox" />
            </span>
          </div>
        </div>
      </div>
    </form>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import { BModal } from 'bootstrap-vue-next';

import {
  leagueClient,
  type DraftGameRequest,
  type MasterGameYearViewModel,
  type PlayerClaimResultViewModel,
  type PossibleMasterGameYearViewModel,
  type PublisherSingleSlotRequirementsViewModel
} from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { makeToast } from '@/utilities/toast';
import SearchSlotTypeBadge from '@/components/gameTables/searchSlotTypeBadge.vue';
import MasterGameSummary from '@/components/masterGameSummary.vue';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';

type SearchMode = 'search' | 'topAvailable' | 'watchlist' | null;

interface PlayerDraftGameForm {
  searchGameName: string;
  draftUnlistedGameName: string;
  draftMasterGame: MasterGameYearViewModel | null;
  draftResult: PlayerClaimResultViewModel | null;
  possibleMasterGames: PossibleMasterGameYearViewModel[];
  showingUnlistedField: boolean;
  searchMode: SearchMode;
  isBusy: boolean;
  draftRequestIsBusy: boolean;
  errorInfo: string;
  selectedSlotKey: string | null;
  allowIneligibleSlot: boolean;
  latestSearchID: number;
}

export default defineComponent({
  components: {
    PossibleMasterGamesTable,
    MasterGameSummary,
    SearchSlotTypeBadge
  },
  data(): PlayerDraftGameForm {
    return {
      searchGameName: '',
      draftUnlistedGameName: '',
      draftMasterGame: null,
      draftResult: null,
      possibleMasterGames: [],
      searchMode: null,
      showingUnlistedField: false,
      isBusy: false,
      draftRequestIsBusy: false,
      errorInfo: '',
      selectedSlotKey: null,
      allowIneligibleSlot: false,
      latestSearchID: 0
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'requiredUserPublisher', 'isManager']),
    searchHeading(): string {
      switch (this.searchMode) {
        case 'search':
          return 'Search Results';
        case 'topAvailable':
          return 'Top Available Games';
        case 'watchlist':
          return 'Watchlist';
        default:
          return '';
      }
    },
    formIsValid(): boolean {
      return !!this.draftUnlistedGameName || !!this.draftMasterGame;
    }
  },
  methods: {
    async searchGame(): Promise<void> {
      await this.fetchGames('search', null, () => leagueClient.possibleMasterGames(this.searchGameName, this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID));
    },
    async getTopGames(): Promise<void> {
      await this.fetchGames('topAvailable', null, () =>
        leagueClient.topAvailableGames(this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID, this.requiredUserPublisher.publisherID, null)
      );
    },
    async getQueuedGames(): Promise<void> {
      await this.fetchGames('watchlist', null, () => leagueClient.currentQueuedGameYears(this.requiredUserPublisher.publisherID, null));
    },
    async getGamesForSlot(slotInfo: PublisherSingleSlotRequirementsViewModel, slotKey: string): Promise<void> {
      const slotJSON = JSON.stringify(slotInfo);
      const base64Slot = btoa(slotJSON);
      await this.fetchGames('topAvailable', slotKey, () =>
        leagueClient.topAvailableGames(this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID, this.requiredUserPublisher.publisherID, base64Slot)
      );
    },
    async fetchGames(searchMode: SearchMode, slotKeyToSelect: string | null, searchAction: () => Promise<PossibleMasterGameYearViewModel[]>): Promise<void> {
      this.clearDataExceptSearch();
      this.selectedSlotKey = slotKeyToSelect;
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
    showUnlistedField(): void {
      this.showingUnlistedField = true;
      this.draftUnlistedGameName = this.searchGameName;
    },
    async addGame(): Promise<void> {
      const gameName = this.draftMasterGame?.gameName ?? this.draftUnlistedGameName;
      const request: DraftGameRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        gameName: gameName,
        counterPick: false,
        masterGameID: this.draftMasterGame?.masterGameID,
        allowIneligibleSlot: this.allowIneligibleSlot
      };

      this.draftRequestIsBusy = true;
      try {
        this.draftResult = await leagueClient.draftGame(request);
        if (!this.draftResult.success) {
          return;
        }

        (this.$refs.playerDraftGameFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        makeToast('You have drafted: ' + gameName);
        this.clearData();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to draft game.');
      } finally {
        this.draftRequestIsBusy = false;
      }
    },
    clearDataExceptSearch(): void {
      this.isBusy = false;
      this.draftUnlistedGameName = '';
      this.draftMasterGame = null;
      this.draftResult = null;
      this.possibleMasterGames = [];
      this.searchMode = null;
      this.selectedSlotKey = null;
      this.showingUnlistedField = false;
      this.allowIneligibleSlot = false;
      this.errorInfo = '';
    },
    clearData(): void {
      this.clearDataExceptSearch();
      this.searchGameName = '';
      this.latestSearchID++;
    },
    newGameSelected(): void {
      this.draftResult = null;
      this.allowIneligibleSlot = false;
    }
  }
});
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
