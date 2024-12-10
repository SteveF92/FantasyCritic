<template>
  <b-modal id="renewLeagueForNewYear" ref="renewLeagueForNewYearRef" title="Renew League for New Year" size="lg" @hidden="clearData">
    <div class="alert alert-info">
      Renewing a league for {{ conference.activeYear }} will use the settings you have chosen for the Primary League.
      <br />
      <br />
      By default, the league will keep the same players as were in it last year. You can adjust this before you start the draft.
    </div>

    <b-alert variant="danger" :show="!!errorInfo">{{ errorInfo }}</b-alert>

    <div class="form-group" v-if="renewableLeagues">
      <label for="league" class="control-label">League</label>
      <b-form-select v-model="selectedLeague">
        <option v-for="league in renewableLeagues" :key="league.leagueID" :value="league">
          {{ league.leagueName }}
        </option>
      </b-form-select>
    </div>

    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Renew League" @click="renewLeague" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';
import ConferenceMixin from '@/mixins/conferenceMixin.js';

export default {
  mixins: [ConferenceMixin],
  data() {
    return {
      renewableLeagues: null,
      selectedLeague: null,
      errorInfo: null
    };
  },
  async mounted() {
    this.clearData();
    const response = await axios.get('/api/Conference/GetConference/' + this.conference.conferenceID);
    const leaguesAlreadyRenewed = this.conferenceYear.leagueYears.map((x) => x.leagueID);
    this.renewableLeagues = response.data.leaguesInConference.filter((x) => !leaguesAlreadyRenewed.includes(x.leagueID));
  },
  methods: {
    async renewLeague() {
      const model = {
        conferenceID: this.conference.conferenceID,
        year: this.conferenceYear.year,
        leagueID: this.selectedLeague.leagueID
      };

      try {
        await axios.post('/api/Conference/AddNewLeagueYear', model);
        this.$refs.renewLeagueForNewYearRef.hide();
        await this.notifyAction(`League has been renewed for ${this.conferenceYear.year}.`);
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.errorInfo = null;
    }
  }
};
</script>
