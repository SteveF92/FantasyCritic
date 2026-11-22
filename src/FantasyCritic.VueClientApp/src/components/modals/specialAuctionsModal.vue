<template>
  <b-modal id="special-auctions-modal" ref="specialAuctionsModalRef" size="lg" title="Manage Special Auctions" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">
      Special auctions allow your league to process the bids for a given game before the normal bid processing time. The main reason to do this is in case of 'shadow drops', which is when a game is
      announced and then released very shortly after.
    </div>
    <div v-if="requiredLeagueYear.activeSpecialAuctions.length > 0">
      <label>Active Special Auctions</label>
      <b-table-lite :items="requiredLeagueYear.activeSpecialAuctions" :fields="activeSpecialAuctionFields" bordered responsive striped>
        <template #cell(masterGame)="data">
          <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
        </template>
        <template #cell(creationTime)="data">
          {{ formatDateTime(data.item.creationTime) }}
        </template>
        <template #cell(scheduledEndTime)="data">
          {{ formatDateTime(data.item.scheduledEndTime) }}
          <b-badge v-if="data.item.isLocked" variant="danger">Ended</b-badge>
          <b-button v-else variant="danger" @click="cancelSpecialAuction(data.item.masterGameYear)" :disabled="isBusy">Cancel</b-button>
        </template>
      </b-table-lite>
    </div>

    <form method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
      <div class="form-group">
        <label for="searchGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" :disabled="!searchGameName" @click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable
          v-if="possibleMasterGames.length > 0 && !specialAuctionMasterGame"
          v-model="specialAuctionMasterGame"
          :possible-games="possibleMasterGames"
          @update:model-value="newGameSelected"></possibleMasterGamesTable>
      </div>
    </form>
    <div v-if="specialAuctionMasterGame">
      <h3 class="selected-game text-black">Selected Game:</h3>
      <masterGameSummary :master-game="specialAuctionMasterGame"></masterGameSummary>
      <div v-if="specialAuctionMasterGame.releasingToday" class="alert alert-warning">
        This game is releasing today. It may already be released. You can make a special auction, but it's up to you to decide if there is already too much information out about this game. You should
        consider:
        <ul>
          <li>Is it playable right now?</li>
          <li>Are there already reviews?</li>
        </ul>

        It is ultimately up to you though.
      </div>
      <hr />
      <label>Auction End Time (In your local time zone)</label>
      <input v-model="scheduledEndTime" type="datetime-local" class="form-control" />
      <b-button variant="success" class="create-button" @click="createSpecialAuction" :disabled="!formIsValid || isBusy">Create Special Auction</b-button>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal, TableField } from 'bootstrap-vue-next';

import {
  leagueClient,
  leagueManagerClient,
  type CancelSpecialAuctionRequest,
  type CreateSpecialAuctionRequest,
  type MasterGameYearViewModel,
  type PossibleMasterGameYearViewModel,
  type SpecialAuctionViewModel
} from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatDateTime } from '@/utilities/formatters';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import MasterGameSummary from '@/components/masterGameSummary.vue';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';

interface SpecialAuctionsModalData {
  searchGameName: string;
  specialAuctionMasterGame: MasterGameYearViewModel | null;
  scheduledEndTime: string;
  possibleMasterGames: PossibleMasterGameYearViewModel[];
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  setup() {
    const activeSpecialAuctionFields: TableField<SpecialAuctionViewModel>[] = [
      { key: 'masterGame', label: 'Game', thClass: ['bg-primary'] },
      { key: 'creationTime', label: 'Creation Time', thClass: ['bg-primary'] },
      { key: 'scheduledEndTime', label: 'End Time', thClass: ['bg-primary'] }
    ];

    return { activeSpecialAuctionFields };
  },
  components: {
    PossibleMasterGamesTable,
    MasterGameSummary,
    MasterGamePopover
  },
  data(): SpecialAuctionsModalData {
    return {
      searchGameName: '',
      specialAuctionMasterGame: null,
      scheduledEndTime: '',
      possibleMasterGames: [],
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear']),
    formIsValid(): boolean {
      return !!this.specialAuctionMasterGame && !!this.scheduledEndTime;
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    formatDateTime,
    async createSpecialAuction(): Promise<void> {
      if (!this.scheduledEndTime || !this.specialAuctionMasterGame) {
        throw new Error('Special auction form is not valid.');
      }

      const dateObject = new Date(this.scheduledEndTime);
      const utcDate = dateObject.toISOString();

      const request: CreateSpecialAuctionRequest = {
        leagueID: this.requiredLeagueYear.leagueID,
        year: this.requiredLeagueYear.year,
        masterGameID: this.specialAuctionMasterGame.masterGameID,
        scheduledEndTime: utcDate
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.createSpecialAuction(request);
        const gameName = this.specialAuctionMasterGame.gameName;
        (this.$refs.specialAuctionsModalRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Special auction created for: ' + gameName);
        this.clearData();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when creating the special auction.');
      } finally {
        this.isBusy = false;
      }
    },
    async cancelSpecialAuction(masterGameYear: MasterGameYearViewModel): Promise<void> {
      const request: CancelSpecialAuctionRequest = {
        leagueID: this.requiredLeagueYear.leagueID,
        year: this.requiredLeagueYear.year,
        masterGameID: masterGameYear.masterGameID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.cancelSpecialAuction(request);
        await this.notifyAction('Cancelled special auction for: ' + masterGameYear.gameName);
        this.clearData();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when cancelling the special auction.');
      } finally {
        this.isBusy = false;
      }
    },
    async searchGame(): Promise<void> {
      this.specialAuctionMasterGame = null;
      this.possibleMasterGames = [];

      this.isBusy = true;
      this.errorInfo = '';
      try {
        this.possibleMasterGames = await leagueClient.possibleMasterGames(this.searchGameName, this.requiredLeagueYear.year, this.requiredLeagueYear.leagueID);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when searching.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.searchGameName = '';
      this.scheduledEndTime = '';
      this.specialAuctionMasterGame = null;
      this.possibleMasterGames = [];
      this.errorInfo = '';
    },
    newGameSelected(): void {
      this.errorInfo = '';
    }
  }
});
</script>

<style scoped>
.game-search-input {
  margin-bottom: 15px;
}

.create-button {
  margin-top: 20px;
  width: 100%;
}
</style>
