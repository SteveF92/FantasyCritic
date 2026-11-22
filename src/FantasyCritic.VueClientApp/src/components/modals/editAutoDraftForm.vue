<template>
  <b-modal id="edit-auto-draft-form" ref="editAutoDraftFormRef" size="lg" title="Edit Auto Draft" @show="initialize">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">
      <p class="text-white">If "Auto Draft" is turned on, the site will select your games for you when it is your turn.</p>
      <p class="text-white">Games will be chosen from your watchlist first, and if there are no available games on your watchlist, the available game with the highest hype factor will be chosen.</p>
      <p class="text-white">For counter picks, the game with the highest counter pick % site-wide will be chosen.</p>
      <p class="text-white">
        If you turn on the 'Standard Games Only' mode, then the site will not autodraft counter picks for you. You will need to make those selections yourself, as if auto draft was off.
      </p>
      <p class="text-white">
        If you check 'Only Auto Draft from Watchlist', the site will only consider games on your watchlist when auto drafting. If no watchlist games are available, the auto draft will stop and you
        will need to make the pick manually.
      </p>
    </div>
    <b-form-select v-model="autoDraftMode" :options="autoDraftOptions" class="mb-3" @update:model-value="onModeChange"></b-form-select>
    <b-form-checkbox v-model="onlyDraftFromWatchlist" :disabled="autoDraftMode === 'Off'">Only Auto Draft from Watchlist</b-form-checkbox>

    <template #footer>
      <input type="submit" class="btn btn-primary" value="Set Auto Draft" :disabled="isBusy" @click="setAutoDraft" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueClient, type SetAutoDraftRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface EditAutoDraftFormData {
  autoDraftMode: string;
  onlyDraftFromWatchlist: boolean;
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
    ...mapState(useLeagueStore, ['requiredUserPublisher'])
  },
  data(): EditAutoDraftFormData {
    return {
      autoDraftMode: '',
      onlyDraftFromWatchlist: false,
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    initialize(): void {
      this.autoDraftMode = this.requiredUserPublisher.autoDraftMode;
      this.onlyDraftFromWatchlist = this.requiredUserPublisher.onlyAutoDraftFromWatchlist;
      this.errorInfo = '';
    },
    onModeChange(newMode: unknown): void {
      if (newMode === 'Off') {
        this.onlyDraftFromWatchlist = false;
      }
    },
    async setAutoDraft(): Promise<void> {
      const request: SetAutoDraftRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        mode: this.autoDraftMode,
        onlyDraftFromWatchlist: this.onlyDraftFromWatchlist
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueClient.setAutoDraft(request);

        (this.$refs.editAutoDraftFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Auto draft set to ' + request.mode);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when setting auto draft.');
      } finally {
        this.isBusy = false;
      }
    }
  }
});
</script>
