<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Create a Conference</h1>
    <hr />
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div v-if="possibleLeagueOptions.openYears.length === 0" class="alert alert-warning">
      Unfortunately, conferences cannot be created right now, as the current year is closed and the next year is not open yet. Check Discord for updates.
    </div>
    <template v-else>
      <div class="alert alert-info">Conferences are intended for large groups that do not fit into one league. For more details, see the FAQ page.</div>
      <div class="text-well">
        <h2>Basic Settings</h2>
        <div class="form-group">
          <label for="conferenceName" class="control-label">Conference Name</label>
          <ValidationField v-slot="{ errors }" :model-value="conferenceName" name="Conference Name" rules="required">
            <input id="conferenceName" v-model="conferenceName" name="conferenceName" type="text" class="form-control input" aria-describedby="conferenceName-error" />
            <span id="conferenceName-error" class="text-danger">{{ errors[0] }}</span>
          </ValidationField>
        </div>

        <div class="alert alert-info">
          The first step in setting up a conference is to set up your "primary league". By default, you will be in this league as it's league manager. All other leagues that you create in this
          conference will have the same options as this league automatically set.
        </div>

        <div class="form-group">
          <label for="primaryLeagueName" class="control-label">Primary League Name</label>
          <ValidationField v-slot="{ errors }" :model-value="primaryLeagueName" name="Primary League Name" rules="required">
            <input id="primaryLeagueName" v-model="primaryLeagueName" name="primaryLeagueName" type="text" class="form-control input" aria-describedby="primaryLeagueName-error" />
            <span id="primaryLeagueName-error" class="text-danger">{{ errors[0] }}</span>
          </ValidationField>
        </div>
        <hr />
        <div class="form-group">
          <label for="initialYear" class="control-label">Year to Play</label>
          <p>The best time to start a game is at the beginning of the year, the earlier the better. You are free to start playing as early as the December before the new year begins.</p>
          <select id="initialYear" v-model="initialYear" class="form-control" :disabled="presetSelected">
            <option v-for="possibleYear in possibleLeagueOptions.openYears" :key="possibleYear" :value="possibleYear">{{ possibleYear }}</option>
          </select>
        </div>
      </div>

      <template v-if="readyToSetupLeagueYear && !presetSelected">
        <hr />
        <div class="text-well">
          <leagueCreationPresets v-model="presetInput" player-count-label="How many players do you think will be in each league in this conference?"></leagueCreationPresets>
          <div class="preset-step">
            <b-alert variant="info" :model-value="true" class="preset-step-copy">These settings are just to provide a baseline, you'll be able to fine tune your settings in the next step.</b-alert>
            <b-button variant="primary" :disabled="!playerCountIsValid(presetInput.playerCount)" @click="lockPresets">Select Preset</b-button>
          </div>
        </div>
      </template>

      <template v-if="presetSelected">
        <hr />
        <div class="text-well">
          <div class="preset-step">
            <b-alert variant="info" :model-value="true" class="preset-step-copy">
              The following settings are our recommendation for a
              <span class="selected-preset-option">{{ presetInput.experienceLevel }}</span>
              level,
              <span class="selected-preset-option">{{ presetInput.playerCount }}</span>
              player per league conference in
              <span class="selected-preset-option">{{ presetInput.gameMode }}</span>
              mode, but you are free to change them.
            </b-alert>
            <b-button variant="secondary" @click="unlockPresets">Select a different preset</b-button>
          </div>
          <leagueYearSettings
            v-model="leagueYearSettings"
            :year="initialYear"
            :game-mode="presetInput.gameMode"
            :experience-level="presetInput.experienceLevel"
            :is-multi-draft="presetInput.gameMode === 'Multi Draft'"
            fresh-settings>
            <template v-if="presetInput.gameMode !== 'One Shot'" #draft-settings>
              <hr />
              <h3>Draft Settings</h3>
              <b-alert v-if="presetInput.gameMode === 'Multi Draft'" variant="info" :model-value="true">
                These leagues will have multiple drafts. You need to add a least two now, but you can add more later.
              </b-alert>
              <DraftCreationSettings
                v-model="drafts"
                :standard-games="leagueYearSettings.standardGames"
                :game-mode="presetInput.gameMode"
                :bids-only-before-next-scheduled-draft="leagueYearSettings.bidsOnlyBeforeNextScheduledDraft"></DraftCreationSettings>
              <b-alert v-if="bidsOnlyBeforeNextDraftScheduleError" variant="warning" :model-value="true">{{ bidsOnlyBeforeNextDraftScheduleError }}</b-alert>
            </template>
          </leagueYearSettings>
        </div>

        <div v-if="leagueYearIsValid || leagueYearEverValid">
          <hr />
          <div class="text-well">
            <h2>Other Options</h2>
            <div>
              <b-form-checkbox v-model="customRulesConference">
                <span class="checkbox-label">Custom Rules Conference</span>
                <p>
                  If checked, all leagues in this conference won't affect the site's overall stats. Please use this if you are running a "highly custom" conference, such as a conference where you are
                  picking the
                  <em>worst</em>
                  games of the year. If you want to change a custom rules conference into a regular conference, you'll need to contact us.
                </p>
              </b-form-checkbox>
            </div>
          </div>

          <div class="alert alert-info disclaimer">Reminder: All of these settings can always be changed later.</div>

          <b-alert v-if="bidsOnlyBeforeNextDraftScheduleError" variant="warning" :model-value="true">{{ bidsOnlyBeforeNextDraftScheduleError }}</b-alert>
          <div v-else-if="!readyToSetupLeagueYear" class="alert alert-warning disclaimer">Can't create league. Enter a league name.</div>
          <div v-else-if="!leagueYearIsValid" class="alert alert-warning disclaimer">Can't create league. Some of your settings are invalid.</div>

          <div class="form-group">
            <b-button class="col-10 offset-1" variant="primary" :disabled="!leagueYearIsValid" @click="postRequest">Create Conference</b-button>
          </div>
        </div>
      </template>
    </template>
  </div>
</template>
<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';

import { conferenceClient, type CreateConferenceRequest, type DraftSettingsRequest, type LeagueYearSettingsRequest } from '@/api/clients';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { buildOneShotDraft, computePreset, playerCountIsValid, type GameMode, type PresetInput } from '@/utilities/leagueCreationPresets';
import DraftCreationSettings from '@/components/DraftCreationSettings.vue';
import LeagueCreationPresets from '@/components/leagueCreationPresets.vue';
import LeagueYearSettings from '@/components/leagueYearSettings.vue';

interface CreateConferenceData {
  errorInfo: string;
  conferenceName: string;
  primaryLeagueName: string;
  initialYear: number;
  leagueYearSettings: LeagueYearSettingsRequest;
  drafts: DraftSettingsRequest[];
  presetInput: PresetInput;
  presetSelected: boolean;
  customRulesConference: boolean;
  leagueYearEverValid: boolean;
}

export default defineComponent({
  components: {
    LeagueYearSettings,
    LeagueCreationPresets,
    DraftCreationSettings
  },
  data(): CreateConferenceData {
    const initialYear = useInterLeagueStore().possibleLeagueOptions.openYears[0];
    const defaultGameMode = 'Standard';
    const defaultExperienceLevel = 'Standard';
    //We don't actually want to show a default number of players, but we have to choose something here to get default values for everything.
    //The code is just simpler if we can always assume that leagueYearSettings is valid.
    const defaultNumberOfPlayers = 10;
    const defaultSettings = computePreset(defaultGameMode, defaultExperienceLevel, defaultNumberOfPlayers, initialYear);

    return {
      errorInfo: '',
      conferenceName: '',
      primaryLeagueName: '',
      initialYear: initialYear,
      leagueYearSettings: defaultSettings.settings,
      drafts: defaultSettings.drafts,
      presetInput: { playerCount: undefined, gameMode: defaultGameMode, experienceLevel: defaultExperienceLevel },
      presetSelected: false,
      customRulesConference: false,
      leagueYearEverValid: false
    };
  },
  computed: {
    ...mapState(useInterLeagueStore, ['possibleLeagueOptions']),
    readyToSetupLeagueYear(): boolean {
      return !!this.conferenceName.trim() && !!this.primaryLeagueName.trim();
    },
    bidsOnlyBeforeNextDraftScheduleError(): string | null {
      if (!this.leagueYearSettings.bidsOnlyBeforeNextScheduledDraft) return null;
      if (this.presetInput.gameMode !== 'Multi Draft') return null;
      const draftsMissingDate = this.drafts.slice(1).some((d) => !d.scheduledDate);
      if (draftsMissingDate) {
        return "'Only allow bids before next scheduled draft' is enabled — all drafts after the first must have a scheduled date.";
      }
      return null;
    },
    leagueYearIsValid(): boolean {
      const validNumberOfPlayers = playerCountIsValid(this.presetInput.playerCount);
      if (!validNumberOfPlayers || !this.drafts.length) {
        return false;
      }
      const settingsOk =
        this.leagueYearSettings.standardGames >= 1 && this.leagueYearSettings.standardGames <= 50 && this.leagueYearSettings.counterPicks >= 0 && this.leagueYearSettings.counterPicks <= 20;
      const draftsOk = this.presetInput.gameMode === 'Multi Draft' ? this.drafts.length >= 2 : this.drafts.length >= 1;
      return this.readyToSetupLeagueYear && settingsOk && draftsOk && !this.bidsOnlyBeforeNextDraftScheduleError;
    }
  },
  watch: {
    leagueYearIsValid: function (newValue: boolean) {
      let allValid = this.readyToSetupLeagueYear && newValue;
      if (allValid) {
        this.leagueYearEverValid = true;
      }
    },
    leagueYearSettings: {
      handler() {
        this.syncOneShotDrafts(this.presetInput.gameMode);
      }
    }
  },
  methods: {
    playerCountIsValid,
    syncOneShotDrafts(gameMode: GameMode): void {
      if (gameMode !== 'One Shot') {
        return;
      }
      this.drafts = [buildOneShotDraft(this.leagueYearSettings.standardGames, this.leagueYearSettings.counterPicks)];
    },
    lockPresets() {
      if (!playerCountIsValid(this.presetInput.playerCount)) return;
      const presetValues = computePreset(this.presetInput.gameMode, this.presetInput.experienceLevel, this.presetInput.playerCount, this.initialYear);
      this.leagueYearSettings = presetValues.settings;
      this.drafts = presetValues.drafts;
      this.presetSelected = true;
    },
    unlockPresets() {
      this.presetSelected = false;
    },
    async postRequest(): Promise<void> {
      this.errorInfo = '';
      const request: CreateConferenceRequest = {
        conferenceName: this.conferenceName.trim(),
        primaryLeagueName: this.primaryLeagueName.trim(),
        customRulesConference: this.customRulesConference,
        leagueYearSettings: this.leagueYearSettings,
        drafts: this.drafts
      };

      try {
        const newConferenceID = await conferenceClient.createConference(request);
        this.$router.push({ name: 'conference', params: { conferenceid: newConferenceID, year: this.initialYear } });
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error);
        window.scroll({
          top: 0,
          left: 0,
          behavior: 'smooth'
        });
      }
    }
  }
});
</script>
<style scoped>
label {
  font-size: 18px;
}

.submit-button {
  text-align: right;
}

.preset-step {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem 1rem;
  margin-bottom: 1rem;
}

.preset-step-copy {
  flex: 1 1 20rem;
  margin-bottom: 0;
}

.selected-preset-option {
  padding: 0.1em 0.45em;
  border-radius: 0.25rem;
  background-color: rgba(0, 0, 0, 0.3);
  white-space: nowrap;
}
</style>
