<template>
  <b-modal id="changeLeagueOptionsForm" ref="changeLeagueOptionsFormRef" title="Change League Options" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newleagueName" class="control-label">League Name</label>
        <input v-model="newleagueName" id="newleagueName" name="newleagueName" type="text" class="form-control input" />
      </div>
      <div class="form-group">
        <b-form-checkbox v-model="publicLeague">
          <span class="checkbox-label">Public League</span>
          <p>If checked, anyone with a link to your league will be able to view it. If unchecked, your league will only be viewable by its members.</p>
        </b-form-checkbox>
      </div>
      <div class="form-group">
        <b-form-checkbox v-model="testLeague" :disabled="initialTestLeague">
          <span class="checkbox-label">Test League</span>
          <p>If checked, this league won't affect the site's overall stats. Please check this if you are just testing out the site.
          If you want to change a test league into a regular league, you'll need to contact me.</p>
        </b-form-checkbox>
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Change Settings" v-on:click="changeleagueName" :disabled="!newleagueName"/>
    </div>
  </b-modal>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';

export default {
  data() {
    return {
      newleagueName: '',
      publicLeague: true,
      testLeague: false,
      errorInfo: '',
      initialTestLeague: false
    };
  },
  props: ['league'],
  methods: {
    changeleagueName() {
      var model = {
        leagueID: this.league.leagueID,
        leagueName: this.newleagueName,
        publicLeague: this.publicLeague,
        testLeague: this.testLeague
      };
      axios
        .post('/api/leagueManager/ChangeLeagueOptions', model)
        .then(response => {
          this.$refs.changeLeagueOptionsFormRef.hide();
          this.$emit('leagueOptionsChanged');
          this.newleagueName = '';
        })
        .catch(response => {
        });
    },
    clearData() {
      this.newleagueName = this.league.leagueName;
    }
  },
  mounted() {
    this.newleagueName = this.league.leagueName;
    this.publicLeague = this.league.publicLeague;
    this.testLeague = this.league.testLeague;
    this.initialTestLeague = this.league.testLeague;
  }
};
</script>
