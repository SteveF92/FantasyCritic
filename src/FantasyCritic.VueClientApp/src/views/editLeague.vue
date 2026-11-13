<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Edit League Year Settings</h1>
    <hr />
    <div v-show="errorInfo" class="alert alert-danger">
      <h2>Error!</h2>
      <p>{{ errorInfo }}</p>
    </div>

    <div v-if="leagueYearSettings && leagueYear">
      <div class="text-well league-options">
        <div class="form-group">
          <label for="leagueYearName" class="control-label">League Year Name (Optional)</label>
          <input id="leagueYearName" v-model="leagueYearName" name="League Year Name" type="text" class="form-control input" placeholder="Leave blank to use the league name" />
          <p>If set, this name will be displayed instead of the league name for this specific year only.</p>
        </div>
        <leagueYearSettings
          v-model="leagueYearSettings"
          :year="year"
          edit-mode
          :current-number-of-players="activePlayersInLeague"
          :fresh-settings="freshSettings"
          :is-multi-draft="isMultiDraft"
          :league-id="leagueid">
          <template v-if="!isMultiDraft && firstDraft && !oneShotMode" #draft-settings>
            <hr />
            <h3>Draft Settings</h3>
            <DraftCreationSettings
              v-model="firstDraftAsList"
              :standard-games="leagueYearSettings.standardGames"
              game-mode="Standard"
              edit-mode
              :bids-only-before-next-scheduled-draft="leagueYearSettings.bidsOnlyBeforeNextScheduledDraft"></DraftCreationSettings>
          </template>
        </leagueYearSettings>
      </div>

      <b-alert v-if="bidsOnlyBeforeNextDraftScheduleError" variant="warning" :model-value="true">{{ bidsOnlyBeforeNextDraftScheduleError }}</b-alert>
      <div v-else-if="!leagueYearIsValid" class="alert alert-warning disclaimer">Some of your settings are invalid.</div>

      <div class="form-group">
        <b-button class="col-10 offset-1" variant="primary" :disabled="!leagueYearIsValid" @click="postRequest">Confirm Settings</b-button>
      </div>
    </div>
  </div>
</template>
<script lang="ts">
import { defineComponent } from 'vue';

import { leagueClient, leagueManagerClient, type DraftSettingsRequest, type EditLeagueYearRequest, type LeagueYearSettingsViewModel, type LeagueYearViewModel } from '@/api/clients';
import { getApiErrorMessage } from '@/utilities/apiError';
import { buildOneShotDraft } from '@/utilities/leagueCreationPresets';
import DraftCreationSettings from '@/components/DraftCreationSettings.vue';
import LeagueYearSettings from '@/components/leagueYearSettings.vue';

interface EditLeagueData {
  errorInfo: string;
  leagueYearSettings: LeagueYearSettingsViewModel | null;
  leagueYearName: string | null;
  leagueYear: LeagueYearViewModel | null;
  firstDraft: DraftSettingsRequest | null;
  freshSettings: boolean;
}

export default defineComponent({
  components: {
    LeagueYearSettings,
    DraftCreationSettings
  },
  props: {
    leagueid: { type: String, required: true },
    year: { type: Number, required: true }
  },
  data(): EditLeagueData {
    return {
      errorInfo: '',
      leagueYearSettings: null,
      leagueYearName: null,
      leagueYear: null,
      firstDraft: null,
      freshSettings: false
    };
  },
  computed: {
    bidsOnlyBeforeNextDraftScheduleError(): string | null {
      if (!this.leagueYearSettings?.bidsOnlyBeforeNextScheduledDraft) return null;
      if (!this.isMultiDraft) return null;
      const drafts = this.leagueYear?.drafts ?? [];
      const draftsMissingDate = drafts.slice(1).some((d) => !d.scheduledDate);
      if (draftsMissingDate) {
        return "'Only allow bids before next scheduled draft' is enabled — all drafts after the first must have a scheduled date.";
      }
      return null;
    },
    leagueYearIsValid(): boolean {
      if (!this.leagueYearSettings) return false;
      const settingsOk =
        this.leagueYearSettings.standardGames >= 1 && this.leagueYearSettings.standardGames <= 50 && this.leagueYearSettings.counterPicks >= 0 && this.leagueYearSettings.counterPicks <= 20;
      const draftOk = this.isMultiDraft || this.oneShotMode || (this.firstDraft && this.firstDraft.gamesToDraft >= 1 && this.firstDraft.counterPicksToDraft >= 0);
      return settingsOk && draftOk && !this.bidsOnlyBeforeNextDraftScheduleError;
    },
    oneShotMode(): boolean {
      return this.leagueYear?.settings?.oneShotMode ?? false;
    },
    firstDraftAsList: {
      get(): DraftSettingsRequest[] {
        return this.firstDraft ? [this.firstDraft] : [];
      },
      set(val: DraftSettingsRequest[]) {
        this.firstDraft = val[0] ?? null;
      }
    },
    activePlayersInLeague(): number | null {
      if (!this.leagueYear || !this.leagueYear.players) {
        return null;
      }
      return this.leagueYear.players.length;
    },
    isMultiDraft(): boolean {
      return (this.leagueYear?.drafts?.length ?? 0) >= 2;
    }
  },
  watch: {
    leagueYearSettings: {
      deep: true,
      handler() {
        this.syncOneShotDraft();
      }
    },
    oneShotMode() {
      this.syncOneShotDraft();
    }
  },
  created() {
    this.freshSettings = false;
    if (this.$route.query.freshSettings) {
      this.freshSettings = this.$route.query.freshSettings;
    }
    this.fetchCurrentLeagueYearOptions();
    this.fetchLeagueYear();
  },
  methods: {
    syncOneShotDraft(): void {
      if (!this.oneShotMode || !this.leagueYearSettings) {
        return;
      }
      this.firstDraft = buildOneShotDraft(this.leagueYearSettings.standardGames, this.leagueYearSettings.counterPicks, this.firstDraft ?? undefined);
    },
    fetchLeagueYear(): void {
      leagueClient
        .getLeagueYear(this.leagueid, this.year, undefined)
        .then((leagueYear) => {
          this.leagueYear = leagueYear;
          if (!this.isMultiDraft && leagueYear.drafts && leagueYear.drafts.length > 0) {
            const d = leagueYear.drafts[0];
            this.firstDraft = {
              name: d.name,
              scheduledDate: d.scheduledDate ?? null,
              gamesToDraft: d.gamesToDraft,
              counterPicksToDraft: d.counterPicksToDraft
            };
          }
          this.syncOneShotDraft();
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    fetchCurrentLeagueYearOptions(): void {
      leagueClient
        .getLeagueYearOptions(this.leagueid, this.year)
        .then((leagueYearSettings) => {
          this.leagueYearSettings = leagueYearSettings;
          this.leagueYearName = leagueYearSettings.leagueYearName ?? null;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    postRequest(): void {
      const request: EditLeagueYearRequest = {
        leagueID: this.leagueid,
        year: this.year,
        leagueYearName: this.leagueYearName || null,
        leagueYearSettings: this.leagueYearSettings,
        firstDraft: this.isMultiDraft ? null : this.firstDraft
      };
      leagueManagerClient.editLeagueYearSettings(request).then(this.responseHandler).catch(this.catchHandler);
    },
    responseHandler(): void {
      this.$router.push({ name: 'league', params: { leagueid: this.leagueid, year: this.year } });
    },
    catchHandler(returnedError: unknown): void {
      this.errorInfo = getApiErrorMessage(returnedError);
      window.scroll({
        top: 0,
        left: 0,
        behavior: 'smooth'
      });
    }
  }
});
</script>
<style scoped>
.league-options {
  margin-bottom: 10px;
}
</style>
