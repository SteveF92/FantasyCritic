<template>
  <b-modal id="reassignLeagueManager" ref="reassignLeagueManagerRef" title="Reassign League Manager" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-warning">
      This option can be used to designate a new league manager for one of the leagues in this conference. This means that the player that is currently league manager will no longer be league manager
      of said league. Leagues can only ever have one manager.
    </div>
    <div class="form-group">
      <label for="reassignLeague" class="control-label">League</label>
      <b-form-select id="reassignLeague" v-model="selectedLeague">
        <option v-for="league in requiredConference.leaguesInConference" :key="league.leagueID" :value="league">
          {{ league.leagueName }}
        </option>
      </b-form-select>
      <label for="reassignLeagueNewManager" class="control-label">New Manager</label>
      <b-form-select id="reassignLeagueNewManager" v-model="newManager">
        <option v-for="user in requiredConference.players" :key="user.userID" :value="user">
          {{ user.displayName }}
        </option>
      </b-form-select>
      <br />
      <br />
      <b-button variant="danger" class="full-width-button" :disabled="!selectedLeague || !newManager || isBusy" @click="promoteNewManager">Transfer League Manager</b-button>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { conferenceClient, type ConferenceLeagueViewModel, type ConferencePlayerViewModel, type PromoteNewLeagueManagerWithinConferenceRequest } from '@/api/clients';
import { useConferenceStore } from '@/store/conferenceStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ReassignLeagueManagerData {
  selectedLeague: ConferenceLeagueViewModel | null;
  newManager: ConferencePlayerViewModel | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): ReassignLeagueManagerData {
    return {
      selectedLeague: null,
      newManager: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useConferenceStore, ['requiredConference'])
  },
  methods: {
    ...mapActions(useConferenceStore, ['notifyAction']),
    async promoteNewManager(): Promise<void> {
      if (!this.selectedLeague || !this.newManager) {
        return;
      }

      const request: PromoteNewLeagueManagerWithinConferenceRequest = {
        conferenceID: this.requiredConference.conferenceID,
        leagueID: this.selectedLeague.leagueID,
        newManagerUserID: this.newManager.userID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.reassignLeagueManager(request);

        (this.$refs.reassignLeagueManagerRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('League manager has been changed.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when changing the league manager.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.selectedLeague = null;
      this.newManager = null;
      this.errorInfo = '';
    }
  }
});
</script>
