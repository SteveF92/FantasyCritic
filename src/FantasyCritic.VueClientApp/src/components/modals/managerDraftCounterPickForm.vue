<template>
  <b-modal id="manager-draft-counter-pick-form" ref="managerDraftCounterPickFormRef" title="Select Counter Pick" no-footer @hidden="clearData" @show="getPossibleCounterPicks">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div v-if="nextPublisherUp">
      <div class="form-group">
        <label for="nextPublisherUp" class="control-label">Select the next counter pick for publisher:</label>
        <label>
          <strong>{{ nextPublisherUp.publisherName }} (Display Name: {{ nextPublisherUp.playerName }})</strong>
        </label>
      </div>
      <form class="form-horizontal" @submit.prevent="selectCounterPick">
        <div class="form-group">
          <label for="selectedCounterPick" class="control-label">Game</label>
          <b-form-select id="selectedCounterPick" v-model="selectedCounterPick" @update:model-value="newGameSelected">
            <option v-for="publisherGame in possibleCounterPicks" :key="publisherGame.publisherGameID" :value="publisherGame">
              {{ publisherGame.gameName }}
            </option>
          </b-form-select>
        </div>

        <div v-if="draftResult && !draftResult.success" class="alert draft-error" :class="{ 'alert-danger': !draftResult.showAsWarning, 'alert-warning': draftResult.showAsWarning }">
          <h3 class="alert-heading">Error!</h3>
          <ul>
            <li v-for="error in draftResult.errors" :key="error">{{ error }}</li>
          </ul>
        </div>

        <div v-if="selectedCounterPick">
          <input type="submit" class="btn btn-primary full-width-button" value="Select Game as Counter Pick" :disabled="isBusy" />
        </div>

        <div v-if="draftResult && draftResult.overridable" class="plain-check">
          <span>
            <label>Do you want to override these warnings?</label>
            <input v-model="draftOverride" class="plain-check-input override-checkbox" type="checkbox" />
          </span>
        </div>
      </form>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueClient, leagueManagerClient, type ManagerClaimResultViewModel, type ManagerDraftGameRequest, type PublisherGameViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { makeToast } from '@/utilities/toast';

interface ManagerDraftCounterPickFormData {
  selectedCounterPick: PublisherGameViewModel | null;
  possibleCounterPicks: PublisherGameViewModel[];
  draftResult: ManagerClaimResultViewModel | null;
  draftOverride: boolean;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['nextPublisherUp'])
  },
  data(): ManagerDraftCounterPickFormData {
    return {
      selectedCounterPick: null,
      possibleCounterPicks: [],
      draftResult: null,
      draftOverride: false,
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    async selectCounterPick(): Promise<void> {
      const counterPick = this.selectedCounterPick;
      const publisher = this.nextPublisherUp;
      if (!counterPick || !publisher) {
        return;
      }

      const request: ManagerDraftGameRequest = {
        publisherID: publisher.publisherID,
        gameName: counterPick.gameName,
        counterPick: true,
        masterGameID: counterPick.masterGame?.masterGameID,
        managerOverride: this.draftOverride,
        allowIneligibleSlot: false
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        this.draftResult = await leagueManagerClient.managerDraftGame(request);
        if (!this.draftResult.success) {
          return;
        }

        (this.$refs.managerDraftCounterPickFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        makeToast(counterPick.gameName + ' selected as a counter pick by ' + publisher.publisherName);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when selecting the counter pick.');
      } finally {
        this.isBusy = false;
      }
    },
    async getPossibleCounterPicks(): Promise<void> {
      if (!this.nextPublisherUp) {
        return;
      }

      this.isBusy = true;
      this.errorInfo = '';
      try {
        this.possibleCounterPicks = await leagueClient.possibleCounterPicks(this.nextPublisherUp.publisherID);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to load counter picks.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.isBusy = false;
      this.selectedCounterPick = null;
      this.possibleCounterPicks = [];
      this.draftResult = null;
      this.draftOverride = false;
      this.errorInfo = '';
    },
    newGameSelected(): void {
      this.draftResult = null;
      this.draftOverride = false;
    }
  }
});
</script>

<style scoped>
.draft-error {
  margin-top: 10px;
}
.override-checkbox {
  margin-left: 10px;
  margin-top: 8px;
}
</style>
