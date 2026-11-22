<template>
  <b-modal id="addLeagueToConference" ref="addLeagueToConferenceRef" title="Add League to Conference" size="lg" @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">
      All leagues added to your conference will start with the same options as the primary league in the conference. You only need to chose the league name, and the league manager. Both can be changed
      later.
      <br />
      <br />
      The league manager can be any user in the conference, including you (the conference manager).
      <br />
      <br />
      Note! You should make sure you are happy with the settings in the primary league (how many games per player, what games are allowed, etc.) before creating additional leagues. While you can
      change league settings after they are created, it's easier to get it all right from the start.
    </div>

    <div class="form-horizontal">
      <div class="form-group">
        <label for="newLeagueName" class="control-label">League Name</label>
        <input id="newLeagueName" v-model="newLeagueName" name="newLeagueName" type="text" class="form-control input" />
      </div>

      <div class="form-group">
        <label for="newLeagueManager" class="control-label">League Manager</label>
        <b-form-select id="newLeagueManager" v-model="newLeagueManager">
          <option v-for="player in requiredConference.players" :key="player.userID" :value="player">
            {{ player.displayName }}
          </option>
        </b-form-select>
      </div>
    </div>

    <template #footer>
      <input type="submit" class="btn btn-primary" value="Add New League" :disabled="!newLeagueName.trim() || !newLeagueManager || isBusy" @click="addNewLeague" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { conferenceClient, type AddLeagueToConferenceRequest, type ConferencePlayerViewModel } from '@/api/clients';
import { useConferenceStore } from '@/store/conferenceStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface AddLeagueToConferenceData {
  newLeagueName: string;
  newLeagueManager: ConferencePlayerViewModel | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useConferenceStore, ['requiredConference', 'requiredConferenceYear'])
  },
  data(): AddLeagueToConferenceData {
    return {
      newLeagueName: '',
      newLeagueManager: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useConferenceStore, ['notifyAction']),
    async addNewLeague(): Promise<void> {
      if (!this.newLeagueManager) {
        return;
      }

      const request: AddLeagueToConferenceRequest = {
        conferenceID: this.requiredConference.conferenceID,
        year: this.requiredConferenceYear.year,
        leagueName: this.newLeagueName,
        leagueManager: this.newLeagueManager.userID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.addLeagueToConference(request);

        (this.$refs.addLeagueToConferenceRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('A new league has been added.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when adding the league.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.newLeagueName = '';
      this.newLeagueManager = null;
      this.errorInfo = '';
    }
  }
});
</script>
