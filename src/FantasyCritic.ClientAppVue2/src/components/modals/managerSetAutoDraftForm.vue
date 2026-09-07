<template>
  <b-modal id="managerSetAutoDraftForm" ref="managerSetAutoDraftFormRef" size="lg" title="Edit Auto Draft" @show="onShow">
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
        <b-form-select v-model="settings.autoDraftMode" :options="autoDraftOptions" class="mode-select" @change="onModeChange(settings)"></b-form-select>
        <b-form-checkbox v-model="settings.onlyDraftFromWatchlist" :disabled="settings.autoDraftMode === 'Off'" class="watchlist-checkbox">Only from Watchlist</b-form-checkbox>
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
      localPublisherSettings: [],
      autoDraftOptions: [
        { text: 'Off', value: 'Off' },
        { text: "Standard Games Only (Don't Auto Draft Counter Picks)", value: 'StandardGamesOnly' },
        { text: 'On (Including Counter Picks)', value: 'All' }
      ]
    };
  },

  methods: {
    onShow() {
      this.localPublisherSettings = this.publishers.map((p) => ({
        publisherID: p.publisherID,
        publisherName: p.publisherName,
        autoDraftMode: p.autoDraftMode,
        onlyDraftFromWatchlist: p.onlyAutoDraftFromWatchlist
      }));
    },
    onModeChange() {
      for (const settings of this.localPublisherSettings) {
        if (settings.autoDraftMode === 'Off') {
          settings.onlyDraftFromWatchlist = false;
        }
      }
    },
    setAutoDraft() {
      let publisherAutoDraft = {};
      for (const settings of this.localPublisherSettings) {
        publisherAutoDraft[settings.publisherID] = {
          mode: settings.autoDraftMode,
          onlyDraftFromWatchlist: settings.onlyDraftFromWatchlist
        };
      }

      const model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        publisherAutoDraft
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
