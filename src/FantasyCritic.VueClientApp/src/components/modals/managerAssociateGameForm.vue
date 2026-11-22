<template>
  <b-modal id="manager-associate-game-form" ref="managerAssociateGameFormRef" size="lg" title="Associate Publisher Game" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">This form allows you to link a game that currently says "Not Linked to Master Game" with the correct master game.</div>
    <div class="form-group">
      <label for="associatePublisher" class="control-label">Publisher</label>
      <b-form-select id="associatePublisher" v-model="associatePublisher" @update:model-value="publisherSelected()">
        <option v-for="publisher in requiredLeagueYear.publishers" :key="publisher.publisherID" :value="publisher">
          {{ publisher.publisherName }}
        </option>
      </b-form-select>
      <div v-if="associatePublisher">
        <label for="associatePublisherGame" class="control-label">Game</label>
        <b-form-select id="associatePublisherGame" v-model="associatePublisherGame" @update:model-value="newGameSelected()">
          <option v-for="publisherGame in associatePublisher.games" :key="publisherGame.publisherGameID" :value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>
    </div>

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
          v-model="associateMasterGame"
          :possible-games="possibleMasterGames"
          @update:model-value="newGameSelected"></possibleMasterGamesTable>

        <label v-if="associateMasterGame" class="control-label">Selected Game: {{ associateMasterGame.gameName }}</label>
      </div>
    </form>

    <form v-if="associateMasterGame" method="post" class="form-horizontal" role="form" @submit.prevent="associateGame">
      <div>
        <input type="submit" class="btn btn-primary full-width-button" value="Associate game" :disabled="!associatePublisherGame || isBusy" />
      </div>

      <div v-if="associateResult && !associateResult.success" class="alert associate-error" :class="{ 'alert-danger': !associateResult.overridable, 'alert-warning': associateResult.overridable }">
        <h3 v-if="associateResult.overridable" class="alert-heading">Warning!</h3>
        <h3 v-else class="alert-heading">Error!</h3>
        <ul>
          <li v-for="error in associateResult.errors" :key="error">{{ error }}</li>
        </ul>

        <div v-if="associateResult.overridable" class="plain-check">
          <span>
            <label for="associateOverride" class="text-white">Do you want to override these warnings?</label>
            <input id="associateOverride" v-model="associateOverride" class="plain-check-input override-checkbox" type="checkbox" />
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
  type AssociateGameRequest,
  type ManagerClaimResultViewModel,
  type MasterGameYearViewModel,
  type PossibleMasterGameYearViewModel,
  type PublisherGameViewModel,
  type PublisherViewModel
} from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';

interface ManagerAssociateGameFormData {
  searchGameName: string;
  associatePublisher: PublisherViewModel | null;
  associatePublisherGame: PublisherGameViewModel | null;
  associateMasterGame: MasterGameYearViewModel | null;
  associateOverride: boolean;
  associateResult: ManagerClaimResultViewModel | null;
  possibleMasterGames: PossibleMasterGameYearViewModel[];
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  components: {
    PossibleMasterGamesTable
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  },
  data(): ManagerAssociateGameFormData {
    return {
      searchGameName: '',
      associatePublisher: null,
      associatePublisherGame: null,
      associateMasterGame: null,
      associateOverride: false,
      associateResult: null,
      possibleMasterGames: [],
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async searchGame(): Promise<void> {
      this.possibleMasterGames = [];
      this.associateResult = null;

      this.isBusy = true;
      this.errorInfo = '';
      try {
        this.possibleMasterGames = await leagueClient.possibleMasterGames(this.searchGameName, this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when searching.');
      } finally {
        this.isBusy = false;
      }
    },
    async associateGame(): Promise<void> {
      const publisher = this.associatePublisher;
      const publisherGame = this.associatePublisherGame;
      const masterGame = this.associateMasterGame;
      if (!publisher || !publisherGame || !masterGame) {
        return;
      }

      const request: AssociateGameRequest = {
        publisherID: publisher.publisherID,
        publisherGameID: publisherGame.publisherGameID,
        masterGameID: masterGame.masterGameID,
        managerOverride: this.associateOverride
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        this.associateResult = await leagueManagerClient.managerAssociateGame(request);
        if (!this.associateResult.success) {
          return;
        }

        (this.$refs.managerAssociateGameFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction(masterGame.gameName + ' successfully associated.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when associating the game.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.searchGameName = '';
      this.associatePublisher = null;
      this.associatePublisherGame = null;
      this.associateMasterGame = null;
      this.associateOverride = false;
      this.associateResult = null;
      this.possibleMasterGames = [];
      this.errorInfo = '';
    },
    publisherSelected(): void {
      this.associatePublisherGame = null;
      this.newGameSelected();
    },
    newGameSelected(): void {
      this.associateResult = null;
      this.associateOverride = false;
    }
  }
});
</script>

<style scoped>
.associate-error {
  margin-top: 10px;
}
.game-search-input {
  margin-bottom: 15px;
}
.override-checkbox {
  margin-left: 10px;
  margin-top: 8px;
}
</style>
