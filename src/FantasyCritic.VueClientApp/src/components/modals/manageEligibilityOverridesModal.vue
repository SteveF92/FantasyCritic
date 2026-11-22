<template>
  <b-modal id="manage-eligibility-overrides-modal" size="lg" title="Manage Eligibility Overrides" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">This option will allow you to manually allow or ban a specific game in your league, no matter what your other settings are.</div>
    <div v-if="requiredLeagueYear.settings.hasSpecialSlots" class="alert alert-warning">
      Warning! Because your league uses 'special roster slots', you should consider using the 'tag override' option, instead of this option. If you set a game as 'eligible' here, it will be eligible
      in
      <em>any</em>
      slot in your league.
    </div>
    <div v-if="requiredLeagueYear.eligibilityOverrides.length > 0">
      <table class="table table-bordered table-striped">
        <thead>
          <tr class="bg-primary">
            <th scope="col" class="game-column">Game</th>
            <th scope="col">Eligible?</th>
            <th scope="col">Reset?</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="eligibilityOverride in requiredLeagueYear.eligibilityOverrides" :key="eligibilityOverride.masterGame.masterGameID">
            <td>{{ eligibilityOverride.masterGame.gameName }}</td>
            <td>{{ formatYesNo(eligibilityOverride.eligible) }}</td>
            <td class="select-cell">
              <b-button variant="danger" :disabled="isBusy" @click="resetEligibility(eligibilityOverride)">Reset</b-button>
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
    <div v-if="overrideMasterGame" class="eligibility-set-buttons">
      <b-button variant="danger" size="sm" :disabled="isBusy" @click="setEligibility(overrideMasterGame, false)">Ban Game</b-button>
      <b-button variant="success" size="sm" :disabled="isBusy" @click="setEligibility(overrideMasterGame, true)">Allow Game</b-button>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';

import {
  leagueClient,
  leagueManagerClient,
  type EligibilityOverrideRequest,
  type EligibilityOverrideViewModel,
  type MasterGameYearViewModel,
  type PossibleMasterGameYearViewModel
} from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatYesNo } from '@/utilities/formatters';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';

interface ManageEligibilityOverridesModalData {
  searchGameName: string;
  overrideMasterGame: MasterGameYearViewModel | null;
  possibleMasterGames: PossibleMasterGameYearViewModel[];
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  components: {
    PossibleMasterGamesTable
  },
  data(): ManageEligibilityOverridesModalData {
    return {
      searchGameName: '',
      overrideMasterGame: null,
      possibleMasterGames: [],
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    formatYesNo,
    async resetEligibility(eligibilityOverride: EligibilityOverrideViewModel): Promise<void> {
      const request: EligibilityOverrideRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        masterGameID: eligibilityOverride.masterGame.masterGameID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.setGameEligibilityOverride(request);
        await this.notifyAction(eligibilityOverride.masterGame.gameName + "'s eligibility was reset to normal.");
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when resetting the eligibility.');
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
    async setEligibility(masterGame: MasterGameYearViewModel, eligible: boolean): Promise<void> {
      const request: EligibilityOverrideRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        masterGameID: masterGame.masterGameID,
        eligible: eligible
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.setGameEligibilityOverride(request);

        const message = eligible ? masterGame.gameName + ' was marked as eligible.' : masterGame.gameName + ' was marked as ineligible.';
        await this.notifyAction(message);
        this.clearData();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when setting the eligibility.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.searchGameName = '';
      this.overrideMasterGame = null;
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
.select-cell {
  text-align: center;
}
.game-search-input {
  margin-bottom: 15px;
}

.eligibility-set-buttons {
  display: flex;
  justify-content: space-around;
}
</style>
