<template>
  <b-modal id="managerSetAutoDraftForm" ref="managerSetAutoDraftFormRef" title="Edit Auto Draft">
    <div class="alert alert-info">You can use this form to turn on or turn off autodraft for one of your players.</div>

    <b-form-group class="form-checkbox-group">
      <div v-for="publisher in publishers" :key="publisher.publisherID" class="publisher-autodraft">
        <label>{{ publisher.publisherName }}</label>
        <b-form-select v-model="publisher.autoDraftMode" :options="autoDraftOptions"></b-form-select>
      </div>
    </b-form-group>

    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Set Auto Draft" @click="setAutoDraft" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  name: 'ManagerSetAutoDraftForm',
  mixins: [LeagueMixin],
  data() {
    return {
      autoDraftOptions: [
        { text: 'Off', value: 'Off' },
        { text: "Standard Games Only (Don't Auto Draft Counter Picks)", value: 'StandardGamesOnly' },
        { text: 'On (Including Counter Picks)', value: 'All' }
      ]
    };
  },
  methods: {
    setAutoDraft() {
      let autoDraftSettings = {};
      for (let i = 0; i < this.publishers.length; i++) {
        autoDraftSettings[this.publishers[i].publisherID] = this.publishers[i].autoDraftMode;
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
<style scoped>
.publisher-autodraft {
  display: flex;
  align-items: end;
  gap: 10px;
  margin-bottom: 10px;
}
</style>
