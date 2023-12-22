<template>
  <b-modal id="editConferenceSettings" ref="editConferenceSettingsRef" title="Change League Options" @hidden="clearData">
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

      <b-alert variant="danger" :show="!!errorInfo">{{ errorInfo }}</b-alert>
    </div>
    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Change Settings" :disabled="!newConferenceName" @click="changeConferenceSettings" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';
import ConferenceMixin from '@/mixins/conferenceMixin.js';

export default {
  mixins: [ConferenceMixin],
  data() {
    return {
      newConferenceName: '',
      customRulesConference: false,
      errorInfo: null
    };
  },
  created() {
    this.newConferenceName = this.conference.conferenceName;
    this.customRulesConference = this.conference.customRulesConference;
  },
  methods: {
    async changeConferenceSettings() {
      const model = {
        conferenceID: this.conference.conferenceID,
        conferenceName: this.newConferenceName,
        customRulesConference: this.customRulesConference
      };

      try {
        await axios.post('/api/conference/EditConference', model);
        this.$refs.editConferenceSettingsRef.hide();
        await this.notifyAction('Conference settings have been updated.');
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.newConferenceName = this.conference.conferenceName;
      this.customRulesConference = this.conference.customRulesConference;
    }
  }
};
</script>
