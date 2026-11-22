<template>
  <b-modal id="manage-tag-overrides-modal" size="lg" title="Manage Tag Overrides" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">This option will allow you to override the tags of a game to whatever you want, if you disagree with how the site has classified something.</div>
    <div v-if="requiredLeagueYear.tagOverrides.length > 0">
      <table class="table table-bordered table-striped">
        <thead>
          <tr class="bg-primary">
            <th scope="col" class="game-column">Game</th>
            <th scope="col">Tags</th>
            <th scope="col">Reset?</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="tagOverride in requiredLeagueYear.tagOverrides" :key="tagOverride.masterGame.masterGameID">
            <td>{{ tagOverride.masterGame.gameName }}</td>
            <td>
              <span v-for="tag in tagOverride.tags" :key="tag">
                <masterGameTagBadge :tag-name="tag"></masterGameTagBadge>
              </span>
            </td>
            <td class="select-cell">
              <b-button variant="danger" :disabled="isBusy" @click="resetTags(tagOverride)">Reset</b-button>
            </td>
          </tr>
        </tbody>
      </table>
      <hr />
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
          v-if="!overrideMasterGame && possibleMasterGames.length > 0"
          v-model="overrideMasterGame"
          :possible-games="possibleMasterGames"
          @update:model-value="newGameSelected"></possibleMasterGamesTable>
      </div>
    </form>
    <label v-if="overrideMasterGame" class="control-label">Selected Game: {{ overrideMasterGame.gameName }}</label>
    <div v-if="overrideMasterGame">
      <masterGameTagSelector v-model="chosenTags"></masterGameTagSelector>
      <b-button variant="info" class="set-tags-button" size="sm" :disabled="isBusy" @click="setTags(overrideMasterGame)">Set Tags</b-button>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';

import {
  leagueClient,
  leagueManagerClient,
  type MasterGameTagViewModel,
  type MasterGameYearViewModel,
  type PossibleMasterGameYearViewModel,
  type TagOverrideRequest,
  type TagOverrideViewModel
} from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';
import MasterGameTagSelector from '@/components/masterGameTagSelector.vue';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';

interface ManageTagOverridesModalData {
  searchGameName: string;
  overrideMasterGame: MasterGameYearViewModel | null;
  possibleMasterGames: PossibleMasterGameYearViewModel[];
  chosenTags: MasterGameTagViewModel[];
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  components: {
    PossibleMasterGamesTable,
    MasterGameTagBadge,
    MasterGameTagSelector
  },
  data(): ManageTagOverridesModalData {
    return {
      searchGameName: '',
      overrideMasterGame: null,
      possibleMasterGames: [],
      chosenTags: [],
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async resetTags(tagOverride: TagOverrideViewModel): Promise<void> {
      const request: TagOverrideRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        masterGameID: tagOverride.masterGame.masterGameID,
        tags: []
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.setGameTagOverride(request);
        await this.notifyAction(tagOverride.masterGame.gameName + ' had its tags reset.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when resetting the tags.');
      } finally {
        this.isBusy = false;
      }
    },
    async searchGame(): Promise<void> {
      this.overrideMasterGame = null;
      this.possibleMasterGames = [];

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
    async setTags(masterGame: MasterGameYearViewModel): Promise<void> {
      const request: TagOverrideRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        masterGameID: masterGame.masterGameID,
        tags: this.chosenTags.map((x) => x.name)
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.setGameTagOverride(request);
        await this.notifyAction(masterGame.gameName + ' had its tags overridden.');
        this.clearData();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when setting the tags.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.searchGameName = '';
      this.overrideMasterGame = null;
      this.possibleMasterGames = [];
      this.chosenTags = [];
      this.errorInfo = '';
    },
    newGameSelected(): void {
      this.errorInfo = '';
    }
  }
});
</script>

<style scoped>
.select-cell {
  text-align: center;
}
.game-search-input {
  margin-bottom: 15px;
}
.set-tags-button {
  margin-top: 10px;
  width: 100%;
}
</style>
