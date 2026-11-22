<template>
  <b-modal id="addNewConferenceYear" ref="addNewConferenceYearRef" title="Add New Conference Year" size="lg" @show="fetchAvailableYears">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">
      Starting a new year will not impact your current year. You will always be able to access all years of your conference, and starting a new year does not end your current year.
      <br />
      <br />
      Starting the new year will renew your Primary League, and you will be able to choose your settings which will then be applied to any other leagues you renew.
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
      <input type="submit" class="btn btn-primary" value="Add New Conference Year" :disabled="!selectedYear || isBusy" @click="addNewYear" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { conferenceClient, type NewConferenceYearRequest } from '@/api/clients';
import { useConferenceStore } from '@/store/conferenceStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface AddNewConferenceYearData {
  availableYears: number[];
  selectedYear: number | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useConferenceStore, ['requiredConference'])
  },
  data(): AddNewConferenceYearData {
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
        this.availableYears = await conferenceClient.availableYears(this.requiredConference.conferenceID);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to load the years you can start.');
      }
    },
    async addNewYear(): Promise<void> {
      if (!this.selectedYear) {
        return;
      }

      const request: NewConferenceYearRequest = {
        conferenceID: this.requiredConference.conferenceID,
        year: this.selectedYear
      };
      const primaryLeagueID = this.requiredConference.primaryLeague.leagueID;

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.addNewConferenceYear(request);

        (this.$refs.addNewConferenceYearRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.$router.push({ name: 'editLeague', params: { leagueid: primaryLeagueID, year: request.year }, query: { freshSettings: 'true' } });
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when starting the new year.');
      } finally {
        this.isBusy = false;
      }
    }
  }
});
</script>
