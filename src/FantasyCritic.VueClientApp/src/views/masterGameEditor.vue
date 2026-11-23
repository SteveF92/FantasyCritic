<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Edit Master Game</h1>
      <b-button variant="info" :to="{ name: 'activeMasterGameChangeRequests' }">View master change game requests</b-button>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
      <b-button variant="info" size="sm" @click="generateSQL()">Generate SQL</b-button>
    </div>
    <hr />
    <b-alert v-if="edited" variant="success" :model-value="true">Master Game edited successfully!</b-alert>
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div v-if="generatedSQL">
      <h3>Generated SQL</h3>
      <div class="row">
        <div class="col-xl-8 col-lg-10 col-md-12 text-well">
          <div class="form-group">
            <label for="generatedSQL" class="control-label">GeneratedSQL</label>
            <input id="generatedSQL" v-model="generatedSQL" name="generatedSQL" class="form-control input" />
          </div>
        </div>
      </div>
    </div>
    <div v-if="masterGame && form">
      <h2>{{ masterGame.gameName }}</h2>
      <router-link class="text-primary" :to="{ name: 'mastergame', params: { mastergameid: masterGame.masterGameID } }"><strong>View full details</strong></router-link>
      <div v-if="changeRequest" class="row">
        <div class="text-well">
          <h2>Request Note</h2>
          <p>{{ changeRequest.requestNote }}</p>
        </div>
      </div>
      <hr />

      <div class="row">
        <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
          <ValidationForm v-slot="{ meta }" as="">
            <form @submit.prevent="sendEditMasterGameRequest">
              <div class="form-group">
                <label for="gameName" class="control-label">Game Name</label>
                <ValidationField v-slot="{ errors }" :model-value="form.gameName" name="Game Name" rules="required">
                  <input id="gameName" v-model="form.gameName" name="Game Name" type="text" class="form-control input" aria-describedby="gameName-error" />
                  <span id="gameName-error" class="text-danger">{{ errors[0] }}</span>
                </ValidationField>
              </div>

              <div class="form-group">
                <label for="releaseDate" class="control-label">Release Date</label>
                <dateInput id="releaseDate" v-model="form.releaseDate" />
              </div>

              <b-button variant="info" size="sm" @click="propagateDate">Propagate Date</b-button>
              <b-button variant="info" size="sm" @click="parseEstimatedReleaseDate">Parse Estimated</b-button>
              <b-button variant="warning" size="sm" @click="clearDates">Clear Dates</b-button>

              <div class="form-group">
                <label for="estimatedReleaseDate" class="control-label">Estimated Release Date</label>
                <input id="estimatedReleaseDate" v-model="form.estimatedReleaseDate" name="estimatedReleaseDate" class="form-control input" />
              </div>
              <div class="form-group">
                <label for="minimumReleaseDate" class="control-label">Minimum Release Date</label>
                <ValidationField v-slot="{ errors }" :model-value="form.minimumReleaseDate" name="Minimum Release Date" rules="required">
                  <dateInput id="minimumReleaseDate" v-model="form.minimumReleaseDate" aria-describedby="minimumReleaseDate-error" />
                  <span id="minimumReleaseDate-error" class="text-danger">{{ errors[0] }}</span>
                </ValidationField>
              </div>
              <div class="form-group">
                <label for="maximumReleaseDate" class="control-label">Maximum Release Date</label>
                <dateInput id="maximumReleaseDate" v-model="form.maximumReleaseDate" />
              </div>
              <div class="form-group">
                <label for="earlyAccessReleaseDate" class="control-label">Early Access Release Date</label>
                <dateInput id="earlyAccessReleaseDate" v-model="form.earlyAccessReleaseDate" />
              </div>
              <div class="form-group">
                <label for="internationalReleaseDate" class="control-label">International Release Date</label>
                <dateInput id="internationalReleaseDate" v-model="form.internationalReleaseDate" />
              </div>
              <div class="form-group">
                <label for="announcementDate" class="control-label">Announcement Date</label>
                <dateInput id="announcementDate" v-model="form.announcementDate" />
              </div>

              <div class="form-group">
                <label for="openCriticID" class="control-label">OpenCritic ID</label>
                <input id="openCriticID" v-model="form.openCriticID" name="openCriticID" type="number" class="form-control input" />
              </div>

              <div v-if="masterGame.criticScore" class="form-group">
                <label for="criticScore" class="control-label">Critic Score</label>
                <input id="criticScore" :value="masterGame.criticScore" name="criticScore" class="form-control input" disabled />
                <b-form-checkbox v-model="clearCriticScore">
                  <span class="checkbox-label">Clear Critic Score?</span>
                </b-form-checkbox>
              </div>

              <div class="form-group">
                <label for="ggToken" class="control-label">GG Token</label>
                <input id="ggToken" v-model="form.ggToken" name="ggToken" class="form-control input" />
              </div>

              <h3>Tags</h3>
              <masterGameTagSelector v-model="tags" include-system></masterGameTagSelector>

              <div class="form-group">
                <label for="notes" class="control-label">Other Notes</label>
                <input id="notes" v-model="form.notes" name="notes" class="form-control input" />
              </div>

              <h3>Other</h3>
              <div class="form-group">
                <b-form-checkbox v-model="form.syncWithExternalAPIs">
                  <span class="checkbox-label">Sync with OpenCritic and GG|</span>
                </b-form-checkbox>
              </div>
              <div class="form-group">
                <b-form-checkbox v-model="form.useSimpleEligibility">
                  <span class="checkbox-label">Use Simple Eligibility</span>
                </b-form-checkbox>
              </div>
              <div class="form-group">
                <b-form-checkbox v-model="form.delayContention">
                  <span class="checkbox-label">Delay in Contention</span>
                </b-form-checkbox>
              </div>
              <div class="form-group">
                <b-form-checkbox v-model="form.showNote">
                  <span class="checkbox-label">Show Note on League Page</span>
                </b-form-checkbox>
              </div>
              <div class="form-group">
                <label for="boxartFileName" class="control-label">Box Art File Name</label>
                <input id="boxartFileName" :value="masterGame.boxartFileName" name="boxartFileName" class="form-control input" disabled />
              </div>

              <b-alert v-if="clearCriticScore && form.syncWithExternalAPIs" variant="warning" :model-value="true">
                Are you sure you want to clear the critic score while leaving OpenCritic Score Refreshing on?
              </b-alert>

              <div class="submit-button">
                <input type="submit" class="btn btn-primary" value="Submit" :disabled="!meta.valid || isBusy" />
                <b-form-checkbox v-model="minorEdit">
                  <span class="checkbox-label">Minor Edit (Will not push to Discord)</span>
                </b-form-checkbox>
              </div>
            </form>
          </ValidationForm>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts">
import { defineComponent } from 'vue';

import { factCheckerClient, gameClient, type EditMasterGameRequest, type MasterGameChangeRequestViewModel, type MasterGameTagViewModel, type MasterGameViewModel } from '@/api/clients';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import DateInput from '@/components/dateInput.vue';
import MasterGameTagSelector from '@/components/masterGameTagSelector.vue';

//The editable fields. Dates are null when cleared, as dateInput gives them.
interface MasterGameEditForm {
  gameName: string;
  estimatedReleaseDate: string;
  releaseDate: string | null;
  minimumReleaseDate: string | null;
  maximumReleaseDate: string | null;
  earlyAccessReleaseDate: string | null;
  internationalReleaseDate: string | null;
  announcementDate: string | null;
  //A number input's value: '' when empty.
  openCriticID: number | '';
  ggToken: string;
  notes: string;
  syncWithExternalAPIs: boolean;
  useSimpleEligibility: boolean;
  delayContention: boolean;
  showNote: boolean;
}

interface MasterGameEditorData {
  isBusy: boolean;
  errorInfo: string;
  masterGame: MasterGameViewModel | null;
  form: MasterGameEditForm | null;
  changeRequest: MasterGameChangeRequestViewModel | null;
  tags: MasterGameTagViewModel[];
  edited: boolean;
  generatedSQL: string;
  minorEdit: boolean;
  clearCriticScore: boolean;
}

//The server's maximum release date for a game with no known latest date.
const noMaximumReleaseDate = '9999-12-31';

function toForm(masterGame: MasterGameViewModel): MasterGameEditForm {
  return {
    gameName: masterGame.gameName,
    estimatedReleaseDate: masterGame.estimatedReleaseDate,
    releaseDate: masterGame.releaseDate ?? null,
    minimumReleaseDate: masterGame.minimumReleaseDate,
    maximumReleaseDate: masterGame.maximumReleaseDate === noMaximumReleaseDate ? null : masterGame.maximumReleaseDate,
    earlyAccessReleaseDate: masterGame.earlyAccessReleaseDate ?? null,
    internationalReleaseDate: masterGame.internationalReleaseDate ?? null,
    announcementDate: masterGame.announcementDate ?? null,
    openCriticID: masterGame.openCriticID ?? '',
    ggToken: masterGame.ggToken ?? '',
    notes: masterGame.notes ?? '',
    syncWithExternalAPIs: masterGame.syncWithExternalAPIs,
    useSimpleEligibility: masterGame.useSimpleEligibility,
    delayContention: masterGame.delayContention,
    showNote: masterGame.showNote
  };
}

export default defineComponent({
  components: {
    DateInput,
    MasterGameTagSelector
  },
  props: {
    mastergameid: { type: String, required: true }
  },
  data(): MasterGameEditorData {
    return {
      isBusy: false,
      errorInfo: '',
      masterGame: null,
      form: null,
      changeRequest: null,
      tags: [],
      edited: false,
      generatedSQL: '',
      minorEdit: false,
      clearCriticScore: false
    };
  },
  watch: {
    async $route() {
      await this.initializePage();
    }
  },
  async created() {
    await this.initializePage();
  },
  methods: {
    async initializePage(): Promise<void> {
      this.errorInfo = '';
      this.edited = false;
      this.clearCriticScore = false;
      try {
        const masterGame = await gameClient.masterGame(this.mastergameid);
        this.masterGame = masterGame;
        this.form = toForm(masterGame);
        this.populateTags(masterGame);
        await this.fetchChangeRequest();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when loading the master game.');
      }
    },
    async fetchChangeRequest(): Promise<void> {
      const changeRequestID = this.$route.query.changeRequestID;
      if (typeof changeRequestID !== 'string') {
        this.changeRequest = null;
        return;
      }

      this.changeRequest = await factCheckerClient.getMasterGameChangeRequest(changeRequestID);
    },
    populateTags(masterGame: MasterGameViewModel): void {
      const interLeagueStore = useInterLeagueStore();
      this.tags = interLeagueStore.allTags.filter((x) => masterGame.tags.includes(x.name));
    },
    async parseEstimatedReleaseDate(): Promise<void> {
      const form = this.form;
      if (!form || form.releaseDate || !form.estimatedReleaseDate) {
        return;
      }

      try {
        const parsed = await factCheckerClient.parseEstimatedDate(form.estimatedReleaseDate);
        form.minimumReleaseDate = parsed.minimumReleaseDate ?? null;
        form.maximumReleaseDate = parsed.maximumReleaseDate ?? null;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when parsing the estimated release date.');
      }
    },
    propagateDate(): void {
      if (!this.form) {
        return;
      }

      this.form.maximumReleaseDate = this.form.releaseDate;
      this.form.minimumReleaseDate = this.form.releaseDate;
      this.form.estimatedReleaseDate = this.form.releaseDate ?? '';
    },
    clearDates(): void {
      if (!this.form) {
        return;
      }

      this.form.releaseDate = null;
      this.form.minimumReleaseDate = null;
      this.form.maximumReleaseDate = null;
      this.form.estimatedReleaseDate = '';
      this.form.internationalReleaseDate = null;
      this.form.announcementDate = null;
      this.form.earlyAccessReleaseDate = null;
    },
    async sendEditMasterGameRequest(): Promise<void> {
      const form = this.form;
      if (!this.masterGame || !form || !form.minimumReleaseDate) {
        return;
      }

      const ggToken = form.ggToken.trim();
      //No critic score is sent, so the server keeps the game's own. The one shown can be its sub-games' average.
      const request: EditMasterGameRequest = {
        masterGameID: this.masterGame.masterGameID,
        gameName: form.gameName,
        estimatedReleaseDate: form.estimatedReleaseDate,
        releaseDate: form.releaseDate ?? undefined,
        minimumReleaseDate: form.minimumReleaseDate,
        maximumReleaseDate: form.maximumReleaseDate ?? undefined,
        earlyAccessReleaseDate: form.earlyAccessReleaseDate ?? undefined,
        internationalReleaseDate: form.internationalReleaseDate ?? undefined,
        announcementDate: form.announcementDate ?? undefined,
        openCriticID: form.openCriticID === '' ? undefined : form.openCriticID,
        ggToken: ggToken || undefined,
        notes: form.notes,
        tags: this.tags.map((x) => x.name),
        syncWithExternalAPIs: form.syncWithExternalAPIs,
        useSimpleEligibility: form.useSimpleEligibility,
        delayContention: form.delayContention,
        showNote: form.showNote,
        clearCriticScore: this.clearCriticScore,
        minorEdit: this.minorEdit
      };

      this.isBusy = true;
      this.errorInfo = '';
      this.edited = false;
      try {
        this.masterGame = await factCheckerClient.editMasterGame(request);
        this.edited = true;
        this.clearCriticScore = false;
        window.scroll({
          top: 0,
          left: 0,
          behavior: 'smooth'
        });
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when editing the master game.');
      } finally {
        this.isBusy = false;
      }
    },
    generateSQL(): void {
      this.generatedSQL = `select * from tbl_mastergame where MasterGameID = '${this.mastergameid}';`;
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
  padding-left: 5px;
}

label {
  font-size: 18px;
}

.submit-button {
  display: flex;
  align-items: center;
  gap: 15px;
}
</style>
