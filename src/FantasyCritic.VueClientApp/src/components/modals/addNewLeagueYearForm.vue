<template>
  <b-modal id="add-new-league-year-form" ref="addNewLeagueYearFormRef" title="Start New Year" @show="fetchAvailableYears">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">
      Starting a new year will not impact your current year. You will always be able to access all years of your league, and starting a new year does not end your current year.
    </div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="selectedYear" class="control-label">New Year to Play</label>
        <b-form-select id="selectedYear" v-model="selectedYear" class="form-control">
          <template #first>
            <option :value="null">Select a Year</option>
          </template>
          <option v-for="possibleYear in availableYears" :key="possibleYear" :value="possibleYear">{{ possibleYear }}</option>
        </b-form-select>
      </div>
    </div>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Start New Year" :disabled="!selectedYear || isBusy" @click="addNewLeagueYear" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type NewLeagueYearRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface AddNewLeagueYearFormData {
  availableYears: number[];
  selectedYear: number | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['requiredLeague'])
  },
  data(): AddNewLeagueYearFormData {
    return {
      availableYears: [],
      selectedYear: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    async fetchAvailableYears(): Promise<void> {
      this.selectedYear = null;
      this.errorInfo = '';
      try {
        this.availableYears = await leagueManagerClient.availableYears(this.requiredLeague.leagueID);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to load the years you can start.');
      }
    },
    async addNewLeagueYear(): Promise<void> {
      if (!this.selectedYear) {
        return;
      }

      const request: NewLeagueYearRequest = {
        leagueID: this.requiredLeague.leagueID,
        year: this.selectedYear
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.addNewLeagueYear(request);

        (this.$refs.addNewLeagueYearFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.$router.push({ name: 'editLeague', params: { leagueid: request.leagueID, year: request.year }, query: { freshSettings: 'true' } });
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when starting the new year.');
      } finally {
        this.isBusy = false;
      }
    }
  }
});
</script>
