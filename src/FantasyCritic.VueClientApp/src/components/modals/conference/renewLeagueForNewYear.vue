<template>
  <b-modal id="renewLeagueForNewYear" ref="renewLeagueForNewYearRef" title="Renew League for New Year" size="lg" @show="initialize" @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">
      Renewing a league for {{ requiredConference.activeYear }} will use the settings you have chosen for the Primary League.
      <br />
      <br />
      By default, the league will keep the same players as were in it last year. You can adjust this before you start the draft.
    </div>

    <div class="form-group">
      <label for="renewLeague" class="control-label">League</label>
      <b-form-select id="renewLeague" v-model="selectedLeague">
        <option v-for="league in renewableLeagues" :key="league.leagueID" :value="league">
          {{ league.leagueName }}
        </option>
      </b-form-select>
    </div>

    <template #footer>
      <input type="submit" class="btn btn-primary" value="Renew League" :disabled="!selectedLeague || isBusy" @click="renewLeague" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { conferenceClient, type AddNewLeagueYearRequest, type ConferenceLeagueViewModel } from '@/api/clients';
import { useConferenceStore } from '@/store/conferenceStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface RenewLeagueForNewYearData {
  renewableLeagues: ConferenceLeagueViewModel[];
  selectedLeague: ConferenceLeagueViewModel | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useConferenceStore, ['requiredConference', 'requiredConferenceYear'])
  },
  data(): RenewLeagueForNewYearData {
    return {
      renewableLeagues: [],
      selectedLeague: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useConferenceStore, ['notifyAction']),
    async initialize(): Promise<void> {
      this.errorInfo = '';
      try {
        // The conference year only has the leagues already playing this year, so the full list comes from the conference.
        const conference = await conferenceClient.getConference(this.requiredConference.conferenceID);
        const leaguesAlreadyRenewed = this.requiredConferenceYear.leagueYears.map((x) => x.leagueID);
        this.renewableLeagues = conference.leaguesInConference.filter((x) => !leaguesAlreadyRenewed.includes(x.leagueID));
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to load the leagues in this conference.');
      }
    },
    async renewLeague(): Promise<void> {
      if (!this.selectedLeague) {
        return;
      }

      const request: AddNewLeagueYearRequest = {
        conferenceID: this.requiredConference.conferenceID,
        year: this.requiredConferenceYear.year,
        leagueID: this.selectedLeague.leagueID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.addNewLeagueYear(request);

        (this.$refs.renewLeagueForNewYearRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction(`League has been renewed for ${request.year}.`);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when renewing the league.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.renewableLeagues = [];
      this.selectedLeague = null;
      this.errorInfo = '';
    }
  }
});
</script>
