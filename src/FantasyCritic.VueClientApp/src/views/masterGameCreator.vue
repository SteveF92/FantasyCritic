<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Create Master Game</h1>
      <b-button variant="info" :to="{ name: 'activeMasterGameRequests' }">View master game requests</b-button>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
    </div>
    <hr />
    <div v-if="createdGame" class="alert alert-success">
      Master Game created: {{ createdGame.masterGameID }}
      <b-button variant="info" size="sm" @click="copyMasterGameID">Copy</b-button>
      <b-button variant="warning" :to="{ name: 'masterGameEditor', params: { mastergameid: createdGame.masterGameID } }">Edit Master Game</b-button>
    </div>
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div v-if="openCriticLink || steamID || ggLink">
      <h2>Links</h2>
      <ul>
        <li>
          <a v-if="openCriticLink" :href="openCriticLink" target="_blank">
            OpenCritic Link
            <font-awesome-icon icon="external-link-alt" />
          </a>
          <span v-else>No OpenCritic Link</span>
        </li>
        <li>
          <a v-if="steamID" :href="steamLink" target="_blank">
            Steam Link
            <font-awesome-icon icon="external-link-alt" />
          </a>
          <span v-else>No Steam Link</span>
        </li>
        <li>
          <a v-if="ggLink" :href="ggLink" target="_blank">
            GG| Link
            <font-awesome-icon icon="external-link-alt" />
          </a>
          <span v-else>No GG| Link</span>
        </li>
      </ul>
      <hr />
    </div>
    <div v-if="request?.requestNote" class="row">
      <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
        <h2>Request Note</h2>
        <p>{{ request.requestNote }}</p>
      </div>
      <hr />
    </div>
    <div v-if="requestingPlayerLeagues.length > 0" class="row">
      <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
        <h3>League Options for Requesting Player</h3>
        <ul>
          <li v-for="(league, index) in requestingPlayerLeagues" :key="league.key">
            League {{ index + 1 }}
            <router-link :to="{ name: 'league', params: { leagueid: league.options.leagueID, year: league.options.year } }" class="league-link">(Link)</router-link>
            <template v-if="league.supportedYear">
              <span v-b-modal="`leagueOptionsModal_${league.key}`" class="fake-link action">(Settings)</span>
              <leagueOptionsModal :league-year-options="league.options" :supported-year="league.supportedYear"></leagueOptionsModal>
            </template>
          </li>
        </ul>
      </div>
      <hr />
    </div>
    <div class="row">
      <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
        <ValidationForm v-slot="{ meta }" as="">
          <form @submit.prevent="createMasterGame">
            <div class="form-group">
              <label for="gameName" class="control-label">Game Name</label>
              <ValidationField v-slot="{ errors }" :model-value="gameName" name="Game Name" rules="required">
                <input id="gameName" v-model="gameName" name="Game Name" type="text" class="form-control input" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationField>
            </div>

            <div class="form-group">
              <label for="releaseDate" class="control-label">Release Date</label>
              <dateInput id="releaseDate" v-model="releaseDate" />
            </div>

            <b-button variant="info" size="sm" @click="propagateDate">Propagate Date</b-button>
            <b-button variant="info" size="sm" @click="parseEstimatedReleaseDate">Parse Estimated</b-button>
            <b-button variant="warning" size="sm" @click="clearDates">Clear Dates</b-button>

            <div class="form-group">
              <label for="estimatedReleaseDate" class="control-label">Estimated Release Date</label>
              <input id="estimatedReleaseDate" v-model="estimatedReleaseDate" name="estimatedReleaseDate" class="form-control input" />
            </div>
            <div class="form-group">
              <label for="minimumReleaseDate" class="control-label">Minimum Release Date</label>
              <dateInput id="minimumReleaseDate" v-model="minimumReleaseDate" />
            </div>
            <div class="form-group">
              <label for="maximumReleaseDate" class="control-label">Maximum Release Date</label>
              <dateInput id="maximumReleaseDate" v-model="maximumReleaseDate" />
            </div>
            <div class="form-group">
              <label for="earlyAccessReleaseDate" class="control-label">Early Access Release Date</label>
              <dateInput id="earlyAccessReleaseDate" v-model="earlyAccessReleaseDate" />
            </div>
            <div class="form-group">
              <label for="internationalReleaseDate" class="control-label">International Release Date</label>
              <dateInput id="internationalReleaseDate" v-model="internationalReleaseDate" />
            </div>

            <div class="form-group">
              <label for="openCriticID" class="control-label">OpenCritic ID</label>
              <input id="openCriticID" v-model="openCriticID" name="openCriticID" type="number" class="form-control input" />
            </div>

            <div class="form-group">
              <label for="ggToken" class="control-label">GG| Token</label>
              <input id="ggToken" v-model="ggToken" name="ggToken" class="form-control input" />
            </div>

            <h3>Tags</h3>
            <masterGameTagSelector v-model="tags" include-system></masterGameTagSelector>

            <div class="form-group">
              <label for="notes" class="control-label">Other Notes</label>
              <input id="notes" v-model="notes" name="notes" class="form-control input" />
            </div>

            <div class="form-group">
              <div class="col-md-offset-2 col-md-4">
                <input type="submit" class="btn btn-primary" value="Submit" :disabled="!meta.valid || isBusy" />
              </div>
            </div>
          </form>
        </ValidationForm>
      </div>
    </div>
  </div>
</template>
<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';

import {
  factCheckerClient,
  type CreateMasterGameRequest,
  type LeagueYearSettingsResponse,
  type MasterGameRequestViewModel,
  type MasterGameTagViewModel,
  type MasterGameViewModel,
  type SupportedYearViewModel
} from '@/api/clients';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { getGGLinkForGame } from '@/utilities/ggLinks';
import { openCriticLink } from '@/utilities/openCriticLinks';
import DateInput from '@/components/dateInput.vue';
import MasterGameTagSelector from '@/components/masterGameTagSelector.vue';
import LeagueOptionsModal from '@/components/modals/leagueOptionsModal.vue';

interface RequestingPlayerLeague {
  key: string;
  options: LeagueYearSettingsResponse;
  //Undefined when the league's year is no longer a supported year, so there are no settings to show.
  supportedYear: SupportedYearViewModel | undefined;
}

interface MasterGameCreatorData {
  createdGame: MasterGameViewModel | null;
  isBusy: boolean;
  errorInfo: string;
  request: MasterGameRequestViewModel | null;
  //A number input's value: '' when empty.
  openCriticID: number | '';
  ggToken: string;
  gameName: string;
  estimatedReleaseDate: string;
  minimumReleaseDate: string | null;
  maximumReleaseDate: string | null;
  earlyAccessReleaseDate: string | null;
  internationalReleaseDate: string | null;
  releaseDate: string | null;
  notes: string;
  tags: MasterGameTagViewModel[];
}

export default defineComponent({
  components: {
    DateInput,
    MasterGameTagSelector,
    LeagueOptionsModal
  },
  data(): MasterGameCreatorData {
    return {
      createdGame: null,
      isBusy: false,
      errorInfo: '',
      request: null,
      openCriticID: '',
      ggToken: '',
      gameName: '',
      estimatedReleaseDate: '',
      minimumReleaseDate: null,
      maximumReleaseDate: null,
      earlyAccessReleaseDate: null,
      internationalReleaseDate: null,
      releaseDate: null,
      notes: '',
      tags: []
    };
  },
  computed: {
    ...mapState(useInterLeagueStore, ['supportedYears']),
    steamID(): number | undefined {
      return this.request?.steamID;
    },
    openCriticLink(): string | undefined {
      return openCriticLink({ openCriticID: this.openCriticID || null });
    },
    steamLink(): string {
      return `https://store.steampowered.com/app/${this.steamID}`;
    },
    ggLink(): string | undefined {
      return getGGLinkForGame({ ggToken: this.ggToken }) ?? undefined;
    },
    requestingPlayerLeagues(): RequestingPlayerLeague[] {
      if (!this.request) {
        return [];
      }

      return this.request.leagueOptionsForRequestingPlayer.map((options) => ({
        key: `${options.leagueID}-${options.year}`,
        options,
        supportedYear: this.supportedYears.find((x) => x.year === options.year)
      }));
    }
  },
  async created() {
    await this.fetchRequest();
    await this.parseEstimatedReleaseDate();
  },
  methods: {
    async createMasterGame(): Promise<void> {
      const ggToken = this.ggToken.trim();
      const request: CreateMasterGameRequest = {
        gameName: this.gameName,
        estimatedReleaseDate: this.estimatedReleaseDate,
        minimumReleaseDate: this.minimumReleaseDate ?? undefined,
        maximumReleaseDate: this.maximumReleaseDate ?? undefined,
        earlyAccessReleaseDate: this.earlyAccessReleaseDate ?? undefined,
        internationalReleaseDate: this.internationalReleaseDate ?? undefined,
        releaseDate: this.releaseDate ?? undefined,
        openCriticID: this.openCriticID === '' ? undefined : this.openCriticID,
        ggToken: ggToken || undefined,
        tags: this.tags.map((x) => x.name),
        notes: this.notes,
        //The server's defaults for a new game.
        syncWithExternalAPIs: true,
        useSimpleEligibility: false,
        delayContention: false,
        showNote: false
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        this.createdGame = await factCheckerClient.createMasterGame(request);
        window.scroll({
          top: 0,
          left: 0,
          behavior: 'smooth'
        });
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when creating the master game.');
      } finally {
        this.isBusy = false;
      }
    },
    async copyMasterGameID(): Promise<void> {
      if (!this.createdGame) {
        return;
      }

      await navigator.clipboard.writeText(this.createdGame.masterGameID);
    },
    async parseEstimatedReleaseDate(): Promise<void> {
      if (this.releaseDate || !this.estimatedReleaseDate) {
        return;
      }

      try {
        const parsed = await factCheckerClient.parseEstimatedDate(this.estimatedReleaseDate);
        this.minimumReleaseDate = parsed.minimumReleaseDate ?? null;
        this.maximumReleaseDate = parsed.maximumReleaseDate ?? null;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when parsing the estimated release date.');
      }
    },
    async fetchRequest(): Promise<void> {
      const requestID = this.$route.query.requestID;
      if (typeof requestID !== 'string') {
        return;
      }

      try {
        const request = await factCheckerClient.getMasterGameRequest(requestID);
        this.request = request;
        this.gameName = request.gameName;
        this.estimatedReleaseDate = request.estimatedReleaseDate;
        if (request.releaseDate) {
          this.releaseDate = request.releaseDate;
          this.minimumReleaseDate = request.releaseDate;
          this.maximumReleaseDate = request.releaseDate;
        }
        this.openCriticID = request.openCriticID ?? '';
        this.ggToken = request.ggToken ?? '';
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when loading the request.');
      }
    },
    propagateDate(): void {
      this.maximumReleaseDate = this.releaseDate;
      this.minimumReleaseDate = this.releaseDate;
      this.estimatedReleaseDate = this.releaseDate ?? '';
    },
    clearDates(): void {
      this.releaseDate = null;
      this.minimumReleaseDate = null;
      this.maximumReleaseDate = null;
      this.estimatedReleaseDate = '';
      this.internationalReleaseDate = null;
      this.earlyAccessReleaseDate = null;
    }
  }
});
</script>
<style scoped>
.select-cell {
  text-align: center;
}

.eligibility-explanation {
  margin-bottom: 50px;
  max-width: 1300px;
}

.eligibility-section {
  margin-bottom: 30px;
}

.eligibility-description {
  margin-top: 25px;
}

.checkbox-label {
  padding-left: 25px;
}

label {
  font-size: 18px;
}

.league-link {
  margin-right: 5px;
}
</style>
