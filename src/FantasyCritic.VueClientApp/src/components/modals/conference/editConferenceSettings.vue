<template>
  <b-modal id="editConferenceSettings" ref="editConferenceSettingsRef" title="Change Conference Options" @show="setData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">If you want to edit the actual game settings, like how many games per player, or what games are allowed, you'll need to do that inside each of the leagues.</div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newConferenceName" class="control-label">Conference Name</label>
        <input id="newConferenceName" v-model="newConferenceName" name="newConferenceName" type="text" class="form-control input" />
      </div>
      <div class="form-group">
        <b-form-checkbox v-model="customRulesConference">
          <span class="checkbox-label">Custom Rules Conference</span>
          <p>
            If checked, the leagues in this conference won't affect the site's overall stats. Please use this if you are running a "highly custom" custom, such as a league where you are picking the
            <em>worst</em>
            games of the year. If you want to change a custom rules conference into a regular conference, you'll need to contact us.
          </p>
        </b-form-checkbox>
      </div>
    </div>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Change Settings" :disabled="!newConferenceName.trim() || isBusy" @click="changeConferenceSettings" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { conferenceClient, type EditConferenceRequest } from '@/api/clients';
import { useConferenceStore } from '@/store/conferenceStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface EditConferenceSettingsData {
  newConferenceName: string;
  customRulesConference: boolean;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useConferenceStore, ['requiredConference'])
  },
  data(): EditConferenceSettingsData {
    return {
      newConferenceName: '',
      customRulesConference: false,
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useConferenceStore, ['notifyAction']),
    setData(): void {
      this.newConferenceName = this.requiredConference.conferenceName;
      this.customRulesConference = this.requiredConference.customRulesConference;
      this.errorInfo = '';
    },
    async changeConferenceSettings(): Promise<void> {
      const request: EditConferenceRequest = {
        conferenceID: this.requiredConference.conferenceID,
        conferenceName: this.newConferenceName,
        customRulesConference: this.customRulesConference
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.editConference(request);

        (this.$refs.editConferenceSettingsRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Conference settings have been updated.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when changing the conference settings.');
      } finally {
        this.isBusy = false;
      }
    }
  }
});
</script>
