<template>
  <b-modal id="manager-draft-game-form" ref="managerDraftGameFormRef" size="lg" title="Select Draft Game" no-footer @hidden="clearData" @show="getTopGames">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">
      This form will allow you to draft a game for another player. You can use this if you are running a draft off of one computer, for example. If you are just looking to draft your own games, you
      should use "Draft Game" under "Player Actions"
    </div>
    <div v-if="nextPublisherUp">
      <div class="form-group">
        <label for="nextPublisherUp" class="control-label">Select the next game for publisher:</label>
        <label>
          <strong>{{ nextPublisherUp.publisherName }} (Display Name: {{ nextPublisherUp.playerName }})</strong>
        </label>
      </div>
      <form method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
        <label for="searchGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" :disabled="!searchGameName" @click="searchGame">Search Game</b-button>
          </span>
        </div>

        <b-button v-if="!requiredLeagueYear.settings.hasSpecialSlots" variant="secondary" class="show-top-button" @click="getTopGames">Show Top Available Games</b-button>

        <div v-if="requiredLeagueYear.settings.hasSpecialSlots">
          <h5 class="text-black">Top Available by Slot</h5>
          <span class="search-tags">
            <searchSlotTypeBadge
              :game-slot="requiredLeagueYear.slotInfo.overallSlot"
              name="ALL"
              :selected="searchMode === 'topAvailable' && !selectedSlotKey"
              @click="getTopGames"></searchSlotTypeBadge>
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

        <div
          v-show="(searchMode && !draftMasterGame) || showingUnlistedField"
          class="alert"
          :class="{ 'alert-info': possibleMasterGames.length > 0, 'alert-warning': possibleMasterGames.length === 0 }">
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
            <div>You as league manager can link this custom game with a "master game" later.</div>
          </div>
        </div>
      </form>

      <div v-if="draftMasterGame || draftUnlistedGameName">
        <h3 v-show="draftMasterGame" class="selected-game text-black">Selected Game:</h3>
        <masterGameSummary v-if="draftMasterGame" :master-game="draftMasterGame"></masterGameSummary>
        <hr />
        <b-button v-if="formIsValid" variant="primary" class="full-width-button" :disabled="draftRequestIsBusy" @click="addGame">Add Game to Publisher</b-button>
        <div v-if="draftResult && !draftResult.success" class="alert draft-error" :class="{ 'alert-danger': !draftResult.showAsWarning, 'alert-warning': draftResult.showAsWarning }">
          <h3 v-if="draftResult.showAsWarning" class="alert-heading">Warning!</h3>
          <h3 v-else class="alert-heading">Error!</h3>
          <ul>
            <li v-for="error in draftResult.errors" :key="error">{{ error }}</li>
          </ul>

          <div v-if="draftResult.overridable" class="plain-check">
            <span>
              <label for="managerDraftOverride" class="text-white">Do you want to override these warnings?</label>
              <input id="managerDraftOverride" v-model="draftOverride" class="plain-check-input override-checkbox" type="checkbox" />
            </span>
          </div>

          <div v-if="draftResult.noEligibleSpaceError" class="plain-check">
            <div class="text-white">This game is going to end up in an ineligible slot.</div>
            <span>
              <label for="managerDraftAllowIneligibleSlot" class="text-white">Do you want to draft this game anyway?</label>
              <input id="managerDraftAllowIneligibleSlot" v-model="allowIneligibleSlot" class="plain-check-input override-checkbox" type="checkbox" />
            </span>
          </div>
        </div>
      </div>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import {
  leagueClient,
  leagueManagerClient,
  type ManagerClaimResultViewModel,
  type ManagerDraftGameRequest,
  type MasterGameYearViewModel,
  type PossibleMasterGameYearViewModel,
  type PublisherSingleSlotRequirementsViewModel
} from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { makeToast } from '@/utilities/toast';
import SearchSlotTypeBadge from '@/components/gameTables/searchSlotTypeBadge.vue';
import MasterGameSummary from '@/components/masterGameSummary.vue';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';

type SearchMode = 'search' | 'topAvailable' | null;

interface ManagerDraftGameFormData {
  searchGameName: string;
  draftUnlistedGameName: string;
  draftMasterGame: MasterGameYearViewModel | null;
  draftResult: ManagerClaimResultViewModel | null;
  draftOverride: boolean;
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
  data(): ManagerDraftGameFormData {
    return {
      searchGameName: '',
      draftUnlistedGameName: '',
      draftMasterGame: null,
      draftResult: null,
      draftOverride: false,
      possibleMasterGames: [],
      showingUnlistedField: false,
      searchMode: null,
      isBusy: false,
      draftRequestIsBusy: false,
      errorInfo: '',
      selectedSlotKey: null,
      allowIneligibleSlot: false,
      latestSearchID: 0
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'nextPublisherUp']),
    searchHeading(): string {
      switch (this.searchMode) {
        case 'search':
          return 'Search Results';
        case 'topAvailable':
          return 'Top Available Games';
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
      const publisher = this.nextPublisherUp;
      if (!publisher) {
        return;
      }

      await this.fetchGames('topAvailable', null, () => leagueClient.topAvailableGames(this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID, publisher.publisherID, null));
    },
    async getGamesForSlot(slotInfo: PublisherSingleSlotRequirementsViewModel, slotKey: string): Promise<void> {
      const publisher = this.nextPublisherUp;
      if (!publisher) {
        return;
      }

      const slotJSON = JSON.stringify(slotInfo);
      const base64Slot = btoa(slotJSON);
      await this.fetchGames('topAvailable', slotKey, () => leagueClient.topAvailableGames(this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID, publisher.publisherID, base64Slot));
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
      const publisher = this.nextPublisherUp;
      if (!publisher) {
        return;
      }

      const gameName = this.draftMasterGame?.gameName ?? this.draftUnlistedGameName;
      const request: ManagerDraftGameRequest = {
        publisherID: publisher.publisherID,
        gameName: gameName,
        counterPick: false,
        masterGameID: this.draftMasterGame?.masterGameID,
        managerOverride: this.draftOverride,
        allowIneligibleSlot: this.allowIneligibleSlot
      };

      this.draftRequestIsBusy = true;
      this.errorInfo = '';
      try {
        this.draftResult = await leagueManagerClient.managerDraftGame(request);
        if (!this.draftResult.success) {
          return;
        }

        (this.$refs.managerDraftGameFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        makeToast(gameName + ' drafted by ' + publisher.publisherName);
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
      this.draftOverride = false;
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
      this.draftOverride = false;
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
