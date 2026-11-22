<template>
  <b-modal id="change-league-options-form" ref="changeLeagueOptionsFormRef" title="Change League Options" @show="initialize">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newLeagueName" class="control-label">League Name</label>
        <input id="newLeagueName" v-model="newLeagueName" name="newLeagueName" type="text" class="form-control input" />
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
      <b-alert variant="info" :model-value="true">
        The difference between a "Custom Rules League" and a "Test League" is that a "Custom Rules League" will still be listed alongside your normal leagues and you will see upcoming games for that
        league on your home page. There is a dedicated home page tab for test leagues, and the games are not listed in your upcoming games on the home page. You
        <em>are</em>
        allowed to change your "Test League" into a "Custom Rules League", since neither affect the site's stats.
      </b-alert>
    </div>
    <b-alert :model-value="showDiscordWarning" variant="warning">
      Changing a public league to a private league will remove it from any Discord servers it may be linked to via the official Discord bot.
    </b-alert>

    <template #footer>
      <input type="submit" class="btn btn-primary" value="Change Settings" :disabled="!newLeagueName.trim() || isBusy" @click="changeLeagueOptions" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type ChangeLeagueOptionsRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ChangeLeagueOptionsFormData {
  newLeagueName: string;
  publicLeague: boolean;
  testLeague: boolean;
  customRulesLeague: boolean;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): ChangeLeagueOptionsFormData {
    return {
      newLeagueName: '',
      publicLeague: true,
      testLeague: false,
      customRulesLeague: false,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeague']),
    showDiscordWarning(): boolean {
      return this.requiredLeague.publicLeague && !this.publicLeague;
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    initialize(): void {
      this.newLeagueName = this.requiredLeague.leagueName;
      this.publicLeague = this.requiredLeague.publicLeague;
      this.testLeague = this.requiredLeague.testLeague;
      this.customRulesLeague = this.requiredLeague.customRulesLeague;
      this.errorInfo = '';
    },
    async changeLeagueOptions(): Promise<void> {
      const request: ChangeLeagueOptionsRequest = {
        leagueID: this.requiredLeague.leagueID,
        leagueName: this.newLeagueName.trim(),
        publicLeague: this.publicLeague,
        testLeague: this.testLeague,
        customRulesLeague: this.customRulesLeague
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.changeLeagueOptions(request);

        (this.$refs.changeLeagueOptionsFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('League options have been updated.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when changing the league options.');
      } finally {
        this.isBusy = false;
      }
    }
  }
});
</script>
