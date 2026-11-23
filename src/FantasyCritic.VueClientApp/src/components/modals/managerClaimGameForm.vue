<template>
  <b-modal id="manager-claim-game-form" ref="managerClaimGameFormRef" size="lg" title="Add Publisher Game" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-warning">Warning! This feature is intended to fix mistakes and other exceptional circumstances. In general, managers should not be adding games to player's rosters.</div>
    <form method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
      <div class="form-group">
        <label for="searchGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" :disabled="!searchGameName || isBusy" @click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable
          v-if="possibleMasterGames.length > 0"
          v-model="claimMasterGame"
          :possible-games="possibleMasterGames"
          @update:model-value="newGameSelected"></possibleMasterGamesTable>

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
          <h3 class="selected-game text-black">Selected Game:</h3>
          <masterGameSummary :master-game="claimMasterGame"></masterGameSummary>
        </div>
      </div>
    </form>
    <form method="post" class="form-horizontal" role="form" @submit.prevent="addGame">
      <div class="form-group">
        <label for="claimPublisher" class="control-label">Publisher</label>
        <b-form-select id="claimPublisher" v-model="claimPublisher">
          <option v-for="publisher in requiredLeagueYear.publishers" :key="publisher.publisherID" :value="publisher">
            {{ publisher.publisherName }}
          </option>
        </b-form-select>
      </div>
      <div class="plain-check">
        <span>
          <label for="claimCounterPick" class="plain-check-label">Counter Pick</label>
          <input id="claimCounterPick" v-model="claimCounterPick" class="plain-check-input override-checkbox" type="checkbox" />
        </span>
      </div>
      <div>
        <input v-if="formIsValid" type="submit" class="btn btn-primary full-width-button" value="Add game to publisher" :disabled="isBusy" />
      </div>
      <div v-if="claimResult && !claimResult.success" class="alert claim-error" :class="{ 'alert-danger': !claimResult.showAsWarning, 'alert-warning': claimResult.showAsWarning }">
        <h3 v-if="claimResult.showAsWarning" class="alert-heading">Warning!</h3>
        <h3 v-else class="alert-heading">Error!</h3>
        <ul>
          <li v-for="error in claimResult.errors" :key="error">{{ error }}</li>
        </ul>

        <div v-if="claimResult.overridable" class="plain-check">
          <span>
            <label for="claimOverride" class="text-white">Do you want to override these warnings?</label>
            <input id="claimOverride" v-model="claimOverride" class="plain-check-input override-checkbox" type="checkbox" />
          </span>
        </div>

        <div v-if="claimResult.noEligibleSpaceError" class="plain-check">
          <div class="text-white">This game is going to end up in an ineligible slot.</div>
          <span>
            <label for="claimAllowIneligibleSlot" class="text-white">Do you want to add this game anyway?</label>
            <input id="claimAllowIneligibleSlot" v-model="allowIneligibleSlot" class="plain-check-input override-checkbox" type="checkbox" />
          </span>
        </div>
      </div>
    </form>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import {
  leagueClient,
  leagueManagerClient,
  type ClaimGameRequest,
  type ManagerClaimResultViewModel,
  type MasterGameYearViewModel,
  type PossibleMasterGameYearViewModel,
  type PublisherViewModel
} from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import MasterGameSummary from '@/components/masterGameSummary.vue';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';

interface ManagerClaimGameFormData {
  searchGameName: string;
  claimUnlistedGame: string;
  claimPublisher: PublisherViewModel | null;
  claimMasterGame: MasterGameYearViewModel | null;
  claimResult: ManagerClaimResultViewModel | null;
  claimOverride: boolean;
  claimCounterPick: boolean;
  possibleMasterGames: PossibleMasterGameYearViewModel[];
  searched: boolean;
  showingUnlistedField: boolean;
  allowIneligibleSlot: boolean;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  components: {
    PossibleMasterGamesTable,
    MasterGameSummary
  },
  data(): ManagerClaimGameFormData {
    return {
      searchGameName: '',
      claimUnlistedGame: '',
      claimPublisher: null,
      claimMasterGame: null,
      claimResult: null,
      claimOverride: false,
      claimCounterPick: false,
      possibleMasterGames: [],
      searched: false,
      showingUnlistedField: false,
      allowIneligibleSlot: false,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear']),
    formIsValid(): boolean {
      return (!!this.claimUnlistedGame || !!this.claimMasterGame) && !!this.claimPublisher;
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async searchGame(): Promise<void> {
      this.clearDataExceptSearch();

      this.isBusy = true;
      try {
        this.possibleMasterGames = await leagueClient.possibleMasterGames(this.searchGameName, this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID);
        this.searched = true;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when searching.');
      } finally {
        this.isBusy = false;
      }
    },
    async addGame(): Promise<void> {
      const publisher = this.claimPublisher;
      if (!publisher) {
        return;
      }

      const gameName = this.claimMasterGame?.gameName ?? this.claimUnlistedGame;
      const request: ClaimGameRequest = {
        publisherID: publisher.publisherID,
        gameName: gameName,
        counterPick: this.claimCounterPick,
        masterGameID: this.claimMasterGame?.masterGameID,
        managerOverride: this.claimOverride,
        allowIneligibleSlot: this.allowIneligibleSlot
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        this.claimResult = await leagueManagerClient.managerClaimGame(request);
        if (!this.claimResult.success) {
          return;
        }

        (this.$refs.managerClaimGameFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction(gameName + ' added to ' + publisher.publisherName);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when adding the game.');
      } finally {
        this.isBusy = false;
      }
    },
    showUnlistedField(): void {
      this.showingUnlistedField = true;
      this.claimUnlistedGame = this.searchGameName;
    },
    clearDataExceptSearch(): void {
      this.claimUnlistedGame = '';
      this.claimMasterGame = null;
      this.claimResult = null;
      this.claimOverride = false;
      this.possibleMasterGames = [];
      this.searched = false;
      this.showingUnlistedField = false;
      this.claimCounterPick = false;
      this.claimPublisher = null;
      this.allowIneligibleSlot = false;
      this.errorInfo = '';
    },
    clearData(): void {
      this.clearDataExceptSearch();
      this.searchGameName = '';
    },
    newGameSelected(): void {
      this.claimResult = null;
      this.claimOverride = false;
      this.allowIneligibleSlot = false;
    }
  }
});
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
  width: 1.25rem;
  height: 1.25rem;
}
</style>
