<template>
  <b-modal id="managerSetAutoDraftForm" ref="managerSetAutoDraftFormRef" title="Edit Auto Draft">
    <div class="alert alert-info">You can use this form to turn on or turn off autodraft for one of your players.</div>

    <b-form-group class="form-checkbox-group stacked checkboxes">
      <b-form-checkbox v-for="publisher in publishers" v-bind:data="publisher" v-bind:key="publisher.publisherID" v-model="publisher.autoDraft">
        {{ publisher.publisherName }}
      </b-form-checkbox>
    </b-form-group>

    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Set Auto Draft" v-on:click="setAutoDraft" />
    </div>
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  name: 'managerSetAutoDraftForm',
  mixins: [LeagueMixin],
  methods: {
    setAutoDraft() {
      let autoDraftSettings = {};
      for (let i = 0; i < this.publishers.length; i++) {
        autoDraftSettings[this.publishers[i].publisherID] = this.publishers[i].autoDraft;
      }

      const model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        publisherAutoDraft: autoDraftSettings
      };
      axios
        .post('/api/leagueManager/SetAutoDraft', model)
        .then(() => {
          this.$refs.managerSetAutoDraftFormRef.hide();
          this.notifyAction('Auto draft changed.');
        })
        .catch(() => {});
    }
  }
};
</script>
