<template>
  <b-modal id="manager-set-auto-draft-form" ref="managerSetAutoDraftFormRef" size="lg" title="Edit Auto Draft" @show="onShow">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">
      <p class="text-white">You can use this form to turn on or turn off autodraft for one of your players.</p>
      <p class="text-white">
        If you turn on the 'Standard Games Only' mode, then the site will not autodraft counter picks for that player. They will need to make their selections themselves, or you can draft for them as
        league manager.
      </p>
      <p class="text-white">
        If you check 'Only from Watchlist', auto draft will only consider games on that player's watchlist. If no watchlist games are available, the auto draft will stop and the player will need to
        make the pick manually.
      </p>
    </div>

    <b-form-group class="form-checkbox-group">
      <div v-for="settings in localPublisherSettings" :key="settings.publisherID" class="publisher-autodraft">
        <label class="publisher-name">{{ settings.publisherName }}</label>
        <b-form-select v-model="settings.autoDraftMode" :options="autoDraftOptions" class="mode-select" @update:model-value="onModeChange()"></b-form-select>
        <b-form-checkbox v-model="settings.onlyDraftFromWatchlist" :disabled="settings.autoDraftMode === 'Off'" class="watchlist-checkbox">Only from Watchlist</b-form-checkbox>
      </div>
    </b-form-group>

    <template #footer>
      <input type="submit" class="btn btn-primary" value="Set Auto Draft" :disabled="isBusy" @click="setAutoDraft" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type ManagerSetAutoDraftRequest, type PublisherAutoDraftSettingsRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface PublisherAutoDraftSettings {
  publisherID: string;
  publisherName: string;
  autoDraftMode: string;
  onlyDraftFromWatchlist: boolean;
}

interface ManagerSetAutoDraftFormData {
  localPublisherSettings: PublisherAutoDraftSettings[];
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  setup() {
    const autoDraftOptions: { value: string; text: string }[] = [
      { text: 'Off', value: 'Off' },
      { text: "Standard Games Only (Don't Auto Draft Counter Picks)", value: 'StandardGamesOnly' },
      { text: 'On (Including Counter Picks)', value: 'All' }
    ];

    return { autoDraftOptions };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  },
  data(): ManagerSetAutoDraftFormData {
    return {
      localPublisherSettings: [],
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    onShow(): void {
      this.localPublisherSettings = this.requiredLeagueYear.publishers.map((p) => ({
        publisherID: p.publisherID,
        publisherName: p.publisherName,
        autoDraftMode: p.autoDraftMode,
        onlyDraftFromWatchlist: p.onlyAutoDraftFromWatchlist
      }));
      this.errorInfo = '';
    },
    onModeChange(): void {
      for (const settings of this.localPublisherSettings) {
        if (settings.autoDraftMode === 'Off') {
          settings.onlyDraftFromWatchlist = false;
        }
      }
    },
    async setAutoDraft(): Promise<void> {
      const publisherAutoDraft: Record<string, PublisherAutoDraftSettingsRequest> = {};
      for (const settings of this.localPublisherSettings) {
        publisherAutoDraft[settings.publisherID] = {
          mode: settings.autoDraftMode,
          onlyDraftFromWatchlist: settings.onlyDraftFromWatchlist
        };
      }

      const request: ManagerSetAutoDraftRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        publisherAutoDraft
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.setAutoDraft(request);

        (this.$refs.managerSetAutoDraftFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Auto draft changed.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when setting auto draft.');
      } finally {
        this.isBusy = false;
      }
    }
  }
});
</script>

<style scoped>
.publisher-autodraft {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}
.publisher-name {
  min-width: 120px;
}
.mode-select {
  flex: 1;
}
.watchlist-checkbox {
  white-space: nowrap;
}
</style>
