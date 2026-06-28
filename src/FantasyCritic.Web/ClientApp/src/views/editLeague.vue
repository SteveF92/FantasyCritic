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
          <template v-if="!isMultiDraft && firstDraft" #draft-settings>
            <hr />
            <h3>Draft Settings</h3>
            <DraftCreationSettings
              v-model="firstDraftAsList"
              :standard-games="leagueYearSettings.standardGames"
              game-mode="Standard"
              edit-mode>
            </DraftCreationSettings>
          </template>
        </leagueYearSettings>
      </div>

      <div v-show="!leagueYearIsValid" class="alert alert-warning disclaimer">Some of your settings are invalid.</div>

      <div class="form-group">
        <b-button class="col-10 offset-1" variant="primary" :disabled="!leagueYearIsValid" @click="postRequest">Confirm Settings</b-button>
      </div>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import LeagueYearSettings from '@/components/leagueYearSettings.vue';
import DraftCreationSettings from '@/components/DraftCreationSettings.vue';

export default {
  components: {
    LeagueYearSettings,
    DraftCreationSettings
  },
  props: {
    leagueid: { type: String, required: true },
    year: { type: Number, required: true }
  },
  data() {
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
    leagueYearIsValid() {
      if (!this.leagueYearSettings) return false;
      const settingsOk =
        this.leagueYearSettings.standardGames >= 1 &&
        this.leagueYearSettings.standardGames <= 50 &&
        this.leagueYearSettings.counterPicks >= 0 &&
        this.leagueYearSettings.counterPicks <= 20;
      const draftOk = this.isMultiDraft || (
        this.firstDraft &&
        this.firstDraft.gamesToDraft >= 1 &&
        this.firstDraft.counterPicksToDraft >= 0
      );
      return settingsOk && draftOk;
    },
    firstDraftAsList: {
      get() { return this.firstDraft ? [this.firstDraft] : []; },
      set(val) { this.firstDraft = val[0] ?? null; }
    },
    activePlayersInLeague() {
      if (!this.leagueYear || !this.leagueYear.players) {
        return null;
      }
      return this.leagueYear.players.length;
    },
    isMultiDraft() {
      return (this.leagueYear?.drafts?.length ?? 0) >= 2;
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
    fetchLeagueYear() {
      axios
        .get('/api/League/GetLeagueYear?leagueID=' + this.leagueid + '&year=' + this.year)
        .then((response) => {
          this.leagueYear = response.data;
          if (!this.isMultiDraft && response.data.drafts && response.data.drafts.length > 0) {
            const d = response.data.drafts[0];
            this.firstDraft = {
              name: d.name,
              scheduledDate: d.scheduledDate ?? null,
              gamesToDraft: d.gamesToDraft,
              counterPicksToDraft: d.counterPicksToDraft,
            };
          }
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    fetchCurrentLeagueYearOptions() {
      axios
        .get('/api/League/GetLeagueYearOptions?leagueID=' + this.leagueid + '&year=' + this.year)
        .then((response) => {
          this.leagueYearSettings = response.data;
          this.leagueYearName = response.data.leagueYearName ?? null;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    postRequest() {
      const payload = {
        leagueID: this.leagueid,
        year: this.year,
        leagueYearName: this.leagueYearName || null,
        leagueYearSettings: this.leagueYearSettings,
        firstDraft: this.isMultiDraft ? null : this.firstDraft,
      };
      axios.post('/api/leagueManager/EditLeagueYearSettings', payload).then(this.responseHandler).catch(this.catchHandler);
    },
    responseHandler() {
      this.$router.push({ name: 'league', params: { leagueid: this.leagueid, year: this.year } });
    },
    catchHandler(returnedError) {
      this.errorInfo = returnedError.response.data;
      window.scroll({
        top: 0,
        left: 0,
        behavior: 'smooth'
      });
    }
  }
};
</script>
<style scoped>
.league-options {
  margin-bottom: 10px;
}
</style>
