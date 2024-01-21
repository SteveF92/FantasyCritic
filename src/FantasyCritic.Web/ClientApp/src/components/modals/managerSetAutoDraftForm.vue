<template>
  <b-modal id="managerSetAutoDraftForm" ref="managerSetAutoDraftFormRef" title="Edit Auto Draft">
    <div class="alert alert-info">
      <p class="text-white">You can use this form to turn on or turn off autodraft for one of your players.</p>
      <p class="text-white">
        If you turn on the 'Standard Games Only' mode, then the site will not autodraft counter picks for that player. They will need to make their selections themselves, or you can draft for them as
        league manager.
      </p>
    </div>

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
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}
</style>
