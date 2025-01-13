<template>
  <b-modal id="changeLeagueOptionsForm" ref="changeLeagueOptionsFormRef" title="Change League Options" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newleagueName" class="control-label">League Name</label>
        <input id="newleagueName" v-model="newleagueName" name="newleagueName" type="text" class="form-control input" />
      </div>
      <div class="form-group">
        <b-form-checkbox v-model="publicLeague">
          <span class="checkbox-label">Public League</span>
          <p>If checked, anyone with a link to your league will be able to view it. If unchecked, your league will only be viewable by its members.</p>
        </b-form-checkbox>
      </div>
      <div class="form-group">
        <b-form-checkbox v-model="customRulesLeague">
          <span class="checkbox-label">Custom Rules League</span>
          <p>
            If checked, this league won't affect the site's overall stats. Please use this if you are running a "highly custom" league, such as a league where you are picking the
            <em>worst</em>
            games of the year. If you want to change a custom rules league into a regular league, you'll need to contact us.
          </p>
        </b-form-checkbox>
      </div>
      <div class="form-group">
        <b-form-checkbox v-model="testLeague">
          <span class="checkbox-label">Test League</span>
          <p>
            If checked, this league won't affect the site's overall stats. Please check this if you are just testing out the site. If you want to change a test league into a regular league, you'll
            need to contact us.
          </p>
        </b-form-checkbox>
      </div>
      <b-alert variant="info">
        The difference between a "Custom Rules League" and a "Test League" is that a "Custom Rules League" will still be listed alongside your normal leagues and you will see upcoming games for that
        league on your home page. There is a dedicated home page tab for test leagues, and the games are not listed in your upcoming games on the home page. The "Custom Rules League" option is new as
        of 2023, and you
        <em>are</em>
        allowed to change your "Test League" into a "Custom Rules League", since neither affect the site's stats.
      </b-alert>
    </div>
    <b-alert :show="showDiscordWarning" variant="warning">
      Changing a public league to a private league will remove it from any Discord servers it may be linked to via the official Discord bot.
    </b-alert>

    <b-alert variant="danger" :show="!!errorInfo">{{ errorInfo }}</b-alert>

    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Change Settings" :disabled="!newleagueName" @click="changeleagueName" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      newleagueName: '',
      publicLeague: true,
      testLeague: false,
      customRulesLeague: false,
      errorInfo: null
    };
  },
  computed: {
    showDiscordWarning() {
      return this.league.publicLeague && !this.publicLeague;
    }
  },
  created() {
    this.newleagueName = this.league.leagueName;
    this.publicLeague = this.league.publicLeague;
    this.testLeague = this.league.testLeague;
    this.customRulesLeague = this.league.customRulesLeague;
  },
  methods: {
    async changeleagueName() {
      const model = {
        leagueID: this.league.leagueID,
        leagueName: this.newleagueName.trim(),
        publicLeague: this.publicLeague,
        testLeague: this.testLeague,
        customRulesLeague: this.customRulesLeague
      };

      try {
        await axios.post('/api/leagueManager/ChangeLeagueOptions', model);
        this.$refs.changeLeagueOptionsFormRef.hide();
        this.notifyAction('League options have been updated.');
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.newleagueName = this.league.leagueName;
    }
  }
};
</script>
